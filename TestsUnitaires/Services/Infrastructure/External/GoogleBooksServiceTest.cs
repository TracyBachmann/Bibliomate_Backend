using System.Net;
using System.Text;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Services.Infrastructure.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestsUnitaires.Services.Infrastructure.External
{
    /// <summary>
    /// Unit tests for <see cref="GoogleBooksService"/>.
    /// Verifies search logic, argument validation, and JSON mapping.
    /// </summary>
    public class GoogleBooksServiceTest
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<GoogleBooksService>> _loggerMock;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Sets up mocks and an HttpClient with a fake handler returning canned JSON.
        /// </summary>
        public GoogleBooksServiceTest()
        {
            // 1) Mock IConfiguration to supply an empty API key
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["GoogleBooks:ApiKey"]).Returns(string.Empty);

            // 2) Mock ILogger<T>
            _loggerMock = new Mock<ILogger<GoogleBooksService>>();

            // 3) Create HttpClient with a fake handler that returns default {"items":[]} JSON
            var handler = new FakeHttpMessageHandler();
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://www.googleapis.com/")
            };
        }

        /// <summary>
        /// Verifies that calling SearchAsync with a null, empty, or whitespace term
        /// throws an <see cref="ArgumentException"/>.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SearchAsync_InvalidTerm_ThrowsArgumentException(string? term)
        {
            // Arrange
            var service = new GoogleBooksService(_httpClient, _configMock.Object, _loggerMock.Object);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync(term!));
        }

        /// <summary>
        /// Verifies that when the API response contains no "items" property,
        /// SearchAsync returns an empty list.
        /// </summary>
        [Fact]
        public async Task SearchAsync_NoItems_ReturnsEmptyList()
        {
            // Arrange: handler returns JSON without "items"
            var handler = new FakeHttpMessageHandler("{\"kind\":\"books#volumes\"}", HttpStatusCode.OK);
            var client  = new HttpClient(handler);
            var service = new GoogleBooksService(client, _configMock.Object, _loggerMock.Object);

            // Act
            var result = await service.SearchAsync("foo");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that valid JSON with one volume is correctly mapped
        /// to a <see cref="BookCreateDto"/>.
        /// </summary>
        [Fact]
        public async Task SearchAsync_ValidJson_MapsToDto()
        {
            // Arrange: sample JSON containing one volumeInfo entry
            const string json = @"
            {
              ""items"": [
                {
                  ""volumeInfo"": {
                    ""title"": ""Test Book"",
                    ""industryIdentifiers"": [
                      { ""type"": ""ISBN_10"", ""identifier"": ""1234567890"" }
                    ],
                    ""publishedDate"": ""2020-05-01"",
                    ""imageLinks"": {
                      ""thumbnail"": ""https://example.com/thumb.jpg""
                    }
                  }
                }
              ]
            }";
            var handler = new FakeHttpMessageHandler(json, HttpStatusCode.OK);
            var client  = new HttpClient(handler);
            var service = new GoogleBooksService(client, _configMock.Object, _loggerMock.Object);

            // Act
            var list = await service.SearchAsync("test");

            // Assert: exactly one DTO returned with correct mapping
            Assert.Single(list);
            var dto = list[0];
            Assert.Equal("Test Book", dto.Title);
            Assert.Equal("1234567890", dto.Isbn);
            Assert.Equal(new DateTime(2020, 5, 1), dto.PublicationDate);
            Assert.Equal("https://example.com/thumb.jpg", dto.CoverUrl);
            Assert.Equal(0, dto.AuthorId);
            Assert.Empty(dto.TagIds!);
        }

        /// <summary>
        /// Fake HTTP handler that returns a predefined HTTP response.
        /// </summary>
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _responseContent;
            private readonly HttpStatusCode _statusCode;

            public FakeHttpMessageHandler(
                string responseContent = "{\"items\":[]}",
                HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _responseContent = responseContent;
                _statusCode      = statusCode;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            }
        }
    }
}