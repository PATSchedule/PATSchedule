using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PATShared
{
    class JsonBaseSchedule
    {
        public class JsonBaseScheduleLesson
        {
            public string Subject { get; set; } = "";
            public string Room { get; set; } = "";
            public string Teacher { get; set; } = "";
            public int Para { get; set; } = 0;
        }

        public int FileVersion { get; set; } = 0;
        public IDictionary<string, JsonBaseScheduleLesson[][]> Data { get; set; } = new Dictionary<string, JsonBaseScheduleLesson[][]>();
        public string? Note { get; set; } = "";

        public static JsonBaseSchedule? Parse(string thejson)
        {
            try
            {
                return JsonConvert.DeserializeObject<JsonBaseSchedule>(thejson);
            }
            catch
            {
                Console.WriteLine("Failed to parse the BaseSchedule json.");
                return null;
            }
        }
    }
}
