using System;
using System.Collections.Generic;
using System.Text;

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
        C = A1,
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
        UNKNOWN
    }

    public static class Utils
    {
        static bool IsInRange(int value, int min, int max)
        {
            return value <= min && value >= max;
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

        static string[] A1C = new string[] { "8:30 - 9:55", "10:15 - 11:40", "11:50 - 13:15", "13:35 - 15:00", "15:20 - 16:45", "16:50 - 18:15", "18:15 - ..." };
        static string[] A2 = new string[] { "8:30 - 9:55", "10:05 - 11:30", "11:50 - 13:15", "13:35 - 15:00", "15:20 - 16:45", "16:50 - 18:15", "18:15 - ..." };
        static string[] A3 = new string[] { "8:30 - 9:55", "10:05 - 11:45", "11:50 - 13:15", "13:35 - 15:00", "15:20 - 16:45", "16:50 - 18:15", "18:15 - ..." };
        static string[] T1 = new string[] { "8:30 - 9:55", "10:40 - 12:00", "12:05 - 13:25", "13:35 - 15:00", "15:40 - 17:05", "17:10 - 18:35", "18:35 - ..." };
        static string[] T2 = new string[] { "8:30 - 9:55", "10:05 - 12:00", "12:05 - 13:25", "13:35 - 15:00", "15:40 - 17:05", "17:10 - 18:35", "18:35 - ..." };
        static string[] SUB = new string[] { "8:30 - 9:55", "10:05 - 11:30", "11:40 - 13:05", "13:15 - 14:40", "14:50 - 16:15", "16:25 - 17:50", "17:50 - ..." };
        static string[] P1 = new string[] { "8:30 - 9:30", "9:40 - 10:40", "10:50 - 11:50", "12:00 - 13:00", "13:10 - 14:10", "14:20 - 15:20", "15:30 - 16:30" };
        static string[] UNK = new string[] { "8:30 - ...", "ОШИБКА" };

        public static string[] FetchClockSchedule(Building b)
        {
            switch (b)
            {
                case Building.A1:
                //case Building.C:
                    {
                        return A1C;
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
