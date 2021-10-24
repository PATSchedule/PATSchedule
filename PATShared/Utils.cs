using System;
using System.Collections.Generic;
using System.Text;

namespace PATShared
{
    internal static class Utils
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
    }
}
