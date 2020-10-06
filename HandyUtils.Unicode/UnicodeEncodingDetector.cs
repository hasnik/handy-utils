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
        ///     Tries to detect a unicode character encoding from given span of bytes
        /// </summary>
        /// <param name="bytes">Bytes to work on</param>
        /// <returns>Detected unicode flavour</returns>
        public static UnicodeEncodings Detect(Span<byte> bytes)
        {
            if (bytes.Length < ByteOrderMarks.WindowSize)
            {
                return UnicodeEncodings.NotFound;
            }

            var detectedEncoding = default(UnicodeEncodings);

            if (ByteOrderMarks.TryReadBom(bytes, out var encoding))
            {
                detectedEncoding = encoding;
            }
            else if (Utf8Validator.IsSequencePartValid(bytes))
            {
                detectedEncoding = UnicodeEncodings.Utf8;
            }
            else if (Utf16BigEndianValidator.IsSequencePartValid(bytes))
            {
                detectedEncoding = UnicodeEncodings.Utf16BigEndian;
            }
            else if (Utf16LittleEndianValidator.IsSequencePartValid(bytes))
            {
                detectedEncoding = UnicodeEncodings.Utf16LittleEndian;
            }
            else if (Utf32BigEndianValidator.IsSequencePartValid(bytes))
            {
                detectedEncoding = UnicodeEncodings.Utf32BigEndian;
            }
            else if (Utf32LittleEndianValidator.IsSequencePartValid(bytes))
            {
                detectedEncoding = UnicodeEncodings.Utf32LittleEndian;
            }

            return detectedEncoding;
        }

        /// <summary>
        ///     Tries to detect a unicode character encoding from given stream. Warning: this will advance stream position!
        /// </summary>
        /// <param name="stream">Stream to work on</param>
        /// <param name="tasterLength">Number of bytes to perform detection on (lower is faster, more error prone)</param>
        /// <returns>Detected unicode flavour</returns>
        public static async Task<UnicodeEncodings> DetectAsync(Stream stream, int tasterLength)
        {
            if (stream.CanRead == false)
            {
                throw new ArgumentException(
                    $"Provided {nameof(stream)} is not readable."
                );
            }

            using var buffer = new AsyncBuffer<byte>(tasterLength);
            var inputBytes = buffer.Memory;

            var readBytesCount = await stream.ReadAsync(inputBytes);

            return Detect(inputBytes.Span.Slice(0, readBytesCount));
        }
    }
}