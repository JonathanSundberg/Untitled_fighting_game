using System;
using System.Threading.Tasks;
using Discord;

namespace Network
{
    public class DiscordLobby
    {
        private readonly Discord.Discord _discord;
        private readonly Lobby _lobby;

        public long Id => _lobby.Id;
        public long OwnerId => _lobby.OwnerId;
        public uint Capacity => _lobby.Capacity;
        public LobbyType Type => _lobby.Type;
        public bool Locked => _lobby.Locked;
        public string Secret => _lobby.Secret;
        public Task<User> Owner { get
        {
            var taskSource = new TaskCompletionSource<User>();
            
            _discord.GetUserManager().GetUser(_lobby.OwnerId, (Result result, ref User user) =>
            {
                if (result != Result.Ok)
                {
                    taskSource.SetException(new ResultException(result));
                    return;
                }
                
                taskSource.SetResult(user);
            });
            
            return taskSource.Task;
        }}

        public delegate void NetworkMessageCallback(long userId, byte channelId, byte[] message);
        public event NetworkMessageCallback NetworkMessageReceived;

        public delegate void ModifyLobbyTransaction(ref LobbyTransaction transaction);
        public static Task<DiscordLobby> Create(ModifyLobbyTransaction modifyCallback = null)
        {
            var discord = DiscordManager.Discord;
            var lobbyManager = discord.GetLobbyManager();
            var transaction = lobbyManager.GetLobbyCreateTransaction();
            
            modifyCallback?.Invoke(ref transaction);

            var taskSource = new TaskCompletionSource<DiscordLobby>();
            lobbyManager.CreateLobby(transaction, (Result result, ref Lobby lobby) =>
            {
                if (result != Result.Ok)
                {
                    taskSource.SetException(new ResultException(result));
                    return;
                }
                    
                taskSource.SetResult(new DiscordLobby(discord, lobby));
            });

            return taskSource.Task;
        }
            
        public delegate void ModifyLobbySearchQuery(ref LobbySearchQuery searchQuery);
        public static Task<DiscordLobby[]> Search(ModifyLobbySearchQuery modifyCallback = null)
        {
            var discord = DiscordManager.Discord;
            var lobbyManager = discord.GetLobbyManager();
            var searchQuery = lobbyManager.GetSearchQuery();
            
            modifyCallback?.Invoke(ref searchQuery);

            var taskSource = new TaskCompletionSource<DiscordLobby[]>();
            lobbyManager.Search(searchQuery, result =>
            {
                if (result != Result.Ok)
                {
                    taskSource.SetException(new ResultException(result));
                    return;
                }

                var lobbyCount = lobbyManager.LobbyCount();
                var lobbies = new DiscordLobby[lobbyCount];

                for (var lobbyIndex = 0; lobbyIndex < lobbyCount; lobbyIndex++)
                {
                    var lobbyId = lobbyManager.GetLobbyId(lobbyIndex);
                    var lobby = lobbyManager.GetLobby(lobbyId);

                    lobbies[lobbyIndex] = new DiscordLobby(discord, lobby);
                }
                
                taskSource.SetResult(lobbies);
            });

            return taskSource.Task;
        }

        private DiscordLobby(Discord.Discord discord, Lobby lobby)
        {
            _discord = discord;
            _lobby = lobby;
        }

        public User[] GetMembers()
        {
            var lobbyManager = _discord.GetLobbyManager();
            var memberCount = lobbyManager.MemberCount(_lobby.Id);
            var members = new User[memberCount];
            
            for (var memberIndex = 0; memberIndex < memberCount; memberIndex++)
            {
                var userId = lobbyManager.GetMemberUserId(_lobby.Id, memberIndex);
                members[memberIndex] = lobbyManager.GetMemberUser(_lobby.Id, userId);
            }

            return members;
        }

        public Task<bool> Delete()
        {
            var taskSource = new TaskCompletionSource<bool>();

            var lobbyManager = _discord.GetLobbyManager();
            lobbyManager.DeleteLobby(_lobby.Id, result =>
            {
                taskSource.SetResult(result == Result.Ok);
            });

            return taskSource.Task;
        }

        public Task Connect()
        {
            var lobbyManager = _discord.GetLobbyManager();
            
            var taskSource = new TaskCompletionSource<object>();
            lobbyManager.ConnectLobby(_lobby.Id, _lobby.Secret, (Result result, ref Lobby lobby) =>
            {
                if (result != Result.Ok)
                {
                    taskSource.SetException(new ResultException(result));
                    return;
                }
                
                taskSource.SetResult(null);
            });

            return taskSource.Task;
        }
        
        public Task Disconnect()
        {
            var lobbyManager = _discord.GetLobbyManager();
            
            var taskSource = new TaskCompletionSource<object>();
            lobbyManager.DisconnectLobby(_lobby.Id, result =>
            {
                if (result != Result.Ok)
                {
                    taskSource.SetException(new ResultException(result));
                    return;
                }
                
                taskSource.SetResult(null);
            });

            return taskSource.Task;
        }
            
        public void ConnectNetwork()
        {
            var lobbyManager = _discord.GetLobbyManager();
            lobbyManager.ConnectNetwork(_lobby.Id);
            lobbyManager.OpenNetworkChannel(_lobby.Id, 0, true);
            lobbyManager.OpenNetworkChannel(_lobby.Id, 1, false);
            lobbyManager.OpenNetworkChannel(_lobby.Id, 2, false);
            lobbyManager.OnNetworkMessage += OnNetworkMessage;
        }

        public void DisconnectNetwork()
        {
            var lobbyManager = _discord.GetLobbyManager();
            lobbyManager.DisconnectNetwork(_lobby.Id);
            lobbyManager.OnNetworkMessage -= OnNetworkMessage;
        }

        public void SendNetworkMessage(byte channelId, byte[] message)
        {
            var lobbyManager = _discord.GetLobbyManager();

            var memberCount = lobbyManager.MemberCount(_lobby.Id);
            for (var memberIndex = 0; memberIndex < memberCount; memberIndex++)
            {
                var memberUserId = lobbyManager.GetMemberUserId(_lobby.Id, memberIndex);
                lobbyManager.SendNetworkMessage(_lobby.Id, memberUserId, channelId, message);
            }
        }
        
        public void SendNetworkMessage(long userId, byte channelId, byte[] message)
        {
            var lobbyManager = _discord.GetLobbyManager();
            lobbyManager.SendNetworkMessage(_lobby.Id, userId, channelId, message);
        }

        private void OnNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
        {
            if (lobbyId != _lobby.Id) return;
            NetworkMessageReceived?.Invoke(userId, channelId, data);
        }
    }
}