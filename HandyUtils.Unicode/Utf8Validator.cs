using System;

namespace HandyUtils.Unicode
{
    public class Utf8Validator
    {
        //
        // Well formed UTF-8 Byte Sequences
        // https://www.unicode.org/versions/Unicode10.0.0/ch03.pdf page 55
        //
        // ----------------------------------------------------------------------------
        // | Code Points        | First Byte | Second Byte | Third Byte | Fourth Byte |
        // ---------------------|------------|-------------|------------|-------------|
        // | U+0000..U+007F     | 00..7F     |             |            |             |
        // | U+0080..U+07FF     | C2..DF     | 80..BF      |            |             |
        // | U+0800..U+0FFF     | E0         | A0..BF      | 80..BF     |             |
        // | U+1000..U+CFFF     | E1..EC     | 80..BF      | 80..BF     |             |
        // | U+D000..U+D7FF     | ED         | 80..9F      | 80..BF     |             |
        // | U+E000..U+FFFF     | EE..EF     | 80..BF      | 80..BF     |             |
        // | U+10000..U+3FFFF   | F0         | 90..BF      | 80..BF     | 80..BF      |
        // | U+40000..U+FFFFF   | F1..F3     | 80..BF      | 80..BF     | 80..BF      |
        // | U+100000..U+10FFFF | F4         | 80..8F      | 80..BF     | 80..BF      |
        // ----------------------------------------------------------------------------

        // This class validates UTF-8 bytes using a state machine
        //
        // The state is 0 when enough bytes have been read for a valid UTF-8 character,
        // 12 if the byte is not allowed to occur at its position, and other positive multiplications of 12 (24-96),
        // if more bytes have to be read
        //
        // UTF-8 bytes transition schema (every transition not included in this schema leads to invalid state - 12)
        //
        //                                          C2..DF
        //              +-----------------------------------------------------------+
        //              |                                                           |
        //              |            +---------+                                    |
        //              |    E0      |         |               A0..BF               |
        //              +----------->|    48   | -----------------------------------|
        //              |            |         |                                    |
        //              |            +---------+                                    |
        //   00..7F     |                                                           |
        //   +----+     |            +---------+                                    |   +---------+
        //   |    |     |    ED      |         |               80..9F               |   |         |
        //   |    |     +----------->|    60   |------------------------------------+-->|    24   |--+
        //   |    |     |            |         |                                    |   |         |  |
        //   |    |     |            +---------+                                    |   +---------+  |
        //   |    v     |                                                           |                |
        // +---------+  |            +---------+                                    |                |
        // |         |  |    F0      |         |   90..BF                           |                |
        // |    0    |--+----------->|    72   |-----------+                        |                |
        // |         |  |            |         |           |                        |                |
        // +---------+  |            +---------+           |                        |                |
        //      ^       |                                  |                        |                |
        //      |       |            +---------+           |   +---------+          |                |
        //      |       |  F1..F3    |         |   80..BF  |   |         |  80..BF  |                |
        //      |       +----------->|    84   |-----------+-->|    36   |----------+                |
        //      |       |            |         |           |   |         |                           |
        //      |       |            +---------+           |   +---------+                           |
        //      |       |                                  |                                         |
        //      |       |            +---------+           |                                         |
        //      |       |    F4      |         |   80..8F  |                                         |
        //      |       +----------->|    96   |-----------+                                         |
        //      |       |            |         |           |                                         |
        //      |       |            +---------+           |                                         |
        //      |       |                                  |                                         |
        //      |       |          E1..EC, EE..EF          |                                         |
        //      |       +----------------------------------+                                         |
        //      |                                                                                    |
        //      |                                       80..BF                                       |
        //      +------------------------------------------------------------------------------------+

        // Instead of encoding the utf-8 byte ranges directly assign them to a specific character class mask
        //
        // 00..7F -->  0
        // 80..8F -->  1
        // 90..9F -->  9
        // A0..BF -->  7
        // C0..C1 -->  8
        // C2..DF -->  2
        // E0..E0 --> 10
        // E1..EC -->  3
        // ED..ED -->  4
        // EE..EF -->  3
        // F0..F0 --> 11
        // F1..F3 -->  6
        // F4..F4 -->  5
        // F5..FF -->  8

        //
        // Character classes transition schema (every transition not included in this schema leads to invalid state - 12)
        // 
        //                                             2  
        //              +-----------------------------------------------------------+
        //              |                                                           |
        //              |            +---------+                                    |
        //              |    10      |         |                  7                 |
        //              +----------->|    48   | -----------------------------------|
        //              |            |         |                                    |
        //              |            +---------+                                    |
        //     0        |                                                           |
        //   +----+     |            +---------+                                    |   +---------+
        //   |    |     |     4      |         |                 1, 9               |   |         |
        //   |    |     +----------->|    60   |------------------------------------+-->|    24   |--+
        //   |    |     |            |         |                                    |   |         |  |
        //   |    |     |            +---------+                                    |   +---------+  |
        //   |    v     |                                                           |                |
        // +---------+  |            +---------+                                    |                |
        // |         |  |    11      |         |    7, 9                            |                |
        // |    0    |--+----------->|    72   |-----------+                        |                |
        // |         |  |            |         |           |                        |                |
        // +---------+  |            +---------+           |                        |                |
        //      ^       |                                  |                        |                |
        //      |       |            +---------+           |   +---------+          |                |
        //      |       |     6      |         |  1, 7, 9  |   |         |  1, 7, 9 |                |
        //      |       +----------->|    84   |-----------+-->|    36   |----------+                |
        //      |       |            |         |           |   |         |                           |
        //      |       |            +---------+           |   +---------+                           |
        //      |       |                                  |                                         |
        //      |       |            +---------+           |                                         |
        //      |       |     5      |         |     1     |                                         |
        //      |       +----------->|    96   |-----------+                                         |
        //      |       |            |         |           |                                         |
        //      |       |            +---------+           |                                         |
        //      |       |                                  |                                         |
        //      |       |                 3                |                                         |
        //      |       +----------------------------------+                                         |
        //      |                                                                                    |
        //      |                                       1, 7, 9                                      |
        //      +------------------------------------------------------------------------------------+

        private const byte ValidState = 0;
        private const byte InvalidState = 12;

        // This table maps bytes to character classes
        // to reduce the size of the transition table
        private static readonly byte[] Utf8ByteToCharacterClassMask = new byte[256]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 00..0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 10..1F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 20..2F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 30..3F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 40..4F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 50..5F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 60..6F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 70..7F
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 80..8F
            9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, // 90..9F
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, // A0..AF
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, // B0..BF
            8, 8, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // C0..CF
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // D0..DF
            10, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 3, 3, // E0..EF
            11, 6, 6, 6, 5, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 // F0..FF
        };

        // Transition table that maps a combination of a state
        // and a character class to a new state
        private static readonly byte[] StateAndCharacterClassTransitions = new byte[108]
        {
            0, 12, 24, 36, 60, 96, 84, 12, 12, 12, 48, 72, //  state 0 + character class [0-11]
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, // state 12 + character class [0-11]
            12, 0, 12, 12, 12, 12, 12, 0, 12, 0, 12, 12, // state 24 + character class [0-11]
            12, 24, 12, 12, 12, 12, 12, 24, 12, 24, 12, 12, // state 36 + character class [0-11]
            12, 12, 12, 12, 12, 12, 12, 24, 12, 12, 12, 12, // state 48 + character class [0-11]
            12, 24, 12, 12, 12, 12, 12, 12, 12, 24, 12, 12, // state 60 + character class [0-11]
            12, 12, 12, 12, 12, 12, 12, 36, 12, 36, 12, 12, // state 72 + character class [0-11]
            12, 36, 12, 12, 12, 12, 12, 36, 12, 36, 12, 12, // state 84 + character class [0-11]
            12, 36, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, // state 96 + character class [0-11]
        };

        /// <summary>
        /// Checks if provided bytes are valid UTF-8 sequence
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-8</returns>
        public bool IsValidUtf8(Span<byte> bytes)
        {
            return DecodeBytes(bytes) == ValidState;
        }

        /// <summary>
        /// Checks if provided bytes are valid UTF-8 sequence part. Use when unable to determine
        /// that you have a complete UTF-8 sequence i.e. networking, processing in chunks etc.
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-8 sequence part</returns>
        public bool IsValidUtf8Part(Span<byte> bytes)
        {
            return DecodeBytes(bytes) != InvalidState;
        }

        private byte DecodeBytes(Span<byte> bytes)
        {
            byte state = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                DecodeByte(ref state, bytes[i]);
            }

            return state;
        }

        private void DecodeByte(ref byte currentState, byte inputByte)
        {
            var characterClass = Utf8ByteToCharacterClassMask[inputByte];

            currentState = StateAndCharacterClassTransitions[currentState + characterClass];
        }
    }
}
