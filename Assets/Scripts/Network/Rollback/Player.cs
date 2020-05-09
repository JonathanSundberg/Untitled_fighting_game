using System;

namespace Network.Rollback
{
    public enum PlayerType
    {
        Local,
        Remote,
        Spectator
    }
    
    public readonly struct PlayerHandle
    {
        public readonly int Id;
        public readonly PlayerType Type;

        public PlayerHandle(int id, PlayerType type)
        {
            Id = id;
            Type = type;
        }

        public static bool operator ==(PlayerHandle a, PlayerHandle b) => a.Id == b.Id;
        public static bool operator !=(PlayerHandle a, PlayerHandle b) => a.Id != b.Id;
    }
    
    public class Player<TState>
    where TState : struct, IEquatable<TState>
    {
        private struct InputState
        {
            public TState State;
            public int Frame;
        }

        private RingBuffer<InputState> _savedStates;
        private int _lastAddedFrame;

        public PlayerHandle PlayerHandle { get; }
        public int LastConfirmedFrame { get; set; }
        public float Ping { get; set; }

        public float EstimatedLocalFrame => _lastAddedFrame + Ping / Constants.MS_PER_FRAME;

        internal Player(PlayerType playerType)
        {
            PlayerHandle = new PlayerHandle(Guid.NewGuid().GetHashCode(), playerType);
            _savedStates = new RingBuffer<InputState>(Constants.FRAME_BUFFER_SIZE);
            LastConfirmedFrame = Constants.NULL_FRAME;
            _lastAddedFrame = Constants.NULL_FRAME;
        }

        public bool AddInput(int frame, TState state)
        {
            // If input already registered for this frame, ignore it
            if (frame <= _lastAddedFrame) return false;

            Debug.Assert(frame == _lastAddedFrame + 1, "Frames has to be added sequentially");

            _lastAddedFrame = frame;
            _savedStates[frame] = new InputState {State = state, Frame = frame};

            if (PlayerHandle.Type == PlayerType.Remote
            &&  LastConfirmedFrame == Constants.NULL_FRAME
            &&  !_savedStates[frame - 1].State.Equals(state))
            {
                LastConfirmedFrame = frame;
            }

            return true;
        }

        public TState GetInput(int frame)
        {
            frame = Math.Max(0, Math.Min(frame, _lastAddedFrame));

            var input = _savedStates[frame];
            
            Debug.Assert(input.Frame == frame, "Tried to get Input from a discarded frame.");
            
            return input.State;
        }
    }
}