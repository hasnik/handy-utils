using System;
using System.Runtime.CompilerServices;

namespace HandyUtils.Unicode
{
    public static class ByteOrderMarks
    {
        // UTF-32 BOM are 4 bytes long, UTF-16 BOM 2 bytes long,
        // UTF-8 and UTF-7 BOMs are 3 bytes long, so we use uint (4 bytes long)
        // to store them in the same value type and compare
        // them easier against input without casting.
        // Left-over bytes are left empty.

        private const uint Utf32BigEndian = 0xFF_FE_00_00;
        private const uint Utf32LittleEndian = 0x00_00_FE_FF;

        private const uint Utf16BigEndianBom = 0x00_00_FF_FE;
        private const uint Utf16LittleEndian = 0x00_00_FE_FF;

        private const uint Utf8Bom = 0x00_BF_BB_EF;

        //
        // UTF-7 actually has 5 different kinds of BOM, but each on of them start with these bytes,
        // possibly over-simplification, but let's pay the price, since this encoding is exotic
        //
        private const uint Utf7Bom = 0x00_76_2F_2B;

        public static bool TryReadBom(Span<byte> bytes, out UnicodeEncodings encoding)
        {
            if (bytes.Length < 4)
            {
                throw new ArgumentException(
                    $"Expected {nameof(bytes)} length to be at least 4, but was {bytes.Length}");
            }

            var hasFoundBom = false;
            encoding = default;

            ref var bytesInitialElement = ref bytes.GetReference();
            var beginningByteQuartet = Unsafe.As<byte, uint>(ref bytesInitialElement);

            if (IsUtf8ByteOrderMark(beginningByteQuartet))
            {
                hasFoundBom = true;
                encoding = UnicodeEncodings.Utf8;
            }
            else if (IsUtf32BigEndian(beginningByteQuartet))
            {
                hasFoundBom = true;
                encoding = UnicodeEncodings.Utf32BigEndian;
            }
            else if (IsUtf32LittleEndian(beginningByteQuartet))
            {
                hasFoundBom = true;
                encoding = UnicodeEncodings.Utf32LittleEndian;
            }
            else if (IsUtf16BigEndian(beginningByteQuartet))
            {
                hasFoundBom = true;
                encoding = UnicodeEncodings.Utf16BigEndian;
            }
            else if (IsUtf16LittleEndian(beginningByteQuartet))
            {
                hasFoundBom = true;
                encoding = UnicodeEncodings.Utf16LittleEndian;
            }
            else if (IsUtf7ByteOrderMark(beginningByteQuartet))
            {
                hasFoundBom = true;
                encoding = UnicodeEncodings.Utf7;
            }

            return hasFoundBom;
        }

        private static bool IsUtf32BigEndian(uint beginningByteQuartet)
        {
            return beginningByteQuartet == Utf32BigEndian;
        }

        private static bool IsUtf32LittleEndian(uint beginningByteQuartet)
        {
            return beginningByteQuartet == Utf32LittleEndian;
        }

        //
        // When Byte Order Mark is shorter than 4 bytes use binary AND
        // so we only really compare bytes that matter.
        // In other words: whatever & 0 is always 0 e.g.
        //‭ FE FF HE HE & FE FF 00 00 = FE FF 00 00
        //
        private static bool IsUtf16BigEndian(uint beginningByteQuartet)
        {
            return (beginningByteQuartet & Utf16BigEndianBom) == Utf16BigEndianBom;
        }

        private static bool IsUtf16LittleEndian(uint beginningByteQuartet)
        {
            return (beginningByteQuartet & Utf16LittleEndian) == Utf16LittleEndian;
        }

        private static bool IsUtf8ByteOrderMark(uint beginningByteQuartet)
        {
            return (beginningByteQuartet & Utf8Bom) == Utf8Bom;
        }

        private static bool IsUtf7ByteOrderMark(uint beginningByteQuartet)
        {
            return (beginningByteQuartet & Utf7Bom) == Utf7Bom;
        }
    }
}