using UnityEngine;

namespace Logic.Characters
{
    [CreateAssetMenu(menuName = "Character/" + nameof(Animation))]
    public class Animation : ScriptableObject
    {
        public int Duration;
        public HitboxTimeline Hitboxes;
        
        
    }
}