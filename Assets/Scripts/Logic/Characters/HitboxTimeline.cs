using System;
using System.Linq;
using Logic.Collision;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Logic.Characters
{
    [Serializable]
    public struct HitboxTimeline 
    {
        [Serializable]
        private struct HitboxKeyframe
        {
            [FormerlySerializedAs("StartFrame")] 
            public int Frame;
            public Hitbox[] Hitboxes;
        }
        
        [SerializeField] private HitboxKeyframe[] _keyframes;
        
        public Hitbox[] this[int frame] => GetHitboxes(frame);
        
        public void AddHitboxes(int startFrame, Hitbox[] hitboxes)
        {
            var index = GetKeyframeIndex(startFrame);
            var hitboxKeyframe = new HitboxKeyframe
            {
                Frame = startFrame,
                Hitboxes = hitboxes
            };
            
            if (_keyframes == null)
            {
                _keyframes = new[] {hitboxKeyframe};
            }
            else if (_keyframes[index].Frame == startFrame)
            {
                _keyframes[index].Hitboxes = hitboxKeyframe.Hitboxes;
            }
            else 
            {
                ArrayUtility.Insert(ref _keyframes, index + 1, hitboxKeyframe);
            }
        }

        private Hitbox[] GetHitboxes(int frame)
        {
            var index = GetKeyframeIndex(frame);
            return index >= 0 
                ? _keyframes[index].Hitboxes 
                : Array.Empty<Hitbox>();
        }

        public int GetKeyframeIndex(int frame)
        {
            if (_keyframes == null || _keyframes.Length == 0) return -1;
            if (frame < _keyframes[0].Frame) return -1;

            for (var timelineIndex = 0; timelineIndex < _keyframes.Length - 1; timelineIndex++)
            {
                var startFrame = _keyframes[timelineIndex].Frame;
                var endFrame = _keyframes[timelineIndex + 1].Frame;
                if (frame >= startFrame && frame < endFrame)
                {
                    return timelineIndex;
                }
            }

            return _keyframes.Length - 1;
        }

        public int[] GetKeyframes()
        {
            return _keyframes.Select(keyframe => keyframe.Frame).ToArray();
        }

        public bool AddHitbox(int frame, Hitbox hitbox)
        {
            var keyframeIndex = GetKeyframeIndex(frame);
            if (keyframeIndex == -1) return false;
            
            ArrayUtility.Add(ref _keyframes[keyframeIndex].Hitboxes, hitbox);
            return true;
        }
        

        public void SetHitbox(int keyframe, int index, Hitbox hitbox)
        {
            _keyframes[keyframe].Hitboxes[index] = hitbox;
        }
        
        public Hitbox GetHitbox(int keyframe, int index)
        {
            return _keyframes[keyframe].Hitboxes[index];
        }

        public bool AddKeyframe(int frame)
        {
            var index = GetKeyframeIndex(frame);
            if (index >= 0 && _keyframes[index].Frame == frame) return false;
            
            var keyframe = new HitboxKeyframe
            {
                Frame = frame,
                Hitboxes = index >= 0 
                    ? _keyframes[index].Hitboxes.ToArray() 
                    : new Hitbox[] {}
            };
            
            ArrayUtility.Insert(ref _keyframes, index + 1, keyframe);
            return true;
        }

        public void RemoveHitbox(int keyframe, int index)
        {
            ArrayUtility.RemoveAt(ref _keyframes[keyframe].Hitboxes, index);
        }

        public bool RemoveKeyframe(int frame)
        {
            var keyframeIndex = GetKeyframeIndex(frame);
            if (keyframeIndex == -1) return false;
            
            ArrayUtility.RemoveAt(ref _keyframes, keyframeIndex);
            return true;
        }
    }
}