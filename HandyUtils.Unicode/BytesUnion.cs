using System;
using System.Runtime.InteropServices;

namespace HandyUtils.Unicode
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct BytesUnion
    {
        [FieldOffset(0)]
        public readonly byte FirstByte;
        [FieldOffset(1)]
        public readonly byte SecondByte;
        [FieldOffset(2)]
        public readonly byte ThirdByte;
        [FieldOffset(3)]
        public readonly byte FourthByte;

        [FieldOffset(0)]
        public readonly ushort FirstBytePair;
        [FieldOffset(2)]
        public readonly ushort SecondBytePair;

        [FieldOffset(0)]
        public readonly uint ByteQuartet;

        // Watch out when running this on architectures other than x86, assuming little endian byte order
        private BytesUnion(byte firstByte, byte secondByte, byte thirdByte, byte fourthByte)
        {
            ByteQuartet = default;
            FirstBytePair = default;
            SecondBytePair = default;

            FirstByte = firstByte;
            SecondByte = secondByte;
            ThirdByte = thirdByte;
            FourthByte = fourthByte;
        }

        internal static BytesUnion Create(
            byte firstByte,
            byte secondByte = default,
            byte thirdByte = default,
            byte fourthByte = default)
        {
            return new BytesUnion(firstByte, secondByte, thirdByte, fourthByte);
        }

        internal static BytesUnion Create(Span<byte> input)
        {
            return Create(input[0], input[1], input[2], input[3]);
        }

        public static implicit operator uint(in BytesUnion foo) => foo.ByteQuartet;
    }
}
