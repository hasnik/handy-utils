using System;

namespace HandyUtils.Unicode
{
    public class Utf16Validator
    {
        // In UTF-16 Encoding scheme range:
        // 0000..D7FF -> valid 2 bytes code point
        // D800..DBFF -> first 2 bytes of 4 bytes code point (high surrogate)
        // DC00..DFFF -> last 2 bytes of 4 bytes code point (low surrogate)
        // E000..FFFF -> valid 2 bytes code point

        // We can skip the second byte check, as every value is valid, so let's trim them for character classes

        //Character classes
        // 00..D7 -> 0 (first byte of a valid 2 bytes code point)
        // D8..DB -> 1 (first byte of high surrogate)
        // DC..DF -> 2 (first byte of low surrogate)
        // E0..FF -> 0 (first byte of a valid 2 bytes code point)

        private const byte ValidState = 0;
        private const byte InvalidState = 3;

        private static readonly byte[] Utf16MostSignificantByteToCharacterClassMask = new byte[256]
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
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // F0..FF
        };

        private static readonly byte[] StateAndCharacterClassTransitions = new byte[9]
        {
            0, 6, 3, // state 0 + character class [0-2]
            3, 3, 3, // state 3 + character class [0-2]
            3, 3, 0, // state 6 + character class [0-2]
        };

        /// <summary>
        /// Checks if provided bytes are valid UTF-16 BE sequence
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-16 BE</returns>
        public bool IsValidUtf16BigEndian(Span<byte> bytes)
        {
            //
            // Valid UTF-16 sequence length must be even
            //
            if (IsSpanLengthOdd(bytes))
            {
                return false;
            }

            return DecodeBigEndianBytes(bytes) == ValidState;
        }

        /// <summary>
        /// Checks if provided bytes are valid UTF-16 LE sequence
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-16 LE</returns>
        public bool IsValidUtf16LittleEndian(Span<byte> bytes)
        {
            //
            // Valid UTF-16 sequence length must be even
            //
            if (IsSpanLengthOdd(bytes))
            {
                return false;
            }

            return DecodeLittleEndianBytes(bytes) == ValidState;
        }

        /// <summary>
        /// Checks if provided bytes are valid UTF-16 BE sequence part. Use when unable to determine
        /// that you have a complete UTF-16 BE sequence i.e. networking, processing in chunks etc.
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-16 BE sequence part</returns>
        public bool IsValidUtf16BigEndianPart(Span<byte> bytes)
        {
            return DecodeBigEndianBytes(bytes) != InvalidState;
        }

        /// <summary>
        /// Checks if provided bytes are valid UTF-16 LE sequence part. Use when unable to determine
        /// that you have a complete UTF-16 LE sequence i.e. networking, processing in chunks etc.
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-16 LE sequence part</returns>
        public bool IsValidUtf16LittleEndianPart(Span<byte> bytes)
        {
            return DecodeLittleEndianBytes(bytes) != InvalidState;
        }

        private byte DecodeBigEndianBytes(Span<byte> bytes)
        {
            byte state = 0;

            // Most significant byte on the left side of byte pair
            for (var i = 0; i < bytes.Length; i += 2)
            {
                DecodeByte(ref state, bytes[i]);
            }

            return state;
        }

        private byte DecodeLittleEndianBytes(Span<byte> bytes)
        {
            byte state = 0;

            // Most significant byte on the right side of byte pair
            for (var i = 1; i < bytes.Length; i += 2)
            {
                DecodeByte(ref state, bytes[i]);
            }

            return state;
        }

        private void DecodeByte(ref byte currentState, byte inputByte)
        {
            var characterClass = Utf16MostSignificantByteToCharacterClassMask[inputByte];

            currentState = StateAndCharacterClassTransitions[currentState + characterClass];
        }

        private bool IsSpanLengthOdd(Span<byte> bytes)
        {
            return (bytes.Length & 1) == 1;
        }
    }
}
