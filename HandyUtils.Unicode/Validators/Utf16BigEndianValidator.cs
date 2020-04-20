using System;
using HandyUtils.Numbers;

namespace HandyUtils.Unicode.Validators
{
    public static class Utf16BigEndianValidator
    {
        /// <summary>
        ///     Checks if provided bytes are valid UTF-16 BE sequence
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-16 BE</returns>
        public static bool IsSequenceValid(Span<byte> bytes)
        {
            //
            // Valid UTF-16 sequence length must be even
            //
            if (Number.IsDivisibleBy2(bytes.Length) == false)
            {
                return false;
            }

            return Utf16ValidatorCore.DecodeBigEndianBytes(bytes) == Utf16ValidatorCore.ValidState;
        }

        /// <summary>
        ///     Checks if provided bytes are valid UTF-16 BE sequence with tolerance for last code point missing bytes
        ///     Use when unable to determine that you have a complete UTF-16 BE sequence i.e. networking or file tasting
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-16 BE sequence part</returns>
        public static bool IsSequencePartValid(Span<byte> bytes)
        {
            return Utf16ValidatorCore.DecodeBigEndianBytes(bytes) != Utf16ValidatorCore.InvalidState;
        }
    }
}