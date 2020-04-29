using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions
{
    public static class GameObjectExtensions
    {
        public static IEnumerable<GameObject> GetChildren(this GameObject game_object)
        {
            return game_object.transform.GetChildren().Select(child => child.gameObject);
        }
    }
}