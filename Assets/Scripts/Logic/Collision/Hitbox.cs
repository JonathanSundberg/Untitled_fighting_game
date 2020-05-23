using System;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.Collision
{
    [Flags]
    public enum HitboxFlag
    {
        Hurt       = 0x01,
        High       = 0x02,
        Mid        = 0x04,
        Low        = 0x08,
        Projectile = 0x10,
    }

    [Serializable]
    public struct Hitbox
    {
        public HitboxFlag Flags;
        public Rect Rect;
    }

    public static class HitboxExtensions
    {
        private static readonly Color _defaultColor = new Color(0.6f,  0.6f,  0.6f);
        private static readonly Color _bodyColor    = new Color(0.36f, 0.6f,  0.24f);
        private static readonly Color _highColor    = new Color(0.6f,  0.24f, 0.24f);
        private static readonly Color _midColor     = new Color(0.6f,  0.39f, 0.24f);
        private static readonly Color _lowColor     = new Color(0.6f,  0.54f, 0.24f);

        public static Rect GetRect(this Hitbox hitbox, PlayerState state)
        {
            var rect = hitbox.Rect;
            rect.Position *= state.LookDirection;
            rect.Position += state.Position;
            return rect;
        }

        public static void DebugDraw(this Hitbox hitbox, PlayerState state)
        {
            var rect = hitbox.GetRect(state);

            Vector2 _00 = rect.Position + rect.Size * new float2(-0.5f,  0f);
            Vector2 _01 = rect.Position + rect.Size * new float2(-0.5f,  1f);
            Vector2 _10 = rect.Position + rect.Size * new float2( 0.5f,  0f);
            Vector2 _11 = rect.Position + rect.Size * new float2( 0.5f,  1f);

            var color = (hitbox.Flags & HitboxFlag.Hurt) > 0 ? _bodyColor : _highColor;

            Debug.DrawLine(_00, _01, color, 0, false);
            Debug.DrawLine(_11, _01, color, 0, false);
            Debug.DrawLine(_11, _10, color, 0, false);
            Debug.DrawLine(_00, _10, color, 0, false);
        }
    }
}