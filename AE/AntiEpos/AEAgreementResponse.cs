using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntiEpos
{
    public class AEAgreementResponseData
    {
        [JsonPropertyName("agreedUser")]
        public bool AgreedUser { get; set; }

        [JsonPropertyName("agreement")]
        public object? Agreement { get; set; }

        [JsonPropertyName("informing")]
        public bool Informing { get; set; }
    }

    public class AEAgreementResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("data")]
        public AEAgreementResponseData? Data { get; set; }
    }
}
