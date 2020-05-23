using System;
using System.Runtime.InteropServices;

namespace Common.StackArray
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StackArray4<T> where T : unmanaged
    {
        private const int LENGTH = 4;
        public int Length => LENGTH;
        
        #region Elements
        private T _element0;
        private T _element1;
        private T _element2;
        private T _element3;
        #endregion
        
        public T this[int index]
        {
            get => GetElement(index);
            set => GetElement(index) = value;
        }
        
        private unsafe ref T GetElement(int index)
        {
            if (index < 0 || index >= LENGTH) throw new ArgumentOutOfRangeException();
            
            fixed (T* inputPtr = &_element0)
            {
                return ref *(inputPtr + index);
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        
        public struct Enumerator
        {
            private readonly StackArray4<T> _buffer;
            private int _index;

            public Enumerator(StackArray4<T> buffer)
            {
                _buffer = buffer;
                _index = 0;
            }

            public T Current => _buffer[_index];
            public bool MoveNext() => ++_index < LENGTH;
        }
    }
}