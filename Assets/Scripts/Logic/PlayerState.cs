
using Logic.Characters;
using Unity.Mathematics;

namespace Logic
{
    public struct PlayerState
    {
        public readonly Character Character;
        public InputStateBuffer InputBuffer;
        
        public int Health;
        public float2 Position;

        public float2 Velocity;
        public int AirOptions;

        public int TimelineFrame;
        public float2 LookDirection;
        public bool IsAirborne;

        public PlayerState(Character character, float2 position)
        {
            Character = character;
            Position = position;
            IsAirborne = position.y > 0;
            InputBuffer = default;
            Health = 100;
            Velocity = 0;
            AirOptions = 2;
            TimelineFrame = 0;
            LookDirection = 0;
        }
    }

    public static class PlayerStateExtensions
    {
    }
}