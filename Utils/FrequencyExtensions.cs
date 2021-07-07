using Dev_Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Utils
{
    public static class FrequencyExtensions
    {
        public static int GetDays(this Subscriber.EmailFrequency frequency)
        {
            int value = 0;
            switch(frequency)
            {
                case Subscriber.EmailFrequency.AsSoonAsPossible:
                    value = 0;
                    break;
                case Subscriber.EmailFrequency.Daily:
                    value = 1;
                    break;
                case Subscriber.EmailFrequency.Weekly:
                    value = 7;
                    break;
                case Subscriber.EmailFrequency.Monthly:
                    value = 30;
                    break;
            }

            return value;
        }
    }
}
