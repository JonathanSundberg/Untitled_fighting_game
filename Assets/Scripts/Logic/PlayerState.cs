
using Logic.Characters;
using Unity.Mathematics;

namespace Logic
{
    public struct PlayerState
    {
        public readonly Character Character;
        public InputBuffer InputBuffer;
        
        public float2 Position;
        public float2 Velocity;
        public float2 LookDirection;
        public bool ReverseInputs => LookDirection.x < 0;
        
        public bool IsAirborne;
        public int AirOptions;

        public int ActionDuration;
        public int ActiveAttack;
        public int ActiveHits;

        public PlayerState(Character character, float2 position)
        {
            Character = character;
            InputBuffer = default;
            
            Position = position;
            Velocity = 0;
            LookDirection = 0;

            IsAirborne = position.y > math.FLT_MIN_NORMAL;
            AirOptions = 2;

            ActiveAttack = -1;
            ActionDuration = 0;
            ActiveHits = 0;
        }

        public Attack GetActiveAttack()
        {
            return Character.GetAttack(ActiveAttack);
        }
    }
}