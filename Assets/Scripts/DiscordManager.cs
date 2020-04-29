using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using UnityEngine;

public class DiscordManager : MonoBehaviour
{
    private static DiscordManager _instance;
    
    #if DISCORD_DEBUG
    public static bool UseSecondInstance;
    private Discord.Discord _discord0;
    private Discord.Discord _discord1;
    private Discord.Discord _discord => UseSecondInstance ? _discord0 : _discord1;
    #else
    private Discord.Discord _discord;
    #endif

    // Start is called before the first frame update
    private void Awake()
    {
        if (_instance != null)
        {
            throw new Exception("You can only have one DiscordManager.");
        }
        
        DontDestroyOnLoad(this);
        
        #if DISCORD_DEBUG
        Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "0");
        _discord0 = new Discord.Discord(Secrets.DISCORD_CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);
        Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "1");
        _discord1 = new Discord.Discord(Secrets.DISCORD_CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);
        #else
        _discord = new Discord.Discord(Secrets.DISCORD_CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);
        #endif

        _instance = this;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _discord.RunCallbacks();
    }

    public static Task<Lobby> CreateLobby
    (
        uint capacity = 2,
        bool is_public = true,
        (string key, string value)[] metadata = null
    ) 
    => _instance.CreateLobbyInternal(capacity, is_public, metadata);

    private Task<Lobby> CreateLobbyInternal
    (
        uint capacity = 2,
        bool is_public = true,
        (string key, string value)[] metadata = null
    )
    {
        var lobbyManager = _discord.GetLobbyManager();
        var transaction = lobbyManager.GetLobbyCreateTransaction();
        
        transaction.SetCapacity(capacity);
        transaction.SetType(is_public ? LobbyType.Public : LobbyType.Private);

        if (metadata != null)
        {
            foreach (var (key, value) in metadata)
            {
                transaction.SetMetadata(key, value);
            }
        }
        
        var taskCompletionSource = new TaskCompletionSource<Lobby>();
        lobbyManager.CreateLobby(transaction, (Result result, ref Lobby lobby) =>
        {
            if (result != Result.Ok)
            {
                taskCompletionSource.SetException(new Exception(result.ToString()));
                return;
            }

            taskCompletionSource.SetResult(lobby);
        });

        return taskCompletionSource.Task;
    }

    public static Task<long[]> SearchForLobbies() => _instance.SearchForLobbiesInternal();
    private Task<long[]> SearchForLobbiesInternal()
    {
        var lobbyManager = _discord.GetLobbyManager();

        var searchQuery = lobbyManager.GetSearchQuery();

        var taskCompletionSource = new TaskCompletionSource<long[]>();
        lobbyManager.Search(searchQuery, result =>
        {
            if (result != Result.Ok)
            {
                taskCompletionSource.SetException(new Exception(result.ToString()));
                return;
            }

            var lobbyCount = lobbyManager.LobbyCount();
            var lobbyIds = new long[lobbyCount];

            for (var lobbyIndex = 0; lobbyIndex < lobbyCount; lobbyIndex++)
            {
                lobbyIds[lobbyIndex] = lobbyManager.GetLobbyId(lobbyIndex);
            }

            taskCompletionSource.SetResult(lobbyIds);
        });

        return taskCompletionSource.Task;
    }
}
