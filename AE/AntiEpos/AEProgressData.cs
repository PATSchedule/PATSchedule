using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
namespace AntiEpos
{
    public class AEProgressData
    {
        [JsonPropertyName("avg_five")]
        public string AverageFive { get; set; }

        [JsonPropertyName("avg_hundred")]
        public string AverageHundred { get; set; }

        [JsonPropertyName("avg_original")]
        public string AverageOriginal { get; set; }

        [JsonPropertyName("periods")]
        public AEPeriodData[] Periods { get; set; }

        [JsonPropertyName("subject_name")]
        public string SubjectName { get; set; }
    }
}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
