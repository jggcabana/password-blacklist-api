using System.Text.Json.Serialization;

namespace WTW.SecurityAPI.Models.Request
{
    public class CreatePasswordRequest
    {
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
