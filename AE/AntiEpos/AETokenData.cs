using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AntiEpos
{
    public class AETokenData
    {
        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; }

        public AETokenData(string authToken)
        {
            AuthToken = authToken;
        }
    }
}
