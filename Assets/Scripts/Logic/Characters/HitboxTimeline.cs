using System;
using Common;
using Logic.Collision;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.Characters
{
    [CreateAssetMenu(menuName = "Settings/" + nameof(HitboxTimeline))]
    public class HitboxTimeline : ScriptableObject
    {
        [Serializable]
        private struct HitboxKeyframe
        {
            public int StartFrame;
            public Hitbox[] Hitboxes;
        }
        
        [SerializeField] private int _duration;
        [SerializeField] private HitboxKeyframe[] _timeline;

        public Hitbox[] this[int frame] => GetHitboxes(frame);
        public int Duration { get => _duration; set => _duration = value; }

        public void AddHitboxes(int startFrame, Hitbox[] hitboxes)
        {
            var index = GetIndexFromFrame(startFrame);
            var hitboxKeyframe = new HitboxKeyframe
            {
                StartFrame = startFrame,
                Hitboxes = hitboxes
            };
            
            if (_timeline == null)
            {
                _timeline = new[] {hitboxKeyframe};
            }
            else if (_timeline[index].StartFrame == startFrame)
            {
                _timeline[index].Hitboxes = hitboxKeyframe.Hitboxes;
            }
            else 
            {
                ArrayUtility.InsertAt(ref _timeline, index + 1, hitboxKeyframe);
            }
        }

        private Hitbox[] GetHitboxes(int frame)
        {
            var index = GetIndexFromFrame(frame);
            return index >= 0 
                ? _timeline[index].Hitboxes 
                : Array.Empty<Hitbox>();
        }

        private int GetIndexFromFrame(int frame)
        {
            var loopedFrame = math.max(0, frame % Duration);

            if (_timeline == null) return -1;
            if (loopedFrame < _timeline[0].StartFrame) return -1;

            for (var timelineIndex = 0; timelineIndex < _timeline.Length - 1; timelineIndex++)
            {
                var startFrame = _timeline[timelineIndex].StartFrame;
                var endFrame = _timeline[timelineIndex + 1].StartFrame;
                if (loopedFrame >= startFrame && loopedFrame < endFrame)
                {
                    return timelineIndex;
                }
            }

            return _timeline.Length - 1;
        }
    }
}