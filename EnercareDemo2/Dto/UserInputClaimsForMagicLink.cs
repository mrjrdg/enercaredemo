using System.Text.Json.Serialization;

namespace EnercareDemo2.Dto
{
    public class UserInputClaimsForMagicLink
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

    }
}
