using System;
using System.Collections.Generic;

namespace GamerWingman.Data
{
    /// <summary>
    /// User profile data structure stored in SQLite and synced to Firestore.
    /// Tracks playtime, achievements, mood, and gaming preferences.
    /// </summary>
    [System.Serializable]
    public class UserProfile
    {
        public string userId;
        public DateTime createdAt;
        public DateTime lastPlayTime;
        public long totalPlaytimeSeconds;
        public int currentStreak;
        public string mood; // happy, sad, thinking, excited
        public string playstyleSummary;
        public bool isPremium;
        public Dictionary<string, long> playtimePerGame;
        public List<string> friends;
        public Dictionary<string, int> moodCounters; // happy_count, sad_count, etc.

        public UserProfile()
        {
            userId = "";
            createdAt = DateTime.Now;
            lastPlayTime = DateTime.Now;
            totalPlaytimeSeconds = 0;
            currentStreak = 0;
            mood = "happy";
            playstyleSummary = "New player";
            isPremium = false;
            playtimePerGame = new Dictionary<string, long>();
            friends = new List<string>();
            moodCounters = new Dictionary<string, int>
            {
                { "happy", 0 },
                { "sad", 0 },
                { "thinking", 0 },
                { "excited", 0 }
            };
        }

        public override string ToString()
        {
            return $"[UserProfile] ID: {userId}, Mood: {mood}, Premium: {isPremium}, " +
                   $"Playtime: {totalPlaytimeSeconds / 3600}h, Streak: {currentStreak}";
        }
    }

    /// <summary>
    /// Gaming session record
    /// </summary>
    [System.Serializable]
    public class GameSession
    {
        public string sessionId;
        public string userId;
        public string gameName;
        public DateTime startTime;
        public DateTime endTime;
        public long durationSeconds;
        public bool won;
        public int score;
        public List<string> squadMembers;

        public GameSession()
        {
            sessionId = System.Guid.NewGuid().ToString();
            squadMembers = new List<string>();
        }
    }

    /// <summary>
    /// Platform account link
    /// </summary>
    [System.Serializable]
    public class PlatformAccount
    {
        public string platform; // "steam", "xbox", "psn", "epic", "github"
        public string accountId;
        public string accountName;
        public string accessToken;
        public string refreshToken;
        public long tokenExpiresAt;
        public Dictionary<string, object> profileData;
    }

    /// <summary>
    /// Squad member recommendation
    /// </summary>
    [System.Serializable]
    public class SquadMate
    {
        public string userId;
        public string username;
        public string avatar;
        public float compatibilityScore; // 0-100
        public int mutualFriends;
        public string playstyle;
        public bool isOnline;
    }
}
