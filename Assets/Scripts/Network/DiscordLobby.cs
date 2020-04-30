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
        
        public delegate void NetworkMessageCallback(long userId, byte[] message);
        public event NetworkMessageCallback OnNetworkMessage;

        public delegate void ModifyLobbyTransaction(ref LobbyTransaction transaction);
        public static Task<DiscordLobby> Create(ModifyLobbyTransaction modifyCallback = null)
        {
            var discord = DiscordWrapper.Discord;
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
            var discord = DiscordWrapper.Discord;
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
            
        public void ConnectNetwork()
        {
            var lobbyManager = _discord.GetLobbyManager();
            lobbyManager.ConnectNetwork(_lobby.Id);
            lobbyManager.OpenNetworkChannel(_lobby.Id, 0, true);
            lobbyManager.OnNetworkMessage += NetworkMessage;
        }

        public void DisconnectNetwork()
        {
            var lobbyManager = _discord.GetLobbyManager();
            lobbyManager.DisconnectNetwork(_lobby.Id);
            lobbyManager.OnNetworkMessage -= NetworkMessage;
        }

        public void SendNetworkMessage(byte[] message)
        {
            var lobbyManager = _discord.GetLobbyManager();

            var memberCount = lobbyManager.MemberCount(_lobby.Id);
            for (var memberIndex = 0; memberIndex < memberCount; memberIndex++)
            {
                var memberUserId = lobbyManager.GetMemberUserId(_lobby.Id, memberIndex);
                lobbyManager.SendNetworkMessage(_lobby.Id, memberUserId, 0, message);
            }
        }

        private void NetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
        {
            if (lobbyId != _lobby.Id) return;
            OnNetworkMessage?.Invoke(userId, data);
        }
    }
}