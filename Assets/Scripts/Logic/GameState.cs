using System;
using Logic.Collision;
using Logic.Game;
using Unity.Mathematics;
using UnityEngine;

namespace Logic
{
    [Serializable]
    public struct GameState
    {
        public GameSettings GameSettings;
        
        public PlayerState Player1;
        public PlayerState Player2;

        public void Update(InputState player1Input, InputState player2Input)
        {
            Player1.InputBuffer.AddInput(player1Input);
            Player2.InputBuffer.AddInput(player2Input);
            
            var direction = (int) math.sign(Player2.Position.x - Player1.Position.x);
            Player1.LookDirection = new int2( direction, 1);
            Player2.LookDirection = new int2(-direction, 1);

            Player1.Character.UpdateState(ref Player1);
            Player2.Character.UpdateState(ref Player2);
            
            CheckCollisions();

            SimulatePhysics(Player1);
            SimulatePhysics(Player2);
        }

        private void SimulatePhysics(PlayerState player)
        {
            if (!player.IsAirborne)
            {
                var speed = math.abs(player.Velocity.x);
                var sign = (int) math.sign(player.Velocity.x);
                player.Velocity.x -= math.min(speed, player.Character.GroundDrag) * sign;
            }

            if (player.Position.y > 0)
            {
                if (player.IgnoreGravity > 0)
                {
                    player.IgnoreGravity--;
                }
                else
                {
                    player.Velocity.y -= GameSettings.Gravity;
                }

                player.IsAirborne = true;
            }
            else if (player.IsAirborne)
            {
                player.IsAirborne = false;
                player.ActiveAttack = -1;
                player.ActionDuration = 0;
                player.AirOptions = 2;
                player.Velocity.y = 0;
                player.Position.y = 0;
            }

            player.Position += player.Velocity;
            player.Position.x = math.clamp
            (
                player.Position.x,
                -GameSettings.StageHalfSize,
                GameSettings.StageHalfSize
            );
        }

        private void CheckCollisions()
        {
            var p1Hitboxes = Player1.GetHitboxes();
            var p2Hitboxes = Player2.GetHitboxes();

            foreach (var p1Hitbox in p1Hitboxes)
            foreach (var p2Hitbox in p2Hitboxes)
            {
                if (!p1Hitbox.Intersects(p2Hitbox)) continue;

                if (Player1.ActiveHits > 0 
                 && p1Hitbox.CheckCollisionType(p2Hitbox) == CollisionType.Hit)
                {
                    Player1.ActiveHits--;
                    Player2.Velocity = Player1.GetActiveAttack().Force * Player1.LookDirection;
                }

                if (Player2.ActiveHits > 0 
                 && p2Hitbox.CheckCollisionType(p1Hitbox) == CollisionType.Hit)
                {
                    Player2.ActiveHits--;
                    Player1.Velocity = Player2.GetActiveAttack().Force * Player2.LookDirection;
                }
            }
        }
    }

    public static class GameStateExtensions
    {
        public static void DrawHitboxes(this GameState gameState)
        {
            Debug.DrawLine
            (
                new Vector3(-gameState.GameSettings.StageHalfSize, 0, 0),
                new Vector3(gameState.GameSettings.StageHalfSize, 0, 0),
                Color.white, 0
            );
            
            foreach (var hitbox in gameState.Player1.GetHitboxes()) { hitbox.DebugDraw(); }
            foreach (var hitbox in gameState.Player2.GetHitboxes()) { hitbox.DebugDraw(); }
        }
    }
}