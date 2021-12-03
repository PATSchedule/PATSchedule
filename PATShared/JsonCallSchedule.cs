using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PATShared
{
    class JsonCallSchedule
    {
        public class JsonCallScheduleRoot
        {
            public string[]? A1 { get; set; }
            public string[]? C { get; set; }
            public string[]? A2 { get; set; }
            public string[]? A3 { get; set; }
            public string[]? T1 { get; set; }
            public string[]? T2 { get; set; }
            public string[]? SUB { get; set; }
            public string[]? P1 { get; set; }
            public string[]? UNK { get; set; }
        }

        public int FileVersion { get; set; } = 0;
        public string? Note { get; set; } = "";
        public JsonCallScheduleRoot Data { get; set; } = new JsonCallScheduleRoot();

        public static JsonCallSchedule? Parse(string thejson)
        {
            try
            {
                return JsonConvert.DeserializeObject<JsonCallSchedule>(thejson);
            }
            catch
            {
                Console.WriteLine("Failed to parse the CallSchedule json.");
                return null;
            }
        }
    }
}
