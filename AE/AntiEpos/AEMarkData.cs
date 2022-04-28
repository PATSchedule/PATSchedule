using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
namespace AntiEpos
{
    public class AEMarkData
    {
        [JsonPropertyName("control_form_id")]
        public ulong ControlFormId { get; set; }

        [JsonPropertyName("control_form_name")]
        public string ControlFormName { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("grade_system_type")]
        public string GradeSystemType { get; set; }

        [JsonPropertyName("is_exam")]
        public bool IsExam { get; set; }

        [JsonPropertyName("is_point")]
        public bool IsPoint { get; set; }

        [JsonPropertyName("topic_name")]
        public string TopicName { get; set; }

        [JsonPropertyName("values")]
        public AEMarkValue[]? Values { get; set; }

        [JsonPropertyName("weight")]
        public ulong Weight { get; set; }
    }
}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
