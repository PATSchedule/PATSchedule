using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PATShared
{
    /// <summary>
    /// Расписание для корпуса
    /// </summary>
    public enum Building
    {
        /// <summary>
        /// Корпус А 1 этаж
        /// </summary>
        A1,
        /// <summary>
        /// Корпус С, эквивалентно А1.
        /// </summary>
        C,
        /// <summary>
        /// Корпус А 2 этаж
        /// </summary>
        A2,
        /// <summary>
        /// Корпус А 3 этаж
        /// </summary>
        A3,
        /// <summary>
        /// Корпус Т чёт
        /// </summary>
        T1,
        /// <summary>
        /// Корпус Т нечёт
        /// </summary>
        T2,
        /// <summary>
        /// Суббота
        /// </summary>
        SUB,
        /// <summary>
        /// Пара час
        /// </summary>
        P1,
        /// <summary>
        /// Практика...?
        /// </summary>
        UNK
    }

    public static class Utils
    {
        static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool IsSecondWeek(DateTime dt)
        {
            var m = dt.Month;
            var d = dt.Day;

            // https://sun9-73.userapi.com/AWFuFXvG0766RVLMipZvC6TogNN439GtUeKviQ/Nn8U4CwvRz4.jpg
            // там нельзя просто день поделить на два, там кошмар.
            switch (m)
            {
                case 1:  return IsInRange(d, 1, 2)  || IsInRange(d, 10, 16) || IsInRange(d, 24, 30);
                case 2:  return IsInRange(d, 7, 13) || IsInRange(d, 21, 27);
                case 3:  return IsInRange(d, 7, 13) || IsInRange(d, 21, 27);
                case 4:  return IsInRange(d, 4, 10) || IsInRange(d, 18, 24);
                case 5:  return IsInRange(d, 2, 8)  || IsInRange(d, 16, 22) || IsInRange(d, 30, 31);
                case 6:  return IsInRange(d, 1, 5)  || IsInRange(d, 13, 19) || IsInRange(d, 27, 30);
                case 7:  return IsInRange(d, 1, 3)  || IsInRange(d, 11, 17) || IsInRange(d, 25, 31);
                case 9:  return IsInRange(d, 6, 12) || IsInRange(d, 20, 26);
                case 10: return IsInRange(d, 4, 10) || IsInRange(d, 18, 24);
                case 11: return IsInRange(d, 1, 7)  || IsInRange(d, 15, 21) || IsInRange(d, 29, 30);
                case 12: return IsInRange(d, 1, 5)  || IsInRange(d, 13, 19) || IsInRange(d, 27, 31);
            }

            return false;
        }

        static string[] A1 = new string[0];
        static string[] C = new string[0];
        static string[] A2 = new string[0];
        static string[] A3 = new string[0];
        static string[] T1 = new string[0];
        static string[] T2 = new string[0];
        static string[] SUB = new string[0];
        static string[] P1 = new string[0];
        static string[] UNK = new string[0];

        public static async Task DownloadClockSchedule(HttpClient hc, CancellationToken cts)
        {
            try
            {
                string callscheduleurl = "https://raw.githubusercontent.com/PATSchedule/BaseSchedule/main/CallSchedule.json";

                if (cts.IsCancellationRequested) return;

                string thejson = await hc.GetStringAsync(callscheduleurl);

                if (cts.IsCancellationRequested) return;

                var json = JsonCallSchedule.Parse(thejson);

                if (json is null
                    || json.Data.A1 is null
                    || json.Data.C is null
                    || json.Data.A2 is null
                    || json.Data.A3 is null
                    || json.Data.T1 is null
                    || json.Data.T2 is null
                    || json.Data.SUB is null
                    || json.Data.P1 is null
                    || json.Data.UNK is null)
                {
                    if (cts.IsCancellationRequested) return;
                    throw new InvalidOperationException("Invalid operation, json object is null or Data is null.");
                }

                if (cts.IsCancellationRequested) return;

                A1 = json.Data.A1;
                C = json.Data.C;
                A2 = json.Data.A2;
                A3 = json.Data.A3;
                T1 = json.Data.T1;
                T2 = json.Data.T2;
                SUB = json.Data.SUB;
                P1 = json.Data.P1;
                UNK = json.Data.UNK;

                // we're done here!
            }
            catch
            {
                await Console.Error.WriteLineAsync("Failed to parse CallSchedule json :(");
            }
        }

        public static string[] FetchClockSchedule(Building b)
        {
            switch (b)
            {
                case Building.A1:
                    {
                        return A1;
                    }

                case Building.C:
                    {
                        return C;
                    }

                case Building.A2:
                    {
                        return A2;
                    }

                case Building.A3:
                    {
                        return A3;
                    }

                case Building.T1:
                    {
                        return T1;
                    }

                case Building.T2:
                    {
                        return T2;
                    }

                case Building.SUB:
                    {
                        return SUB;
                    }

                case Building.P1:
                    {
                        return P1;
                    }

                default:
                    {
                        return UNK;
                    }
            }
        }

        public static DateTime GetLocalFromUnixTime(long seconds)
        {
            /*
            // секунды -> .NETовые тики
            long _ticks = seconds * TimeSpan.TicksPerSecond;
            // unix тики + мудл время
            return new DateTime(MyUnixTicks + _ticks, DateTimeKind.Utc).ToLocalTime();
            */
            return DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime;
        }
    }
}
