using System;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.Characters
{
    [Flags]
    public enum AttackFlag
    {
        Ground = 0x1,
        Air    = 0x2
    }
    
    [CreateAssetMenu(menuName = "Character/" + nameof(Attack))]
    public class Attack : ScriptableObject
    {
        public short Motion;
        public Button Buttons;
        public AttackFlag Flags;
        [Range(0, 5)] public int Level;
        public float2 Force;
        public int Hits;
        public Animation Animation;

        public int Duration => Animation.Duration;
    }
}