using System;
using Unity.Mathematics;

namespace Logic
{
    public struct PlayerState : IEquatable<PlayerState>
    {
        public  float2 Position;
        private float2 _velocity;
        private int _airOptions;

        private InputState _oldInput;

        public void Update(InputState input)
        {
            if (input.Direction.x < 0)
            {
                _velocity.x = math.max(-0.25f, _velocity.x - 0.05f);
            }
            else if (input.Direction.x > 0)
            {
                _velocity.x = math.min(0.25f, _velocity.x + 0.05f);
            }
            else
            {
                _velocity.x *= 0.7f;
            }

            var jump = input.Direction.y > 0 && _oldInput.Direction.y == 0;
            if (jump && _airOptions > 0)
            {
                _velocity.y = 0.7f;
                _airOptions--;
            }
            else if (Position.y > math.FLT_MIN_NORMAL)
            {
                _velocity.y -= 0.05f;
            }
            else
            {
                _airOptions = 2;
                _velocity.y = 0;
                Position.y = 0;
            }

            Position += _velocity;
            _oldInput = input;
        }

        public bool Equals(PlayerState other)
        {
            return Position.Equals(other.Position) 
                && _velocity.Equals(other._velocity);
        }
    }
}