using System;

namespace HandyUtils.Unicode.Validators
{
    internal static class Utf16ValidatorCore
    {
        // For better understanding of what is happening here see UTF8Validator

        //
        // UTF-16 Encoding scheme range:
        //
        // 0000..D7FF -> valid 2 bytes code points
        // D800..DBFF -> first 2 bytes of 4 bytes code points (high surrogate)
        // DC00..DFFF -> last 2 bytes of 4 bytes code points (low surrogate)
        // E000..FFFF -> valid 2 bytes code points

        // We can skip the second byte check, as every value is valid

        //
        // Character classes
        //
        // 00..D7 -> 0 (first byte of a valid 2 bytes code point)
        // D8..DB -> 1 (first byte of high surrogate)
        // DC..DF -> 2 (first byte of low surrogate)
        // E0..FF -> 0 (first byte of a valid 2 bytes code point)

        internal const byte ValidState = 0;
        internal const byte InvalidState = 3;

        private static readonly byte[] MostSignificantByteToCharacterClassMaskMap = new byte[256]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 00..0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 10..1F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 20..2F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 30..3F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 40..4F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 50..5F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 60..6F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 70..7F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 80..8F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 90..9F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // A0..AF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // B0..BF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // C0..CF
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, // D0..DF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // E0..EF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 // F0..FF
        };

        private static readonly byte[] StateAndCharacterClassTransitions = new byte[9]
        {
            0, 6, 3, // state 0 + character class [0-2]
            3, 3, 3, // state 3 + character class [0-2]
            3, 3, 0 // state 6 + character class [0-2]
        };

        internal static byte DecodeBigEndianBytes(Span<byte> bytes)
        {
            // Most significant byte on the left side of byte pair, hence offset 0
            return DecodeBytes(bytes, 0);
        }

        internal static byte DecodeLittleEndianBytes(Span<byte> bytes)
        {
            // Most significant byte on the right side of byte pair, hence offset 1
            return DecodeBytes(bytes, 1);
        }

        private static byte DecodeBytes(Span<byte> bytes, byte offset)
        {
            byte state = 0;

            for (int i = offset; i < bytes.Length; i += 2)
            {
                DecodeByte(ref state, bytes[i]);
            }

            return state;
        }

        private static void DecodeByte(ref byte currentState, byte inputByte)
        {
            var characterClass = MostSignificantByteToCharacterClassMaskMap[inputByte];

            currentState = StateAndCharacterClassTransitions[currentState + characterClass];
        }
    }
}