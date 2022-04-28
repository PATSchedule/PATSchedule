using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
namespace AntiEpos
{
    public class AEProfileData
    {
        [JsonPropertyName("agree_pers_data")]
        public bool AgreePersonalData { get; set; }

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("org_type_id")]
        public ulong OrgTypeId { get; set; } // TODO: unk

        [JsonPropertyName("roles")]
        public object[]? Roles { get; set; } // TODO: unk

        [JsonPropertyName("school_id")]
        public ulong SchoolId { get; set; }

        [JsonPropertyName("school_shortname")]
        public string SchoolShortname { get; set; }

        [JsonPropertyName("subject_ids")]
        public object[]? SubjectIds { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("user_id")]
        public ulong UserId { get; set; }
    }
}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
