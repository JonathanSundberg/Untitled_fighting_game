using System;
using System.Runtime.InteropServices;

namespace Common.StackArray
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StackArray32<T> where T : unmanaged
    {
        private const int LENGTH = 32;
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
        private T _element16;
        private T _element17;
        private T _element18;
        private T _element19;
        private T _element20;
        private T _element21;
        private T _element22;
        private T _element23;
        private T _element24;
        private T _element25;
        private T _element26;
        private T _element27;
        private T _element28;
        private T _element29;
        private T _element30;
        private T _element31;
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
            private readonly StackArray32<T> _buffer;
            private int _index;

            public Enumerator(StackArray32<T> buffer)
            {
                _buffer = buffer;
                _index = 0;
            }

            public T Current => _buffer[_index];
            public bool MoveNext() => ++_index < LENGTH;
        }
    }
}