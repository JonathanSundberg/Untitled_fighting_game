using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> GetChildren(this Transform transform)
        {
            var childCount = transform.childCount;
            for (var childIndex = 0; childIndex < childCount; childIndex++)
            {
                yield return transform.GetChild(childIndex);
            }
        }
    }
}