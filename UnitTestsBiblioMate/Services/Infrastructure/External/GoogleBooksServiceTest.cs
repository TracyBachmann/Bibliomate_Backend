using System.Net;
using System.Text;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Services.Infrastructure.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTestsBiblioMate.Services.Infrastructure.External
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
        /// Constructor initializes configuration and logger mocks,
        /// and provides a default <see cref="HttpClient"/> using a fake handler.
        /// </summary>
        public GoogleBooksServiceTest()
        {
            // 1) Mock IConfiguration → simulate absence of API key
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["GoogleBooks:ApiKey"]).Returns(string.Empty);

            // 2) Mock ILogger for dependency injection
            _loggerMock = new Mock<ILogger<GoogleBooksService>>();

            // 3) Provide a default HttpClient with a fake handler
            //    By default, it returns {"items": []} to simulate empty results
            var handler = new FakeHttpMessageHandler();
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://www.googleapis.com/")
            };
        }

        // ---------------- Argument validation ----------------

        /// <summary>
        /// SearchAsync should throw an ArgumentException when
        /// the search term is null, empty, or whitespace.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SearchAsync_InvalidTerm_ThrowsArgumentException(string? term)
        {
            // Arrange
            var service = new GoogleBooksService(_httpClient, _configMock.Object, _loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync(term!));
        }

        // ---------------- JSON without items ----------------

        /// <summary>
        /// When API response contains no "items", the service
        /// should return an empty list instead of null or exception.
        /// </summary>
        [Fact]
        public async Task SearchAsync_NoItems_ReturnsEmptyList()
        {
            // Arrange: fake handler returns JSON missing the "items" property
            var handler = new FakeHttpMessageHandler("{\"kind\":\"books#volumes\"}", HttpStatusCode.OK);
            var client  = new HttpClient(handler);
            var service = new GoogleBooksService(client, _configMock.Object, _loggerMock.Object);

            // Act
            var result = await service.SearchAsync("foo");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ---------------- Valid JSON mapping ----------------

        /// <summary>
        /// When API returns valid JSON with one volume,
        /// it should be mapped into a <see cref="BookCreateDto"/>.
        /// </summary>
        [Fact]
        public async Task SearchAsync_ValidJson_MapsToDto()
        {
            // Arrange: JSON sample with one volumeInfo block
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

            // Assert: validate DTO mapping
            Assert.Single(list);
            var dto = list[0];
            Assert.Equal("Test Book", dto.Title);
            Assert.Equal("1234567890", dto.Isbn);
            Assert.Equal(new DateTime(2020, 5, 1), dto.PublicationDate);
            Assert.Equal("https://example.com/thumb.jpg", dto.CoverUrl);

            // Fields that are not mapped should remain default
            Assert.Equal(0, dto.AuthorId);
            Assert.Empty(dto.TagIds!);
        }

        // ---------------- Fake handler ----------------

        /// <summary>
        /// Fake <see cref="HttpMessageHandler"/> that intercepts requests
        /// and always returns a canned JSON response.
        /// This avoids external calls to the real Google Books API.
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

            /// <summary>
            /// Simulates sending a request by returning
            /// the predefined JSON and status code.
            /// </summary>
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
