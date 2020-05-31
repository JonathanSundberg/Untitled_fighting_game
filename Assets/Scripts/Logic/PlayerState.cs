
using System;
using Common;
using Logic.Characters;
using Logic.Collision;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Logic
{
    [Serializable]
    public struct PlayerState
    {
        public Character Character;
        public int2 Position;
        
        [NonSerialized] public InputBuffer InputBuffer;

        [NonSerialized] public int2 Velocity;
        [NonSerialized] public int2 LookDirection;
        [NonSerialized] public int IgnoreGravity;

        [NonSerialized] public bool IsAirborne;
        [NonSerialized] public int AirOptions;

        [NonSerialized] public int ActionDuration;
        [NonSerialized] public int ActiveAttack;
        [NonSerialized] public int ActiveHits;

        public bool ReverseInputs => LookDirection.x < 0;

        public void SimulatePhysics(GameState gameState)
        {
            if (!IsAirborne)
            {
                var speed = math.abs(Velocity.x);
                var sign = (int) math.sign(Velocity.x);
                Velocity.x -= math.min(speed, Character.GroundDrag) * sign;
            }

            if (Position.y > 0)
            {
                if (IgnoreGravity > 0)
                {
                    IgnoreGravity--;
                }
                else
                {
                    Velocity.y -= gameState.GameSettings.Gravity;
                }
                
                IsAirborne = true;
            }
            else if (IsAirborne)
            {
                IsAirborne = false;
                ActiveAttack = -1;
                ActionDuration = 0;
                AirOptions = 2;
                Velocity.y = 0;
                Position.y = 0;
            }

            Position += Velocity;
            Position.x = math.clamp
            (
                Position.x,
                -gameState.GameSettings.StageHalfSize,
                 gameState.GameSettings.StageHalfSize
            );
        }

        public Attack GetActiveAttack()
        {
            return Character.GetAttack(ActiveAttack);
        }

        public NativeArray<Hitbox> GetHitboxes()
        {
            var hitboxes = Character.GetHitboxes(this);
            var transformedHitboxes = new NativeArray<Hitbox>(hitboxes.Length, Allocator.Temp);
            
            for (var hitboxIndex = 0; hitboxIndex < hitboxes.Length; hitboxIndex++)
            {
                transformedHitboxes[hitboxIndex] = hitboxes[hitboxIndex].GetTransformed(this);
            }

            return transformedHitboxes;
        }
        
    }
}