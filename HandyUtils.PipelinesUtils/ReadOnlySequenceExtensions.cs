using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace HandyUtils.PipelinesUtils
{
    public static class ReadOnlySequenceExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(in this ReadOnlySequence<byte> buffer, Encoding encoding)
        {
            if (buffer.IsSingleSegment)
            {
                return encoding.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    encoding.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }
    }
}
