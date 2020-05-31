using System;
using System.Collections.Generic;
using Common;
using Logic;
using Network.Rollback;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Input = UnityEngine.Input;

public class LocalNetcodeTestManager : MonoBehaviour
{
    private GameSynchronizer<GameState, InputState> _gameSynchronizer;

    private PlayerHandle _p1Handle;
    private PlayerHandle _p2Handle;
    
    [SerializeField] private GameState _gameState;
    [SerializeField] private Transform _p1Transform;
    [SerializeField] private Transform _p2Transform;
    [SerializeField] private Slider _p2Delay;

    private void Start()
    {
        _gameSynchronizer = new GameSynchronizer<GameState, InputState>();
        _gameSynchronizer.SimulateGame += SimulateGame;
        _gameSynchronizer.SaveGame += SaveGame; 
        _gameSynchronizer.LoadGame += LoadGame;
        _gameSynchronizer.BroadcastInput += BroadcastInput;
        
        _p1Handle = _gameSynchronizer.AddPlayer(PlayerType.Local);
        _p2Handle = _gameSynchronizer.AddPlayer(PlayerType.Remote);
    }

    private readonly Queue<(float timeWithDelay, float delay, int frame, InputState state)> _remoteInputs 
        = new Queue<(float, float, int, InputState)>();
    
    private double _previousUpdate;

    private void Update()
    {
        _gameSynchronizer.AddLocalInput(_p1Handle, InputState.ReadLocalInputs());
        
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
        
        _p1Transform.position = new Vector2(_gameState.Player1.Position.x, _gameState.Player1.Position.y);
        _p2Transform.position = new Vector2(_gameState.Player2.Position.x, _gameState.Player2.Position.y);
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
        _gameState.Update(playerInputs[0], playerInputs[1]);
    }
    
    private void BroadcastInput(PlayerHandle player, int frame, InputState state)
    {
        var delay = _p2Delay.value;
        _remoteInputs.Enqueue((Time.time + delay / 1000, delay, frame, state));
    }

}