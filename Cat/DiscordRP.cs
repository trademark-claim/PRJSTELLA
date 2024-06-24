using Discord;

namespace Cat
{
    internal static class DiscordRP
    {
        private static readonly Discord.Discord discord = new(DCID, (UInt64)Discord.CreateFlags.Default);

        [CAspects.Logging]
        internal static Discord.Activity LoadBaseActivity()
        {
            return new Discord.Activity
            {
                State = "Building Version 0.15.7",
                Details = "Stella",
                Type = ActivityType.Streaming,
                Timestamps =
                {
                    Start = 1701349260,
                    End = 1719151260
                },
                Assets =
                {
                    LargeImage = "blackhole",
                    LargeText = "Going Insane; Programmer - Level 0x12C",
                    SmallImage = "Stellav1",
                    SmallText = "Struggling with a real girlfriend so I'm building my own :3",
                },
                Party =
                {
                    Id = "3",
                    Size =
                    {
                        CurrentSize = 1,
                        MaxSize = 3,
                    },
                },
                Secrets =
                {
                    Join = "abc",
                    Spectate = "abc",
                },
                Instance = true,
            };
        }

        [CAspects.Logging]
        internal static void SetActivity(Discord.Activity activity)
        {
            discord.GetActivityManager().UpdateActivity(activity, (result) => { Logging.Log("==DISCORD LOG== " + result); });
            discord.RunCallbacks();
        }

        [CAspects.Logging]
        internal static void Init()
        {
            discord.SetLogHook(LogLevel.Debug, (l, m) => Logging.Log($"==DISCORD LOG== ({l}): {m}"));
            var am = discord.GetActivityManager();
            am.OnActivityJoinRequest += (ref Discord.User user) =>
            {
                Logging.Log($"==DISCORD LOG== User {user.Username} ({user.Id}) has clicked the 'Ask to Join' button!");
                am.SendInvite(user.Id, Discord.ActivityActionType.Join, "DM Nexus if you're interested in the Project!", (result) =>
                {
                    if (result == Discord.Result.Ok)
                    {
                        Logging.Log("==DISCORD LOG== Invite message successful!");
                    }
                    else
                    {
                        Logging.Log("==DISCORD LOG== Invite message unsuccessful!");
                    }
                });
            };
        }
    }
}