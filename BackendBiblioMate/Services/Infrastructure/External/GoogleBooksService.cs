using System.Text;
using System.Text.Json;
using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Services.Infrastructure.External
{
    /// <summary>
    /// Defines methods to fetch book metadata from the Google Books API.
    /// </summary>
    public interface IGoogleBooksService
    {
        /// <summary>
        /// Searches Google Books for the specified term and returns a list of <see cref="BookCreateDto"/>.
        /// </summary>
        /// <param name="term">The search term (e.g., title, author, ISBN).</param>
        /// <param name="maxResults">Maximum number of results to retrieve. Default is 20.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="List{BookCreateDto}"/> containing book data.
        /// If no items were found, returns an empty list.
        /// </returns>
        Task<List<BookCreateDto>> SearchAsync(
            string term,
            int maxResults = 20,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service implementation to fetch book metadata from the Google Books API.
    /// </summary>
    public class GoogleBooksService : IGoogleBooksService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly ILogger<GoogleBooksService>? _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="GoogleBooksService"/>.
        /// </summary>
        /// <param name="httpClient">Configured <see cref="HttpClient"/> for Google Books API.</param>
        /// <param name="configuration">Application configuration to retrieve the API key.</param>
        /// <param name="logger">Optional logger for diagnostics.</param>
        public GoogleBooksService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleBooksService>? logger = null)
        {
            _httpClient = httpClient;
            _apiKey     = configuration["GoogleBooks:ApiKey"];
            _logger     = logger;
        }

        /// <inheritdoc/>
        public async Task<List<BookCreateDto>> SearchAsync(
            string term,
            int maxResults = 20,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentException("Search term must be provided.", nameof(term));

            var url = BuildRequestUrl(term, maxResults);

            _logger?.LogDebug("Calling Google Books API: {Url}", url);

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("items", out var items))
            {
                _logger?.LogInformation("No items found for term '{Term}'", term);
                return new List<BookCreateDto>();
            }

            var results = new List<BookCreateDto>();
            foreach (var item in items.EnumerateArray())
            {
                try
                {
                    results.Add(MapVolumeToDto(item));
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to map volumeInfo for one item.");
                }
            }

            return results;
        }

        /// <summary>
        /// Builds the Google Books API request URL with query parameters.
        /// </summary>
        /// <param name="term">Search term.</param>
        /// <param name="maxResults">Maximum results to fetch.</param>
        /// <returns>Full request URL string.</returns>
        private string BuildRequestUrl(string term, int maxResults)
        {
            var builder = new StringBuilder(
                $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(term)}" +
                $"&maxResults={maxResults}");

            if (!string.IsNullOrWhiteSpace(_apiKey))
                builder.Append($"&key={_apiKey}");

            return builder.ToString();
        }

        /// <summary>
        /// Maps a single JSON volume element to a <see cref="BookCreateDto"/>.
        /// </summary>
        /// <param name="item">The JSON element representing a volume.</param>
        /// <returns>A populated <see cref="BookCreateDto"/>.</returns>
        private static BookCreateDto MapVolumeToDto(JsonElement item)
        {
            var info = item.GetProperty("volumeInfo");

            // Title
            var title = info.GetProperty("title").GetString() ?? string.Empty;

            // ISBN (take first identifier)
            string isbn = string.Empty;
            if (info.TryGetProperty("industryIdentifiers", out var ids))
            {
                foreach (var idEl in ids.EnumerateArray())
                {
                    isbn = idEl.GetProperty("identifier").GetString() ?? string.Empty;
                    break;
                }
            }

            // Publication date
            DateTime publicationDate = DateTime.UtcNow;
            if (info.TryGetProperty("publishedDate", out var pd)
             && DateTime.TryParse(pd.GetString(), out var dt))
            {
                publicationDate = dt;
            }

            // Cover URL (thumbnail)
            string? coverUrl = null;
            if (info.TryGetProperty("imageLinks", out var imgs)
             && imgs.TryGetProperty("thumbnail", out var thumb))
            {
                coverUrl = thumb.GetString();
            }

            return new BookCreateDto
            {
                Title           = title,
                Isbn            = isbn,
                PublicationDate = publicationDate,
                AuthorId        = 0,               // TODO: upsert author in domain
                GenreId         = 0,               // TODO: upsert genre
                EditorId        = 0,               // TODO: upsert editor
                ShelfLevelId    = 0,               // TODO: assign default shelf level
                TagIds          = new List<int>(), // TODO: populate tags if needed
                CoverUrl        = coverUrl
            };
        }
    }
}
