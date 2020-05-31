using Logic.Characters;
using UnityEngine;

namespace Logic.Game
{
    [CreateAssetMenu(fileName = "Game/" + nameof(GameSettings))]
    public class GameSettings : ScriptableObject
    {
        public AttackLevel[] AttackLevels;
        public int StageHalfSize;
        public int Gravity;
    }
}