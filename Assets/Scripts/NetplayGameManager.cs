using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Common;
using Logic;
using Network;
using Network.Rollback;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class NetplayGameManager : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    private struct PingPackage
    {
        public uint Id;
        public long Created;
        public long Received;
    }
    
    private static NetplayGameManager _instance;

    private GameSynchronizer<GameState, InputState> _synchronizer;
    private DiscordLobby _lobby;

    private GameState _gameState;
    private bool _gameStarted;
    
    private PlayerHandle _localPlayer;
    private PlayerHandle _remotePlayer;
    private uint _pingPackageId;

    [SerializeField] private Transform _p1Transform;
    [SerializeField] private Transform _p2Transform;

    private void Awake()
    {
        if (_instance != null)
        {
            throw new Exception($"Only one instance of {nameof(NetplayGameManager)} may exist.");
        }
        
        DontDestroyOnLoad(this);
        _instance = this;
    }

    private void Start()
    {
        _gameState.Player1.Position = ((float3) _p1Transform.position).xy;
        _gameState.Player2.Position = ((float3) _p2Transform.position).xy;
    }

    public static void JoinMatch(DiscordLobby lobby) => _instance.JoinMatchInternal(lobby);
    private async void JoinMatchInternal(DiscordLobby lobby)
    {
        if (_lobby != null) throw new NotImplementedException();
        
        _lobby = lobby;
        await _lobby.Connect();
        _lobby.ConnectNetwork();
        _lobby.NetworkMessageReceived += NetworkMessageReceived;
        
        _synchronizer = new GameSynchronizer<GameState, InputState>(SaveGame, LoadGame, SimulateGame, BroadcastInput);
        _remotePlayer = _synchronizer.AddPlayer(PlayerType.Remote);
        _localPlayer  = _synchronizer.AddPlayer(PlayerType.Local);

        _gameStarted = true;
        _lobby.SendNetworkMessage(0, Encoding.UTF8.GetBytes("START"));
    }
    
    public static void CreateMatch(DiscordLobby lobby) => _instance.CreateMatchInternal(lobby);
    private void CreateMatchInternal(DiscordLobby lobby)
    {
        if (_lobby != null) throw new NotImplementedException();

        _lobby = lobby;
        _lobby.ConnectNetwork();
        _lobby.NetworkMessageReceived += NetworkMessageReceived;
        
        _synchronizer = new GameSynchronizer<GameState, InputState>(SaveGame, LoadGame, SimulateGame, BroadcastInput);
        _localPlayer  = _synchronizer.AddPlayer(PlayerType.Local);
        _remotePlayer = _synchronizer.AddPlayer(PlayerType.Remote);

        _gameStarted = true;
    }

    private void Update()
    {
        if (_lobby == null) return;
        
        PingPlayers();

        if (!_gameStarted) return;
    
        _synchronizer.AddLocalInput(_localPlayer, ReadLocalInput());
        _synchronizer.Update(Time.deltaTime * 1000);
        
        _p1Transform.position = (Vector2) _gameState.Player1.Position.xy;
        _p2Transform.position = (Vector2) _gameState.Player2.Position.xy;
    }

    private void PingPlayers()
    {
        _lobby.SendNetworkMessage(2, new PingPackage
        {
            Id = _pingPackageId,
            Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }
        .ToBytes());
        
        _pingPackageId++;
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

    private GameState SaveGame()
    {
        return _gameState;
    }

    private void LoadGame(GameState gameState)
    {
        _gameState = gameState;
    }

    private void SimulateGame(InputState[] inputStates)
    {
        _gameState.Player1.Update(inputStates[0]);
        _gameState.Player2.Update(inputStates[1]);
    }
    
    private void BroadcastInput(PlayerHandle player, int frame, InputState state)
    {
        _lobby.SendNetworkMessage(1, state.CreatePackage(frame));
    }

    private void NetworkMessageReceived(long userId, byte channelId, byte[] data)
    {
        switch (channelId)
        {
            case 0:
            {
                if (Encoding.UTF8.GetString(data) == "START")
                {
                    _gameStarted = true;
                }
                
                break;
            }
            case 1:
            {
                var (frame, input) = InputState.ReadPackage(data);
                _synchronizer.AddRemoteInput(_remotePlayer, frame, input);
                
                break;
            }
            case 2:
            {
                var package = data.ToStruct<PingPackage>();
                var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                if (package.Received == 0)
                {
                    package.Received = now;
                    _lobby.SendNetworkMessage(userId, 2, package.ToBytes());
                }
                else
                {
                    var rtt = now - package.Created;
                    var ping = package.Received - package.Created;
                    var pong = now - package.Received;

                    _synchronizer.SetPing(_remotePlayer, rtt / 2f);
                    Debug.Log($"Ping Complete, Id:{package.Id}, RTT:{rtt}ms, Ping:{ping}, Pong:{pong}");
                }

                break;
            }
        }
    }
}
