using System;
using Discord;
using UnityEngine;


namespace Network
{
    public class DiscordManager : MonoBehaviour
    {
        private static DiscordManager _instance;
        public static Discord.Discord Discord => _instance._discord;
        public static User CurrentUser { get; private set; }

#if DISCORD_DEBUG
        public static bool UseSecondInstance;
        private Discord.Discord _discord0;
        private Discord.Discord _discord1;
        private Discord.Discord _discord => UseSecondInstance ? _discord0 : _discord1;
#else
        private Discord.Discord _discord;
#endif
        
        private void Awake()
        {
            if (_instance != null)
            {
                throw new Exception($"You can only have one {nameof(DiscordManager)}.");
            }

            DontDestroyOnLoad(this);
            _instance = this;

#if DISCORD_DEBUG
            Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "0");
            _discord0 = new Discord.Discord(Secrets.DISCORD_CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);
            
            Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "1");
            _discord1 = new Discord.Discord(Secrets.DISCORD_CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);
#else
            Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "0");
            _discord = new Discord.Discord(Secrets.DISCORD_CLIENT_ID, (ulong) CreateFlags.NoRequireDiscord);
#endif

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
            _discord.Dispose();
        }
    }
}