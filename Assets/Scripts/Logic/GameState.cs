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
            var direction = math.sign(Player2.Position.x - Player1.Position.x);
            Player1.LookDirection = new float2(direction, 1);
            Player1.LookDirection = new float2(-direction, 1);

            Player1.InputBuffer.AddInput(player1Input);
            Player2.InputBuffer.AddInput(player2Input);
            
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
                var p1Rect = p1Hitbox.GetRect(Player1.Position);
                var p2Rect = p2Hitbox.GetRect(Player2.Position);
                var hit = p1Rect.Intersects(p2Rect);
            }
        }
    }

    public static class GameStateExtensions
    {
        public static void DrawHitboxes(this GameState gameState)
        {
            foreach (var hitbox in gameState.Player1.Character.GetHitboxes(gameState.Player1))
            {
                hitbox.DebugDraw(gameState.Player1.Position);
            }
            foreach (var hitbox in gameState.Player2.Character.GetHitboxes(gameState.Player2))
            {
                hitbox.DebugDraw(gameState.Player2.Position);
            }
        }
    }
}