using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
namespace AntiEpos
{
    public class AEMarkValue
    {
        [JsonPropertyName("five")]
        public double Five { get; set; }

        [JsonPropertyName("hundred")]
        public double Hundred { get; set; }

        [JsonPropertyName("nmax")]
        public double NMax { get; set; }

        [JsonPropertyName("original")]
        public string Original { get; set; }
    }
}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
