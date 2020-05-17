using System.Runtime.InteropServices;

namespace Logic
{

    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InputStateBuffer
    {
        private static readonly int Size = 10;

        private int _lastAddedIndex;

        private InputState _input0;
        private InputState _input1;
        private InputState _input2;
        private InputState _input3;
        private InputState _input4;
        private InputState _input5;
        private InputState _input6;
        private InputState _input7;
        private InputState _input8;
        private InputState _input9;
        
        public InputState this[int index]
        {
            get => InputFromIndex(LoopedIndex(index));
            set
            {
                _lastAddedIndex = LoopedIndex(index);
                InputFromIndex(_lastAddedIndex) = value;
            }
        }
        
        private ref InputState InputFromIndex(int index)
        {
            fixed (InputState* inputPtr = &_input0)
            {
                return ref *(inputPtr + index);
            }
        }
        
        private static int LoopedIndex(int index) => (index < 0 ? Size : 0) + index % Size;
        public void AddInput(InputState input) => this[_lastAddedIndex + 1] = input;

        public InputState Current => this[_lastAddedIndex];
        public InputState Previous => this[_lastAddedIndex - 1];

        public Enumerator GetEnumerator() => new Enumerator(this);
        
        public struct Enumerator
        {
            private InputStateBuffer _buffer;
            private readonly int _startIndex;
            private int _offset;

            public Enumerator(InputStateBuffer buffer)
            {
                _offset = 0;
                _buffer = buffer;
                _startIndex = _buffer._lastAddedIndex + 1;
            }

            public InputState Current => _buffer[_startIndex + _offset];
            public bool MoveNext() => ++_offset < Size;
        }
    }
}