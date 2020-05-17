using System;
using System.Collections.Generic;
using Logic.Collision;

namespace Logic.Characters
{

    
    [Serializable]
    public class MoveList
    {


        private Dictionary<Attack, HitboxTimeline> _moves = new Dictionary<Attack, HitboxTimeline>();
        
        public void AddMove(ushort motion, Input input, MoveFlag flags, HitboxTimeline hitboxTimeline)
        {
            _moves.Add(new Attack {Motion = motion, _inputs = input, Flags = flags}, hitboxTimeline);
        }

        public Hitbox[] GetHitboxes(ushort motion, Input input, MoveFlag flags, int frame)
        {
            return _moves[new Attack {Motion = motion, _inputs = input, Flags = flags}][frame];
        }
    }
}