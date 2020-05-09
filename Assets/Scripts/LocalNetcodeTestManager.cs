using System;
using System.Collections.Generic;
using Network.Rollback;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class LocalNetcodeTestManager : MonoBehaviour
{
    private GameSynchronizer<GameState, InputState> _gameSynchronizer;

    public struct GameState : IEquatable<GameState>
    {
        public Vector2 Player1Position;
        public Vector2 Player2Position;

        public bool Equals(GameState other)
        {
            return Player1Position == other.Player1Position
                && Player2Position == other.Player2Position;
        }
    }

    public struct InputState : IEquatable<InputState>
    {
        public Vector2 Input;

        public bool Equals(InputState other)
        {
            return Input == other.Input;
        }
    }

    private GameState _gameState;
    
    private PlayerHandle _p1Handle;
    private PlayerHandle _p2Handle;
    
    [SerializeField] private Transform _p1Transform;
    [SerializeField] private Transform _p2Transform;
    [SerializeField] private Slider _p2Delay;

    private void Start()
    {
        _gameState.Player1Position = _p1Transform.position;
        _gameState.Player2Position = _p2Transform.position;

        _gameSynchronizer = new GameSynchronizer<GameState, InputState>
        (
            SaveGame, 
            LoadGame,
            SimulateGame,
            BroadcastInput
        );
        
        _p1Handle = _gameSynchronizer.AddPlayer(PlayerType.Local);
        _p2Handle = _gameSynchronizer.AddPlayer(PlayerType.Remote);
    }

    private readonly Queue<(float timeWithDelay, float delay, int frame, InputState state)> _remoteInputs 
        = new Queue<(float, float, int, InputState)>();
    
    private double _previousUpdate;

    private void Update()
    {
        Profiler.BeginSample("Add Local Input");
        var playerInput = new InputState
        {
            Input = new Vector2
            (
                Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0,
                Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0
            )
        };
        
        _gameSynchronizer.AddLocalInput(_p1Handle, playerInput);
        Profiler.EndSample();
        
        Profiler.BeginSample("Add Remote Input");
        while (_remoteInputs.Count > 0 && _remoteInputs.Peek().timeWithDelay < Time.time)
        {
            var (_, delay, frame, input) = _remoteInputs.Dequeue();
            _gameSynchronizer.SetPing(_p2Handle, delay);
            _gameSynchronizer.AddRemoteInput(_p2Handle, frame, input);
        }
        Profiler.EndSample();

        Profiler.BeginSample("Update Game Synchronizer");

        _gameSynchronizer.Update(Time.deltaTime * 1000);
        
        _p1Transform.position = _gameState.Player1Position;
        _p2Transform.position = _gameState.Player2Position;
        Profiler.EndSample();
    }
    
    private GameState SaveGame()
    {
        return _gameState;
    }
    
    private void LoadGame(GameState gameState)
    {
        _gameState = gameState;
    }

    private void SimulateGame(InputState[] playerInputs)
    {
        _gameState.Player1Position += playerInputs[0].Input;
        _gameState.Player2Position += playerInputs[1].Input;
    }
    
    private void BroadcastInput(PlayerHandle player, int frame, InputState state)
    {
        var delay = _p2Delay.value;
        _remoteInputs.Enqueue((Time.time + delay / 1000, delay, frame, state));
    }

}