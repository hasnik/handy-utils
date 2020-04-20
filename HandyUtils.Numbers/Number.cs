namespace HandyUtils.Numbers
{
    public static class Number
    {
        private static readonly bool[] BytesDivisibleBy4Map = new bool[256]
        {
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 00..0F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 10..1F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 20..2F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 30..3F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 40..4F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 50..5F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 60..6F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 70..7F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 80..8F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // 90..9F
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // A0..AF
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // B0..BF
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // C0..CF
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // D0..DF
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, // E0..EF
            true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false // F0..FF
        };

        public static bool IsDivisibleBy2(int number)
        {
            return (number & 1) == 0;
        }

        public static bool IsDivisibleBy4(int number)
        {
            return BytesDivisibleBy4Map[number & byte.MaxValue];
        }
    }
}