using System;
using Common;
using Logic.Collision;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.Characters
{
    [Serializable]
    public struct HitboxTimeline 
    {
        [Serializable]
        private struct HitboxKeyFrame
        {
            public int StartFrame;
            public Hitbox[] Hitboxes;
        }
        
        [SerializeField] private HitboxKeyFrame[] _keyframes;
        
        public Hitbox[] this[int frame] => GetHitboxes(frame);
        
        public void AddHitboxes(int startFrame, Hitbox[] hitboxes)
        {
            var index = GetIndex(startFrame);
            var hitboxKeyframe = new HitboxKeyFrame
            {
                StartFrame = startFrame,
                Hitboxes = hitboxes
            };
            
            if (_keyframes == null)
            {
                _keyframes = new[] {hitboxKeyframe};
            }
            else if (_keyframes[index].StartFrame == startFrame)
            {
                _keyframes[index].Hitboxes = hitboxKeyframe.Hitboxes;
            }
            else 
            {
                ArrayUtility.InsertAt(ref _keyframes, index + 1, hitboxKeyframe);
            }
        }

        private Hitbox[] GetHitboxes(int frame)
        {
            var index = GetIndex(frame);
            return index >= 0 
                ? _keyframes[index].Hitboxes 
                : Array.Empty<Hitbox>();
        }

        private int GetIndex(int frame)
        {
            if (_keyframes == null) return -1;
            if (frame < _keyframes[0].StartFrame) return -1;

            for (var timelineIndex = 0; timelineIndex < _keyframes.Length - 1; timelineIndex++)
            {
                var startFrame = _keyframes[timelineIndex].StartFrame;
                var endFrame = _keyframes[timelineIndex + 1].StartFrame;
                if (frame >= startFrame && frame < endFrame)
                {
                    return timelineIndex;
                }
            }

            return _keyframes.Length - 1;
        }
    }
}