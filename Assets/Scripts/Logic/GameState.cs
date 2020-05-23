using Logic.Collision;
using Unity.Mathematics;

namespace Logic
{
    public struct GameState
    {
        public PlayerState Player1;
        public PlayerState Player2;

        public void Update(InputState player1Input, InputState player2Input)
        {
            Player1.InputBuffer.AddInput(player1Input);
            Player2.InputBuffer.AddInput(player2Input);
            
            var direction = (int) math.sign(Player2.Position.x - Player1.Position.x);
            Player1.LookDirection = new float2( direction, 1);
            Player2.LookDirection = new float2(-direction, 1);

            Player1.Character.UpdateState(ref Player1);
            Player2.Character.UpdateState(ref Player2);
            
            CheckCollisions();
        }
        
        private void CheckCollisions()
        {
            var p1Hitboxes = Player1.Character.GetHitboxes(Player1);
            var p2Hitboxes = Player2.Character.GetHitboxes(Player2);

            foreach (var p1Hitbox in p1Hitboxes)
            foreach (var p2Hitbox in p2Hitboxes)
            {
                var p1Rect = p1Hitbox.GetRect(Player1);
                var p2Rect = p2Hitbox.GetRect(Player2);
                if (!p1Rect.Intersects(p2Rect)) continue;
                
                if (Player1.ActiveHits > 0
                 && (p1Hitbox.Flags & ~HitboxFlag.Hurt) != 0
                 && (p2Hitbox.Flags & HitboxFlag.Hurt) != 0)
                {
                    Player2.Velocity += Player1.GetActiveAttack().Force * Player1.LookDirection;
                    Player1.ActiveHits--;
                }
            }
        }
    }

    public static class GameStateExtensions
    {
        public static void DrawHitboxes(this GameState gameState)
        {
            foreach (var hitbox in gameState.Player1.Character.GetHitboxes(gameState.Player1))
            {
                hitbox.DebugDraw(gameState.Player1);
            }
            foreach (var hitbox in gameState.Player2.Character.GetHitboxes(gameState.Player2))
            {
                hitbox.DebugDraw(gameState.Player2);
            }
        }
    }
}