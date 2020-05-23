using System.Runtime.CompilerServices;
using Common.StackArray;
using Unity.Mathematics;

namespace Logic
{
    public struct InputBuffer
    {
        private StackArray16<InputState> _buffer;
        private int _lastAddedIndex;
        
        public InputState this[int index]
        {
            get => _buffer[LoopedIndex(index)];
            set => _buffer[_lastAddedIndex = LoopedIndex(index)] = value;
        }
        
        public InputState Current => this[_lastAddedIndex];
        public InputState Previous => this[_lastAddedIndex - 1];
        
        private int LoopedIndex(int index)
        {
            return (index < 0 ? _buffer.Length : 0) + index % _buffer.Length;
        }

        public void AddInput(InputState input)
        {
            this[_lastAddedIndex + 1] = input;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetButton(Button buttons)
        {
            return Current.GetButton(buttons);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetButtonRelease(Button buttons)
        {
            return !Current.GetButton(buttons) && Previous.GetButton(buttons);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetButtonPress(Button buttons)
        {
            return Current.GetButton(buttons) && !Previous.GetButton(buttons);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetDirection(Direction direction)
        {
            return Current.GetDirection(direction);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetDirectionRelease(Direction direction)
        {
            return !Current.GetDirection(direction) && Previous.GetDirection(direction);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetDirectionPress(Direction direction)
        {
            return Current.GetDirection(direction) && !Previous.GetDirection(direction);
        }

        public unsafe bool ContainsMotion(short motion, bool reverseInputs)
        {
            var directions = ParseMotion(motion, reverseInputs, out var motionLength);

            var confirmedDirections = 0;
            var currentDirection = (byte) directions[confirmedDirections];
            
            foreach (var input in this)
            {
                if ((input.Inputs & currentDirection) != currentDirection) continue;
                if (++confirmedDirections < motionLength)
                {
                    currentDirection = (byte) directions[confirmedDirections];
                }
                else return true;
            }

            return false;
        }

        private static unsafe Direction* ParseMotion(short motion, bool reverseInputs, out int length)
        {
            length = (int) math.log10(motion) + 1;
            var directions = stackalloc Direction[length];

            for (var digitIndex = 0; digitIndex < length; digitIndex++)
            {
                var digit = GetDigit(motion, length, digitIndex);
                directions[digitIndex] = NumpadToDirection(digit, reverseInputs);
            }

            return directions;
        }

        private static Direction NumpadToDirection(byte numpadNotation, bool reverseInputs)
        {
            var direction = default(Direction);
            var modulus = (numpadNotation - 1) % 3;
            direction |= numpadNotation >= 7 ? Direction.Up : 0;
            direction |= numpadNotation <= 3 ? Direction.Down : 0;
            
            if (!reverseInputs)
            {
                direction |= modulus < 0 ? Direction.Left : 0;
                direction |= modulus > 0 ? Direction.Right : 0;
            }
            else
            {
                direction |= modulus < 0 ? Direction.Right : 0;
                direction |= modulus > 0 ? Direction.Left : 0;
            }
            
            return direction;
        }

        private static byte GetDigit(int number, int length, int index)
        {
            return (byte) (number / math.max(1, (int) math.pow(10, length - 1 - index)) % 10);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            private InputBuffer _inputBuffer;
            private readonly int _startIndex;
            private int _offset;

            public Enumerator(InputBuffer inputBuffer)
            {
                _inputBuffer = inputBuffer;
                _startIndex = _inputBuffer._lastAddedIndex + 1;
                _offset = 0;
            }

            public InputState Current => _inputBuffer[_startIndex + _offset];
            public bool MoveNext() => ++_offset < _inputBuffer._buffer.Length;
        }
    }
}