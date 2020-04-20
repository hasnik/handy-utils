using System;
using System.IO;
using System.Threading.Tasks;
using HandyUtils.Buffers;
using HandyUtils.Unicode.Validators;

namespace HandyUtils.Unicode
{
    public static class UnicodeEncodingDetector
    {
        /// <summary>
        ///     Tries to detect a unicode character encoding from given chunk of bytes
        /// </summary>
        /// <param name="bytes">Bytes to work on</param>
        /// <returns>Detected unicode flavour</returns>
        public static UnicodeEncodings Detect(Span<byte> bytes)
        {
            const byte bomWindowSize = 4;

            if (bytes.Length < bomWindowSize)
            {
                return UnicodeEncodings.NotFound;
            }

            if (ByteOrderMarks.TryReadBom(bytes, out var encoding))
            {
                return encoding;
            }

            var isUtf8 = Utf8Validator.IsSequencePartValid(bytes);

            if (isUtf8)
            {
                return UnicodeEncodings.Utf8;
            }

            var isUtf16Be = Utf16BigEndianValidator.IsSequencePartValid(bytes);

            if (isUtf16Be)
            {
                return UnicodeEncodings.Utf16BigEndian;
            }

            var isUtf16Le = Utf16LittleEndianValidator.IsSequencePartValid(bytes);

            if (isUtf16Le)
            {
                return UnicodeEncodings.Utf16LittleEndian;
            }

            var isUtf32Be = Utf32BigEndianValidator.IsSequencePartValid(bytes);

            if (isUtf32Be)
            {
                return UnicodeEncodings.Utf32BigEndian;
            }

            var isUtf32Le = Utf32LittleEndianValidator.IsSequencePartValid(bytes);

            if (isUtf32Le)
            {
                return UnicodeEncodings.Utf32LittleEndian;
            }

            //
            // Basically surrender, detection failed
            //
            return UnicodeEncodings.NotFound;
        }

        /// <summary>
        ///     Tries to detect a unicode character encoding from given stream
        /// </summary>
        /// <param name="stream">Stream to work on</param>
        /// <param name="tasterLength">Number of bytes to perform detection on (lower is faster, more error prone)</param>
        /// <returns></returns>
        public static async Task<UnicodeEncodings> Detect(Stream stream, int tasterLength)
        {
            using var buffer = new AsyncBuffer<byte>(tasterLength);
            var inputBytes = buffer.Memory;

            var readBytesCount = await stream.ReadAsync(inputBytes);
            stream.Position = 0;

            return Detect(inputBytes.Span.Slice(0, readBytesCount));
        }
    }
}