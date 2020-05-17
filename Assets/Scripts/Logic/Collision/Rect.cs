using System;
using Unity.Mathematics;

namespace Logic.Collision
{
    [Serializable]
    public struct Rect
    {
        public float2 Position;
        public float2 Size;
    }

    public static class RectExtensions
    {
        public static bool Intersects(this Rect a, Rect b)
        {
            return math.all(a.Position < b.Position + b.Size)
                && math.all(a.Position + a.Size > b.Position);
        }
    }
}