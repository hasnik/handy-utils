using System.Runtime.CompilerServices;

namespace HandyUtils.Numbers
{
    public static class Number
    {
        public static bool IsDivisibleBy2(int number)
        {
            return (number & 1) == 0;
        }

        public static bool IsDivisibleBy4(int number)
        {
            return (number & 3) == 0;
        }
    }
}
