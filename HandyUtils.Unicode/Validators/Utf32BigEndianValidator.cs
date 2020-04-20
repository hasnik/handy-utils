using System;
using HandyUtils.Numbers;

namespace HandyUtils.Unicode.Validators
{
    public static class Utf32BigEndianValidator
    {
        /// <summary>
        ///     Checks if provided bytes are valid UTF-32 BE sequence
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-32 BE</returns>
        public static bool IsSequenceValid(Span<byte> bytes)
        {
            if (Number.IsDivisibleBy4(bytes.Length) == false)
            {
                return false;
            }

            return Utf32ValidatorCore.DecodeBigEndianBytes(bytes) == Utf32ValidatorCore.ValidState;
        }

        /// <summary>
        ///     Checks if provided bytes are valid UTF-32 BE sequence with tolerance for last code point missing bytes
        ///     Use when unable to determine that you have a complete UTF-32 BE sequence i.e. networking or file tasting
        /// </summary>
        /// <param name="bytes">Span containing bytes to examine</param>
        /// <returns>If the bytes are valid UTF-32 BE sequence part</returns>
        public static bool IsSequencePartValid(Span<byte> bytes)
        {
            return Utf32ValidatorCore.DecodeBigEndianBytes(bytes) != Utf32ValidatorCore.InvalidState;
        }
    }
}