using System;
using Common;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Logic.Collision
{
    public enum HitboxType
    {
        Body,
        Attack
    }

    public enum CollisionType
    {
        None,
        Clash,
        Hit,
    }

    [Serializable]
    public struct Hitbox
    {
        public HitboxType Type;
        public int2 Position;
        public int2 Size;
    }

    public static class HitboxExtensions
    {
        private static readonly Color _bodyColor = new Color(0.37f, 0.8f, 0.16f);
        private static readonly Color _attackColor = new Color(0.6f, 0.12f, 0.12f);
        
        public static bool Intersects(this Hitbox a, Hitbox b)
        {
            return math.all(a.Position < b.Position + b.Size) 
                && math.all(a.Position + a.Size > b.Position);
        }

        public static CollisionType CheckCollisionType(this Hitbox a, Hitbox b)
        {
            if (a.Type == HitboxType.Attack)
            {
                if (b.Type == HitboxType.Attack) return CollisionType.Clash;
                if (b.Type == HitboxType.Body) return CollisionType.Hit;
            }

            return CollisionType.None;
        }

        public static Hitbox GetTransformed(this Hitbox hitbox, PlayerState state)
        {
            var offsetHitbox = hitbox;
            offsetHitbox.Position *= state.LookDirection;
            offsetHitbox.Position += state.Position;
            return offsetHitbox;
        }

        public static void DebugDraw(this Hitbox hitbox)
        {
            Vector2 _00 = hitbox.Position + hitbox.Size * new float2(-0.5f,  0f);
            Vector2 _01 = hitbox.Position + hitbox.Size * new float2(-0.5f,  1f);
            Vector2 _10 = hitbox.Position + hitbox.Size * new float2( 0.5f,  0f);
            Vector2 _11 = hitbox.Position + hitbox.Size * new float2( 0.5f,  1f);

            Color color;
            switch (hitbox.Type)
            {
                case HitboxType.Body: color = _bodyColor; break;
                case HitboxType.Attack: color = _attackColor; break;
                default: throw new ArgumentOutOfRangeException();
            }

            Debug.DrawLine(_00, _01, color, 0, false);
            Debug.DrawLine(_11, _01, color, 0, false);
            Debug.DrawLine(_11, _10, color, 0, false);
            Debug.DrawLine(_00, _10, color, 0, false);
        }
    }
}