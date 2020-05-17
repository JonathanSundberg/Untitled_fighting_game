using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Logic.Collision;
using Unity.Mathematics;
using UnityEngine;
using static Logic.Characters.MoveFlag;

namespace Logic.Characters
{

    [Flags]
    public enum Direction
    {
        Up    = 0x10,
        Down  = 0x20,
        Left  = 0x40,
        Right = 0x80
    }

    [Flags]
    public enum MoveFlag
    {
        Ground = 0x1,
        Air    = 0x2
    }
    
    [Serializable]
    public struct Attack
    {
        public ushort Motion;
        public Input _inputs;
        public MoveFlag Flags;
        public HitboxTimeline Hitboxes;

        public Attack(ushort motion, Input inputs, MoveFlag flags, HitboxTimeline hitboxes)
        {
            Motion = motion;
            _inputs = inputs;
            Flags = flags;
            Hitboxes = hitboxes;
        }
    }
    
    [CreateAssetMenu(menuName = "Settings/" + nameof(Character))]
    public class Character : ScriptableObject
    {
        [SerializeField] private List<Attack> _attacks;
        [SerializeField] private HitboxTimeline _idleHitboxes;

        public void UpdateState(ref PlayerState state)
        {
            state.TimelineFrame++;
            HandleInput(ref state);
            UpdatePosition(ref state);
        }

        private void UpdatePosition(ref PlayerState state)
        {
            if (!state.IsAirborne)
            {
                state.Velocity.x *= 0.9f;
            }

            if (state.Position.y > 0)
            {
                state.Velocity.y -= 0.05f;
                state.IsAirborne = true;
            }
            else if (state.Position.y < 0)
            {
                state.IsAirborne = false;
                state.AirOptions = 2;
                state.Velocity.y = 0;
                state.Position.y = 0;
            }

            state.Velocity.x = math.clamp(state.Velocity.x, -1, 1);
            state.Position += state.Velocity;
        }

        private void HandleInput(ref PlayerState state)
        {
            var currentInput = state.InputBuffer.Current;
            var previousInput = state.InputBuffer.Previous;
            
            if (state.AirOptions > 0 
            && currentInput.HasInput(Input.Up) 
            && !previousInput.HasInput(Input.Up))
            {
                if (currentInput.HasInput(Input.Left))
                {
                    state.Velocity.x -= 0.5f;
                }
                else if (currentInput.HasInput(Input.Right))
                {
                    state.Velocity.x += 0.5f;
                }
                
                state.Velocity.y += 0.7f;
                state.AirOptions--;
            }

            if (!state.IsAirborne)
            {
                if (currentInput.HasInput(Input.Left))
                {
                    state.Velocity.x -= 0.5f;
                }

                if (currentInput.HasInput(Input.Right))
                {
                    state.Velocity.x += 0.5f;
                }
            }
        }

        public Hitbox[] GetHitboxes(PlayerState playerStateHandle)
        {
            return _idleHitboxes[playerStateHandle.TimelineFrame];
        }
    }
}