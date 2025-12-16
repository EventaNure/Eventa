using System.Text.Json.Serialization;

namespace Eventa.Infrastructure.Responses
{
    public class GoogleTokenResponse
    {
        public string IdToken { get; set; } = string.Empty;
    }
}
