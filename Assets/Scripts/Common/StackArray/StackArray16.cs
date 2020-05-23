using System;
using System.Runtime.InteropServices;

namespace Common.StackArray
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StackArray16<T> where T : unmanaged
    {
        private const int LENGTH = 16;
        public int Length => LENGTH;
        
        #region Elements
        private T _element0;
        private T _element1;
        private T _element2;
        private T _element3;
        private T _element4;
        private T _element5;
        private T _element6;
        private T _element7;
        private T _element8;
        private T _element9;
        private T _element10;
        private T _element11;
        private T _element12;
        private T _element13;
        private T _element14;
        private T _element15;
        #endregion
        
        public T this[int index]
        {
            get => GetElement(index);
            set => GetElement(index) = value;
        }
        
        public unsafe ref T GetElement(int index)
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
            private readonly StackArray16<T> _buffer;
            private int _index;

            public Enumerator(StackArray16<T> buffer)
            {
                _buffer = buffer;
                _index = 0;
            }

            public T Current => _buffer[_index];
            public bool MoveNext() => ++_index < LENGTH;
        }
    }
}