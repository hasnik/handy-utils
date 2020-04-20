using System;
using System.Runtime.InteropServices;

namespace HandyUtils.Unicode
{
    internal static class SpanExtensions
    {
        internal static ref T GetReference<T>(this Span<T> span)
        {
            return ref MemoryMarshal.GetReference(span);
        }
    }
}