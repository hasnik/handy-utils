using System.Runtime.CompilerServices;

namespace HandyUtils.Numbers
{
    public static class Number
    {
        private static readonly bool[] BytesDivisibleBy4Map = new bool[128]
        {
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 00..0F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 10..1F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 20..2F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 30..3F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 40..4F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 50..5F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 60..6F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 70..7F
        };

        public static bool IsDivisibleBy2(int number)
        {
            return (number & 1) == 0;
        }

        public static bool IsDivisibleBy4(int number)
        {
            const byte lowestByteWithEveryBitSetTo1HigherThanDecimal99 = 0b_0111_1111;

            return BytesDivisibleBy4Map[number & lowestByteWithEveryBitSetTo1HigherThanDecimal99];
        }
    }
}