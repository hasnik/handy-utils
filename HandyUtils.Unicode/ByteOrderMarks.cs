using System;
using System.Runtime.InteropServices;

namespace HandyUtils.Unicode
{
    public static class ByteOrderMarks
    {
        public const byte WindowSize = 4;

        // UTF-32 BOM are 4 bytes long, UTF-16 BOM 2 bytes long,
        // UTF-8 and UTF-7 BOMs are 3 bytes long, so we use uint (4 bytes long)
        // to store them in the same value type and compare
        // them easier against input without casting.

        private const uint Utf32BigEndian = 0xFF_FE_00_00;
        private const uint Utf32LittleEndian = 0x00_00_FE_FF;

        private const uint Utf16BigEndianBom = 0xFF_FE_00_00;
        private const uint Utf16LittleEndian = 0xFE_FF_00_00;

        private const uint Utf8Bom = 0xBF_BB_EF_00;

        //
        // UTF-7 actually has 5 different kinds of BOM, but each on of them start with these bytes,
        // possibly over-simplification, but let's pay the price, since this encoding is exotic
        //
        private const uint Utf7Bom = 0x76_2F_2B_00;

        public static bool TryReadBom(Span<byte> bytes, out UnicodeEncodings encoding)
        {
            if (bytes.Length < WindowSize)
            {
                throw new ArgumentException(
                    $"Expected {nameof(bytes)} length to be at least {WindowSize}, but was {bytes.Length}");
            }

            var beginningByteQuartet = MemoryMarshal.Read<uint>(bytes);
            
            var hasFoundBom = false;
            encoding = default;

            if (IsUtf8ByteOrderMark(beginningByteQuartet))
            {
                encoding = UnicodeEncodings.Utf8;
                hasFoundBom = true;
            }
            else if (IsUtf32BigEndian(beginningByteQuartet))
            {
                encoding = UnicodeEncodings.Utf32BigEndian;
                hasFoundBom = true;
            }
            else if (IsUtf32LittleEndian(beginningByteQuartet))
            {
                encoding = UnicodeEncodings.Utf32LittleEndian;
                hasFoundBom = true;
            }
            else if (IsUtf16BigEndian(beginningByteQuartet))
            {
                encoding = UnicodeEncodings.Utf16BigEndian;
                hasFoundBom = true;
            }
            else if (IsUtf16LittleEndian(beginningByteQuartet))
            {
                encoding = UnicodeEncodings.Utf16LittleEndian;
                hasFoundBom = true;
            }
            else if (IsUtf7ByteOrderMark(beginningByteQuartet))
            {
                encoding = UnicodeEncodings.Utf7;
                hasFoundBom = true;
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
        // When Byte Order Mark is shorter than 4 bytes use binary shift
        //
        private static bool IsUtf16BigEndian(uint beginningByteQuartet)
        {
            return beginningByteQuartet << 16 == Utf16BigEndianBom;
        }

        private static bool IsUtf16LittleEndian(uint beginningByteQuartet)
        {
            return beginningByteQuartet << 16 == Utf16LittleEndian;
        }

        private static bool IsUtf8ByteOrderMark(uint beginningByteQuartet)
        {
            return beginningByteQuartet << 8 == Utf8Bom;
        }

        private static bool IsUtf7ByteOrderMark(uint beginningByteQuartet)
        {
            return beginningByteQuartet << 8 == Utf7Bom;
        }
    }
}