using System;
using Logic.Characters;
using Network.Rollback;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.GameManagers
{
    public class TrainingModeManager : MonoBehaviour
    {
        [SerializeField] private Character _player1Character;
        [SerializeField] private Transform _player1Transform;
        [SerializeField] private Character _player2Character;
        [SerializeField] private Transform _player2Transform;
    
        private GameSynchronizer<GameState, InputState> _gameSynchronizer;
        private PlayerHandle _playerHandle;
        private GameState _gameState;

        private void Awake()
        {
            _gameSynchronizer = new GameSynchronizer<GameState, InputState>
            (
                SaveGame,
                LoadGame,
                SimulateGame,
                BroadcastInput
            );

            _playerHandle = _gameSynchronizer.AddPlayer(PlayerType.Local);

            _gameState.Player1 = new PlayerState(_player1Character, (Vector2) _player1Transform.position);
            _gameState.Player2 = new PlayerState(_player2Character, (Vector2) _player2Transform.position);
        }

        private void SimulateGame(InputState[] inputStates)
        {
            _gameState.Update(inputStates[0], new InputState());
        }

        private static void BroadcastInput(PlayerHandle player, int frame, InputState state) {}
        private void LoadGame(GameState gameState) => _gameState = gameState;
        private GameState SaveGame() => _gameState;

        private void Update()
        {
            _gameSynchronizer.AddLocalInput(_playerHandle, InputState.ReadLocalInputs());
            _gameSynchronizer.Update(Time.deltaTime * 1000);

            _player1Transform.position = (Vector2) _gameState.Player1.Position;
            _player2Transform.position = (Vector2) _gameState.Player2.Position;
            _gameState.DrawHitboxes();
        }
    }
}
