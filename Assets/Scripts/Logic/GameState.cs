using System;
using Unity.Mathematics;

namespace Logic
{
    public struct GameState : IEquatable<GameState>
    {
        public PlayerState Player1;
        public PlayerState Player2;

        public bool Equals(GameState other)
        {
            return Player1.Equals(other.Player1) 
                && Player2.Equals(other.Player2);
        }

        public void Update(InputState player1Input, InputState player2Input)
        {
            Player1.Update(player1Input);
            Player2.Update(player2Input);
        }
    }
}