using System.Collections.Generic;
using Logic.Collision;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.Characters
{
    [CreateAssetMenu(menuName = "Character/" + nameof(Character))]
    public class Character : ScriptableObject
    {
        [SerializeField] private int ForwardSpeed;
        [SerializeField] private int BackSpeed;
        [SerializeField] public int GroundDrag;
        [SerializeField] private int2 JumpForce;
        [SerializeField] private int2 AirDashForce;
        [SerializeField] private int AirDashDuration;
        
        [SerializeField] private Animation _standingAnimation;
        [SerializeField] private List<Attack> _normals;
        [SerializeField] private List<Attack> _specials;

        public void UpdateState(ref PlayerState state)
        {
            state.ActionDuration++;
            if (state.ActiveAttack >= 0)
            {
                var attackDuration = _specials[state.ActiveAttack].Duration;
                if (state.ActionDuration >= attackDuration)
                {
                    state.ActiveAttack = -1;
                }
            }
            else
            {
                HandleInput(ref state);
            }
        }
        
        private void HandleInput(ref PlayerState state)
        {
            // handle special inputs
            for (var specialIndex = 0; specialIndex < _specials.Count; specialIndex++)
            {
                var special = _specials[specialIndex];
                if (!state.InputBuffer.GetButtonPress(special.Buttons)
                 || !state.InputBuffer.ContainsMotion(special.Motion, state.ReverseInputs)) continue;
                
                state.ActiveAttack = specialIndex;
                state.ActiveHits = special.Hits;
                state.ActionDuration = 0;

                return;
            }
            // handle normal inputs
            for (var normalIndex = 0; normalIndex < _normals.Count; normalIndex++)
            {
                var normal = _normals[normalIndex];
                var direction = normal.Motion.ToDirection(state.ReverseInputs);
                
                if (!state.InputBuffer.GetButtonPress(normal.Buttons)
                 || !state.InputBuffer.GetDirection(direction)) continue;
                
                state.ActiveAttack = normalIndex;
                state.ActiveHits = normal.Hits;
                state.ActionDuration = 0;

                return;
            }
            // handle air options
            if (state.AirOptions > 0)
            {
                if (state.InputBuffer.GetDirectionPress(Direction.Up))
                {
                    state.AirOptions--;
                    state.Velocity.y = JumpForce.y;
                
                    if (state.InputBuffer.GetDirection(Direction.Left))
                    {
                        state.Velocity.x = -JumpForce.x;
                    }
                
                    if (state.InputBuffer.GetDirection(Direction.Right))
                    {
                        state.Velocity.x = JumpForce.x;
                    }
                }

                var forward = state.ReverseInputs ? Direction.Left : Direction.Right;
                
                if (state.IsAirborne 
                 && state.AirOptions > 0
                 && state.InputBuffer.GetDirectionPress(forward) 
                 && state.InputBuffer.ContainsMotion(656, state.ReverseInputs))
                {
                    state.AirOptions--;
                    state.Velocity = AirDashForce * state.LookDirection;
                    state.IgnoreGravity = AirDashDuration;
                }
            }
            // handle grounded options
            if (!state.IsAirborne)
            {
                if (state.InputBuffer.GetDirection(Direction.Left))
                {
                    state.Velocity.x = -(state.ReverseInputs ? ForwardSpeed : BackSpeed);
                }

                if (state.InputBuffer.GetDirection(Direction.Right))
                {
                    state.Velocity.x = state.ReverseInputs ? BackSpeed : ForwardSpeed;
                }
            }
        }

        public Hitbox[] GetHitboxes(PlayerState state)
        {
            if (state.ActiveAttack < 0)
            {
                return _standingAnimation.Hitboxes[state.ActionDuration % _standingAnimation.Duration];
            }

            return _specials[state.ActiveAttack].Animation.Hitboxes[state.ActionDuration];
        }

        public Attack GetAttack(int index)
        {
            return _specials[index];
        }
    }
}