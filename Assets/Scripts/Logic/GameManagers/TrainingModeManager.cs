using Common;
using Logic.Characters;
using Network.Rollback;
using UnityEngine;

namespace Logic.GameManagers
{
    public class TrainingModeManager : MonoBehaviour
    {
        [SerializeField] private Transform _player1Transform;
        [SerializeField] private Transform _player2Transform;
    
        private GameSynchronizer<GameState, InputState> _gameSynchronizer;
        private PlayerHandle _playerHandle;
        
        [SerializeField] private GameState _gameState;

        private void Awake()
        {
            _gameSynchronizer = new GameSynchronizer<GameState, InputState>();
            _gameSynchronizer.SimulateGame += SimulateGame;
            _gameSynchronizer.SaveGame += SaveGame;
            _gameSynchronizer.LoadGame += LoadGame;

            _playerHandle = _gameSynchronizer.AddPlayer(PlayerType.Local);
        }

        private void SimulateGame(InputState[] inputStates)
        {
            _gameState.Update(inputStates[0], new InputState());
        }

        private GameState SaveGame() => _gameState;
        private void LoadGame(GameState gameState) => _gameState = gameState;

        private void Update()
        {
            _gameSynchronizer.AddLocalInput(_playerHandle, InputState.ReadLocalInputs());
            _gameSynchronizer.Update(Time.deltaTime * 1000);
            
            _player1Transform.position = new Vector2(_gameState.Player1.Position.x, _gameState.Player1.Position.y);
            _player2Transform.position = new Vector2(_gameState.Player2.Position.x, _gameState.Player2.Position.y);
            _gameState.DrawHitboxes();
        }
    }
}
