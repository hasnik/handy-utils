using System;

namespace HandyUtils.Unicode.Validators
{
    internal static class Utf32ValidatorCore
    {
        // For better understanding of what is happening here see UTF8Validator

        //
        // UTF-32 Encoding scheme range:
        //
        // 00000000..0010FFFF -> valid 4 bytes code points

        // Most significant byte should be 0, and the next one in 00..10 range
        // We can skip the 2 least significant bytes check, as every value is valid

        //
        // Character classes
        //
        // 00..00 -> 0
        // 01..10 -> 1
        // 11..FF -> 2

        internal const byte ValidState = 0;
        internal const byte InvalidState = 3;

        private static readonly byte[] MostSignificantBytePairToCharacterClassMask = new byte[256]
        {
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, // 00..0F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 10..1F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 20..2F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 30..3F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 40..4F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 50..5F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 60..6F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 70..7F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 80..8F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 90..9F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // A0..AF
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // B0..BF
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // C0..CF
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // D0..DF
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // E0..EF
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 // F0..FF
        };

        private static readonly byte[] StateAndCharacterClassTransitions = new byte[9]
        {
            6, 3, 3, // state 0 + character class [0-2]
            3, 3, 3, // state 3 + character class [0-2]
            0, 0, 3 // state 6 + character class [0-2]
        };

        internal static byte DecodeBigEndianBytes(Span<byte> bytes)
        {
            return DecodeBytes(bytes, 1);
        }

        internal static byte DecodeLittleEndianBytes(Span<byte> bytes)
        {
            return DecodeBytes(bytes, 3);
        }

        private static byte DecodeBytes(Span<byte> bytes, byte offset)
        {
            byte state = 0;

            for (int i = offset; i < bytes.Length; i += 4)
            {
                DecodeByte(ref state, bytes[i]);
                DecodeByte(ref state, bytes[i - 1]);
            }

            return state;
        }

        private static void DecodeByte(ref byte currentState, byte inputByte)
        {
            var characterClass = MostSignificantBytePairToCharacterClassMask[inputByte];

            currentState = StateAndCharacterClassTransitions[currentState + characterClass];
        }
    }
}