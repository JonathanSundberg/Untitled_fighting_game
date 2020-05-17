using System;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.Collision
{
    public enum HitboxType
    {
        Body,
        High,
        Mid,
        Low
    }
    
    [Serializable]
    public struct Hitbox
    {
        public HitboxType Type;
        public Rect Rect;

        public Hitbox(float x, float y, float w, float h)
        {
            Rect = new Rect
            {
                Position = {x = x, y = y},
                Size = {x = w, y = h}
            };

            Type = HitboxType.Body;
        }
    }

    public static class HitboxExtensions
    {
        private static readonly Color _defaultColor = new Color(0.6f,  0.6f,  0.6f);
        private static readonly Color _bodyColor    = new Color(0.36f, 0.6f,  0.24f);
        private static readonly Color _highColor    = new Color(0.6f,  0.24f, 0.24f);
        private static readonly Color _midColor     = new Color(0.6f,  0.39f, 0.24f);
        private static readonly Color _lowColor     = new Color(0.6f,  0.54f, 0.24f);

        public static Rect GetRect(this Hitbox hitbox, float2 positionOffset = default)
        {
            var rect = hitbox.Rect;
            rect.Position += positionOffset;
            return rect;
        }

        public static void DebugDraw(this Hitbox hitbox, float2 position)
        {
            var center = hitbox.Rect.Position + position;
            var size = hitbox.Rect.Size;

            Vector2 _00 = center + size * new float2(-0.5f, -0.5f);
            Vector2 _01 = center + size * new float2(-0.5f,  0.5f);
            Vector2 _10 = center + size * new float2( 0.5f, -0.5f);
            Vector2 _11 = center + size * new float2( 0.5f,  0.5f);

            var color =
                hitbox.Type == HitboxType.Body ? _bodyColor :
                hitbox.Type == HitboxType.High ? _highColor :
                hitbox.Type == HitboxType.Mid  ? _midColor  :
                hitbox.Type == HitboxType.Low  ? _lowColor  :
                _defaultColor;

            
            Debug.DrawLine(_00, _01, color, 0, false);
            Debug.DrawLine(_11, _01, color, 0, false);
            Debug.DrawLine(_11, _10, color, 0, false);
            Debug.DrawLine(_00, _10, color, 0, false);
        }
    }
}