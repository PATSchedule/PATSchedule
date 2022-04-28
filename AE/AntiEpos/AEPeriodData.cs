using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
namespace AntiEpos
{
    public class AEPeriodData
    {
        [JsonPropertyName("avg_five")]
        public string AverageFive { get; set; }

        [JsonPropertyName("avg_hundred")]
        public string AverageHundred { get; set; }

        [JsonPropertyName("avg_original")]
        public string AverageOriginal { get; set; }

        [JsonPropertyName("end")]
        public string End { get; set; }

        [JsonPropertyName("end_iso")]
        public string EndIso { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("marks")]
        public AEMarkData[]? Marks { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; }

        [JsonPropertyName("start_iso")]
        public string StartIso { get; set; }
    }
}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
