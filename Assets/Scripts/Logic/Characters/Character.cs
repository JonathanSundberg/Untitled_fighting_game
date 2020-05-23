using System.Collections.Generic;
using Logic.Collision;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.Characters
{
    [CreateAssetMenu(menuName = "Character/" + nameof(Character))]
    public class Character : ScriptableObject
    {
        [SerializeField] private float ForwardSpeed;
        [SerializeField] private float BackSpeed;
        
        [SerializeField] private List<Attack> _attacks;
        [SerializeField] private Animation _standingAnimation;

        public void UpdateState(ref PlayerState state)
        {
            state.ActionDuration++;
            if (state.ActiveAttack >= 0)
            {
                var attackDuration = _attacks[state.ActiveAttack].Duration;
                if (state.ActionDuration >= attackDuration)
                {
                    state.ActiveAttack = -1;
                }
            }
            else
            {
                HandleInput(ref state);
            }
            
            UpdatePosition(ref state);
        }
        
        private void HandleInput(ref PlayerState state)
        {
            for (var attackIndex = 0; attackIndex < _attacks.Count; attackIndex++)
            {
                var attack = _attacks[attackIndex];
                if (!state.InputBuffer.GetButtonPress(attack.Buttons)
                 || !state.InputBuffer.ContainsMotion(attack.Motion, state.ReverseInputs)) continue;
                
                state.ActiveAttack = attackIndex;
                state.ActiveHits = attack.Hits;
                state.ActionDuration = 0;

                break;
            }

            if (state.ActiveAttack != -1) return;

            if (state.AirOptions > 0)
            {
                if (state.InputBuffer.GetDirectionPress(Direction.Up))
                {
                    state.AirOptions--;
                    state.Velocity.y = 0.7f;
                
                    if (state.InputBuffer.GetDirection(Direction.Left))
                    {
                        state.Velocity.x = -0.5f;
                    }
                
                    if (state.InputBuffer.GetDirection(Direction.Right))
                    {
                        state.Velocity.x = 0.5f;
                    }
                }

                var forward = state.ReverseInputs ? Direction.Left : Direction.Right;
                
                if (state.IsAirborne 
                 && state.AirOptions > 0
                 && state.InputBuffer.GetDirectionPress(forward) 
                 && state.InputBuffer.ContainsMotion(656, state.ReverseInputs))
                {
                    state.AirOptions--;
                    state.Velocity.x = state.ReverseInputs ? -1 : 1;
                    state.Velocity.y = 0.3f;
                }
            }

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

        private void UpdatePosition(ref PlayerState state)
        {
            if (!state.IsAirborne)
            {
                state.Velocity.x *= 0.8f;
            }

            if (state.Position.y > math.FLT_MIN_NORMAL)
            {
                state.Velocity.y -= 0.05f;
                state.IsAirborne = true;
            }
            else if (state.IsAirborne)
            {
                state.IsAirborne = false;
                state.ActiveAttack = -1;
                state.ActionDuration = 0;
                state.AirOptions = 2;
                state.Velocity.y = 0;
                state.Position.y = 0;
            }

            state.Position += state.Velocity;
        }

        public Hitbox[] GetHitboxes(PlayerState state)
        {
            if (state.ActiveAttack < 0)
            {
                return _standingAnimation.Hitboxes[state.ActionDuration % _standingAnimation.Duration];
            }

            return _attacks[state.ActiveAttack].Animation.Hitboxes[state.ActionDuration];
        }

        public Attack GetAttack(int index)
        {
            return _attacks[index];
        }
    }
}