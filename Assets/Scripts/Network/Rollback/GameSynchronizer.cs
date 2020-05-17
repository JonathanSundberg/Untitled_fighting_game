using System;
using System.Collections.Generic;
using System.Threading;

namespace Network.Rollback
{
    public class GameSynchronizer<TGameState, TInputState>
    where TGameState  : struct
    where TInputState : struct, IEquatable<TInputState>
    {
        private struct GameState
        {
            public TGameState State;
            public int Frame;
        }

        public delegate TGameState SaveGameFn();
        public delegate void LoadGameFn(TGameState gameState);
        public delegate void SimulateGameFn(TInputState[] inputStates);
        public delegate void BroadcastInputFn(PlayerHandle player, int frame, TInputState state);

        private readonly SaveGameFn _saveGame;
        private readonly LoadGameFn _loadGame;
        private readonly SimulateGameFn _simulateGame;
        private readonly BroadcastInputFn _broadcastInput;
        
        private readonly Mutex _rollbackMutex;
        private readonly List<Player<TInputState>> _players;
        private RingBuffer<GameState> _savedStates;
        private TInputState[] _playerInputStates;
        
        private int _currentFrame;
        private float _timeSpentOnFrame;

        public GameSynchronizer
        (
            SaveGameFn saveGame,
            LoadGameFn loadGame,
            SimulateGameFn simulateGame,
            BroadcastInputFn broadcastInput
        )
        {
            _saveGame = saveGame;
            _loadGame = loadGame;
            _simulateGame = simulateGame;
            _broadcastInput = broadcastInput;
            
            _rollbackMutex = new Mutex();
            _players = new List<Player<TInputState>>();
            _savedStates = new RingBuffer<GameState>(Constants.FRAME_BUFFER_SIZE);
        }

        private int FindStateIndex(int frame)
        {
            var stateIndex = 0;
            for (; stateIndex < _savedStates.Length; stateIndex++)
            {
                if (_savedStates[stateIndex].Frame == frame) break;
            }

            return stateIndex != _savedStates.Length ? stateIndex : -1;
        }

        public void Update(float deltaTimeMs)
        {
            _rollbackMutex.WaitOne();
            
            var latestSynchronizedFrame = GetLatestSynchronizedFrame();
            if (latestSynchronizedFrame != _currentFrame)
            {
                var stateIndex = FindStateIndex(latestSynchronizedFrame);
                
                Debug.Assert(stateIndex != -1, "Can't rollback to a discarded frame");
                
                var latestSynchronizedState = _savedStates[stateIndex];
                _loadGame(latestSynchronizedState.State);

                var framesToResimulate = _currentFrame - latestSynchronizedFrame;
                for (var frameIndex = 0; frameIndex < framesToResimulate; frameIndex++)
                {
                    var resimulateFrame = latestSynchronizedFrame + frameIndex;
                    if (resimulateFrame != latestSynchronizedState.Frame)
                    {
                        _savedStates[resimulateFrame] = new GameState
                        {
                            State = _saveGame(), 
                            Frame = resimulateFrame
                        };
                    }

                    _simulateGame(GetInputs(resimulateFrame));
                }
            }
            
            _rollbackMutex.ReleaseMutex();

            var highestFrameAdvantage = 0f;
            foreach (var player in _players)
            {
                var estimatedFrameAdvantage = _currentFrame - player.EstimatedLocalFrame;
                highestFrameAdvantage = Math.Max(highestFrameAdvantage, estimatedFrameAdvantage);
            }

            _timeSpentOnFrame = Math.Max(0, _timeSpentOnFrame - highestFrameAdvantage * Constants.FRAME_DELAY_FACTOR);

            _timeSpentOnFrame += deltaTimeMs;
            if (_timeSpentOnFrame < Constants.MS_PER_FRAME) return;
            _timeSpentOnFrame -= Constants.MS_PER_FRAME;
            
            _savedStates[_currentFrame] = new GameState
            {
                State = _saveGame(),
                Frame = _currentFrame
            };

            _simulateGame(GetInputs(_currentFrame));

            _currentFrame++;
        }

        private int GetLatestSynchronizedFrame()
        {
            var latestSynchronizedFrame = _currentFrame;

            foreach (var player in _players)
            {
                if (player.LastConfirmedFrame == Constants.NULL_FRAME) continue;
                if (player.LastConfirmedFrame >= latestSynchronizedFrame) continue;
                latestSynchronizedFrame = player.LastConfirmedFrame;
                player.LastConfirmedFrame = Constants.NULL_FRAME;
            }

            return latestSynchronizedFrame;
        }
        
        public PlayerHandle AddPlayer(PlayerType playerType)
        {
            var player = new Player<TInputState>(playerType);
            
            _players.Add(player);
            _playerInputStates = new TInputState[_players.Count];
            
            return player.PlayerHandle;
        }

        private Player<TInputState> GetPlayer(PlayerHandle playerHandle)
        {
            foreach (var player in _players)
            {
                if (player.PlayerHandle == playerHandle) return player;
            }

            Debug.Assert(false, "Could not get Player from PlayerHandle");
            return default;
        }

        public void AddLocalInput(PlayerHandle playerHandle, TInputState inputState)
        {
            Debug.Assert(playerHandle.Type == PlayerType.Local, "Only local players can use AddLocalInput");
            
            var inputAccepted = GetPlayer(playerHandle).AddInput(_currentFrame, inputState);
            if (inputAccepted) _broadcastInput(playerHandle, _currentFrame, inputState);
        }
        
        public void AddRemoteInput(PlayerHandle playerHandle, int frame, TInputState inputState)
        {
            Debug.Assert(playerHandle.Type == PlayerType.Remote, "Only remote players can use AddRemoteInput");
            
            var player = GetPlayer(playerHandle);
            
            _rollbackMutex.WaitOne();
            player.AddInput(frame, inputState);
            _rollbackMutex.ReleaseMutex();
        }

        public void SetPing(PlayerHandle playerHandle, float ms)
        {
            GetPlayer(playerHandle).Ping = ms;
        }
        
        public float GetPing(PlayerHandle playerHandle)
        {
            return GetPlayer(playerHandle).Ping;
        }

        private TInputState[] GetInputs(int frame)
        {
            for (var playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                _playerInputStates[playerIndex] = _players[playerIndex].GetInput(frame);
            }

            return _playerInputStates;
        }
    }
}