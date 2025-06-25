using System.Text.Json;
using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Service to fetch book metadata from the Google Books API.
    /// </summary>
    public class GoogleBooksService
    {
        private readonly HttpClient _http;
        private readonly string? _apiKey;

        public GoogleBooksService(HttpClient http, IConfiguration config)
        {
            _http   = http;
            _apiKey = config["GoogleBooks:ApiKey"];
        }

        /// <summary>
        /// Searches Google Books for a given term and maps results to BookCreateDto.
        /// Note: les AuthorId, GenreId, EditorId, ShelfLevelId et TagIds sont mis à 0 ou vides.
        /// Vous devrez injecter votre propre logique après appel pour les remplir correctement.
        /// </summary>
        public async Task<List<BookCreateDto>> SearchAsync(string term, int maxResults = 20)
        {
            var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(term)}&maxResults={maxResults}";
            if (!string.IsNullOrWhiteSpace(_apiKey))
                url += $"&key={_apiKey}";

            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
            if (!doc.RootElement.TryGetProperty("items", out var items))
                return new List<BookCreateDto>();

            var list = new List<BookCreateDto>();

            foreach (var item in items.EnumerateArray())
            {
                var info = item.GetProperty("volumeInfo");

                // Titre
                var title = info.GetProperty("title").GetString() ?? string.Empty;

                // ISBN (premier identifiant)
                string isbn = string.Empty;
                if (info.TryGetProperty("industryIdentifiers", out var ids))
                {
                    foreach (var id in ids.EnumerateArray())
                    {
                        isbn = id.GetProperty("identifier").GetString() ?? string.Empty;
                        break;
                    }
                }

                // Date de publication
                DateTime pubDate = DateTime.UtcNow;
                if (info.TryGetProperty("publishedDate", out var pd)
                 && DateTime.TryParse(pd.GetString(), out var dt))
                {
                    pubDate = dt;
                }

                // URL de couverture
                string? coverUrl = null;
                if (info.TryGetProperty("imageLinks", out var imgs)
                 && imgs.TryGetProperty("thumbnail", out var thumb))
                {
                    coverUrl = thumb.GetString();
                }

                // Construction du DTO avec Ids à 0
                var dto = new BookCreateDto
                {
                    Title           = title,
                    Isbn            = isbn,
                    PublicationDate = pubDate,
                    AuthorId        = 0,                // TODO : lookup / upsert auteur
                    GenreId         = 0,                // TODO : lookup / upsert genre
                    EditorId        = 0,                // TODO : lookup / upsert éditeur
                    ShelfLevelId    = 0,                // TODO : assign default shelf level
                    TagIds          = new List<int>(),  // TODO : assign tags éventuels
                    CoverUrl        = coverUrl
                };

                list.Add(dto);
            }

            return list;
        }
    }
}
