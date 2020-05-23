using System;
using System.Dynamic;
using Discord;
using UnityEngine;


namespace Network
{
    public class DiscordManager : MonoBehaviour
    {
        private static DiscordManager _instance;
        public static Discord.Discord Discord => _instance._discord;
        public static User CurrentUser { get; private set; }

        private Discord.Discord _discord;
        
        private void Awake()
        {
            if (_instance != null) throw new Exception($"You can only have one {nameof(DiscordManager)}.");
            DontDestroyOnLoad(this);
            _instance = this;
            _discord = new Discord.Discord(Secrets.DISCORD_CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);

            var userManager = Discord.GetUserManager();
            userManager.OnCurrentUserUpdate += () =>
            {
                CurrentUser = userManager.GetCurrentUser();
            };
        }

        private void LateUpdate()
        {
            _discord.GetLobbyManager().FlushNetwork();
            _discord.RunCallbacks();
        }

        private void OnApplicationQuit()
        {
            _discord?.Dispose();
        }
    }
}