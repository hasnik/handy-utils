using System;
using System.Buffers;

namespace HandyUtils.Buffers
{
    /// <summary>
    ///     Wrapper that helps with buffer management in asynchronous scenarios.
    ///     Gives access to trimmed contiguous region of memory through Memory field.
    /// </summary>
    /// <typeparam name="T">Type of data to hold in buffer</typeparam>
    public readonly struct AsyncBuffer<T> : IDisposable
    {
        /// <value>
        /// Gives access to trimmed contiguous region of buffer memory
        /// </value>
        public readonly Memory<T> Memory;

        private readonly T[] _rentedArray;
        private readonly bool _clearBufferOnReturn;

        /// <summary>
        ///     Wraps preallocated buffer if size is sufficient or creates a buffer for temporary data using ArrayPool.
        ///     Consume via using statement.
        /// </summary>
        /// <param name="preallocatedBuffer">Pass a preallocated buffer from consuming method</param>
        /// <param name="bufferSize">Size of buffer needed</param>
        /// <param name="clearBufferOnReturn">When true zeroes-out buffer memory on dispose (default: false)</param>
        public AsyncBuffer(Memory<T> preallocatedBuffer, int bufferSize, bool clearBufferOnReturn = false)
        {
            _clearBufferOnReturn = clearBufferOnReturn;

            if (bufferSize > preallocatedBuffer.Length)
            {
                _rentedArray = ArrayPool<T>.Shared.Rent(bufferSize);
                Memory = _rentedArray.AsMemory(0, bufferSize);
            }
            else
            {
                _rentedArray = null;
                Memory = preallocatedBuffer.Slice(0, bufferSize);
            }
        }

        /// <summary>
        ///     Creates a buffer for temporary data using ArrayPool. Consume via using statement.
        /// </summary>
        /// <param name="bufferSize">Size of buffer needed</param>
        /// <param name="clearBufferOnReturn">When true zeroes-out buffer memory on dispose (default: false)</param>
        public AsyncBuffer(int bufferSize, bool clearBufferOnReturn = false)
        {
            _rentedArray = ArrayPool<T>.Shared.Rent(bufferSize);
            _clearBufferOnReturn = clearBufferOnReturn;

            Memory = _rentedArray.AsMemory(0, bufferSize);
        }

        public void Dispose()
        {
            if (_clearBufferOnReturn)
            {
                Memory.Span.Clear();
            }

            if (_rentedArray != null)
            {
                ArrayPool<T>.Shared.Return(_rentedArray, false);
            }
        }
    }
}