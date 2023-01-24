using System.Text.Json.Serialization;

namespace Authentication.Model
{
    public class LoginResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
    }
}