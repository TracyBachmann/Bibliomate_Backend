using BackendBiblioMate.Interfaces;
using System.Text;
using System.Text.Json;

namespace BackendBiblioMate.Services.Infrastructure.External
{
    /// <summary>
    /// Provides a Brevo (Sendinblue) API-based implementation of <see cref="IEmailService"/>.
    /// Uses HTTPS (port 443) instead of SMTP, bypassing common firewall restrictions.
    /// </summary>
    public class BrevoEmailService : IEmailService
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<BrevoEmailService> _log;
        private readonly IHttpClientFactory _httpClientFactory;

        public BrevoEmailService(
            IConfiguration cfg, 
            ILogger<BrevoEmailService> log,
            IHttpClientFactory httpClientFactory)
        {
            _cfg = cfg;
            _log = log;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var apiKey = _cfg["Brevo:ApiKey"] 
                ?? throw new InvalidOperationException("Missing Brevo:ApiKey configuration");
            
            var fromEmail = _cfg["Brevo:FromEmail"] ?? "noreply@bibliomate.fr";
            var fromName = _cfg["Brevo:FromName"] ?? "BiblioMate";

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.brevo.com");
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var payload = new
            {
                sender = new { name = fromName, email = fromEmail },
                to = new[] { new { email = toEmail } },
                subject = subject,
                htmlContent = htmlContent
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("/v3/smtp/email", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _log.LogInformation("✅ Brevo email sent successfully to {To}", toEmail);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _log.LogError("❌ Brevo API error: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    throw new Exception($"Brevo API failed: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "❌ Failed to send Brevo email to {To}", toEmail);
                throw;
            }
        }
    }
}