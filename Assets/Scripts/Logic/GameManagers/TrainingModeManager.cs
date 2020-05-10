using Network.Rollback;
using Unity.Mathematics;
using UnityEngine;

namespace Logic.GameManagers
{
    public class TrainingModeManager : MonoBehaviour
    {
        [SerializeField] private Transform _player1Transform;
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

            _gameState.Player1.Position = (Vector2) _player1Transform.position;
            _gameState.Player2.Position = (Vector2) _player2Transform.position;
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
            _gameSynchronizer.AddLocalInput(_playerHandle, ReadLocalInput());
            _gameSynchronizer.Update(Time.deltaTime * 1000);

            _player1Transform.position = (Vector2) _gameState.Player1.Position;
            _player2Transform.position = (Vector2) _gameState.Player2.Position;
        }
    
        private static InputState ReadLocalInput()
        {
            return new InputState
            {
                Direction = new int2
                (
                    (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ?  1 : 0), 
                    (Input.GetKey(KeyCode.W) ?  1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0)
                ),
            
                A = Input.GetKey(KeyCode.U), 
                B = Input.GetKey(KeyCode.I),
                C = Input.GetKey(KeyCode.O),
                D = Input.GetKey(KeyCode.P)
            };
        }
    }
}
