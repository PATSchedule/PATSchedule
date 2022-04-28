using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
namespace AntiEpos
{
    public class AESessionsData
    {
        [JsonPropertyName("authentication_token")]
        public string AuthenticationToken { get; set; }

        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("middle_name")]
        public string MiddleName { get; set; }

        [JsonPropertyName("password_change_required")]
        public bool PasswordChangeRequired { get; set; }

        [JsonPropertyName("profiles")]
        public AEProfileData[]? Profiles { get; set; }

        [JsonPropertyName("sex")]
        public string Sex { get; set; } // none :(

        [JsonPropertyName("snils")]
        public string Snils { get; set; } // Individual insurance account number aka Russian SSN
    }
}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
