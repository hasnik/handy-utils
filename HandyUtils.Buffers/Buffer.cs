using System;
using System.Buffers;

namespace HandyUtils.Buffers
{
    public readonly ref struct Buffer<T>
    {
        public readonly Span<T> Span;

        private readonly T[] _rentedArray;
        private readonly bool _clearBufferOnReturn;

        /// <summary>
        /// Returns passed preallocated buffer if size is sufficient or creates a buffer for temporary data using ArrayPool. Consume via using statement.
        /// </summary>
        /// <param name="preallocatedBuffer">Pass a preallocated buffer from consuming method (consider using stackalloc)</param>
        /// <param name="bufferSize">Size of buffer to return</param>
        /// <param name="clearBufferOnReturn">When true zeroes-out buffer memory on dispose (default: false)</param>
        public Buffer(Span<T> preallocatedBuffer, int bufferSize, bool clearBufferOnReturn = false)
        {
            _rentedArray = ArrayPool<T>.Shared.Rent(bufferSize);
            _clearBufferOnReturn = clearBufferOnReturn;

            Span = _rentedArray.AsSpan(0, bufferSize);

            if (bufferSize > preallocatedBuffer.Length)
            {
                _rentedArray = ArrayPool<T>.Shared.Rent(bufferSize);
                _clearBufferOnReturn = clearBufferOnReturn;

                Span = _rentedArray.AsSpan(0, bufferSize);
            }
            else
            {
                Span = preallocatedBuffer.Slice(0, bufferSize);
            }
        }

        /// <summary>
        /// Creates a buffer for temporary data using ArrayPool. Consume via using statement. 
        /// </summary>
        /// <param name="bufferSize">Size of buffer to return</param>
        /// <param name="clearBufferOnReturn">When true zeroes-out buffer memory on dispose</param>
        public Buffer(int bufferSize, bool clearBufferOnReturn = false)
        {
            _rentedArray = ArrayPool<T>.Shared.Rent(bufferSize);
            _clearBufferOnReturn = clearBufferOnReturn;

            Span = _rentedArray.AsSpan(0, bufferSize);
        }

        public void Dispose()
        {
            if (_clearBufferOnReturn)
            {
                Span.Clear();
            }

            if (_rentedArray != null)
            {
                ArrayPool<T>.Shared.Return(_rentedArray, false);
            }
        }
    }
}
