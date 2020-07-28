using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Utils
{
    public static class NumberFormat
    {
        public static string GetShortenedNumber(ulong number)
        {
            if (number < 1000)
            {
                return number.ToString();
            }


            if (number < 10_000)
            {
                return number / 1000 + "." + (number % 1000 / 100) + "k";
            }

            if (number < 1_000_000)
            {
                return number / 1000 + "k";
            }

            if (number < 10_000_000)
            {
                return number / 1_000_000 + "." + (number % 1_000_000 / 100_000) + "m";
            }


            return number / 1_000_000 + "m";
        }

        public static string GetShortenedNumber(long number)
        {
            return number < 0 ? "-" : "" + GetShortenedNumber((ulong)Math.Abs(number));
        }

        public static string GetShortenedNumber(int number)
        {
            return GetShortenedNumber((long)number);
        }

        public static string GetShortenedNumber(uint number)
        {
            return GetShortenedNumber((ulong)number);
        }
    }
}
