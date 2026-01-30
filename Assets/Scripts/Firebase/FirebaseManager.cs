using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerWingman.Firebase
{
    /// <summary>
    /// Manages Firebase initialization, anonymous authentication, and Firestore database operations.
    /// Handles user profile sync, playstyle tracking, and mood state persistence.
    /// </summary>
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseApp app;
        public static FirebaseAuth auth;
        public static FirebaseFirestore db;

        private bool isInitialized = false;
        private string userId;

        public async Task InitializeAsync()
        {
            Debug.Log("[FirebaseManager] Initializing Firebase...");

            // Check and resolve Firebase dependencies
            var dependencyStatus = FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus.Result != DependencyStatus.Available)
            {
                Debug.LogError("[FirebaseManager] Firebase dependencies not available: " + dependencyStatus.Result);
                return;
            }

            app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;

            // Authenticate anonymously
            try
            {
                var result = await auth.SignInAnonymouslyAsync();
                userId = result.User.UserId;
                Debug.Log("[FirebaseManager] Anonymous auth successful. User ID: " + userId);
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("[FirebaseManager] Anonymous auth failed: " + ex.Message);
                return;
            }

            isInitialized = true;
        }

        /// <summary>
        /// Load user profile from Firestore
        /// </summary>
        public async Task LoadUserProfile()
        {
            if (!isInitialized)
            {
                Debug.LogError("[FirebaseManager] Firebase not initialized");
                return;
            }

            try
            {
                var docRef = db.Collection("users").Document(userId);
                var docSnapshot = await docRef.GetSnapshotAsync();

                if (docSnapshot.Exists)
                {
                    // Parse existing profile
                    var data = docSnapshot.ToDictionary();
                    Debug.Log("[FirebaseManager] User profile loaded: " + data.Count + " fields");
                }
                else
                {
                    // Create new profile
                    await CreateNewUserProfile();
                }
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("[FirebaseManager] Failed to load profile: " + ex.Message);
            }
        }

        /// <summary>
        /// Create new user profile in Firestore
        /// </summary>
        private async Task CreateNewUserProfile()
        {
            try
            {
                var userProfile = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "createdAt", FieldValue.ServerTimestamp },
                    { "lastPlayTime", FieldValue.ServerTimestamp },
                    { "totalPlaytime", 0L },
                    { "streak", 0 },
                    { "mood", "happy" },
                    { "playstyleSummary", "New player" },
                    { "premiumTier", false },
                    { "friends", new List<string>() }
                };

                await db.Collection("users").Document(userId).SetAsync(userProfile);
                Debug.Log("[FirebaseManager] New user profile created");
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("[FirebaseManager] Failed to create profile: " + ex.Message);
            }
        }

        /// <summary>
        /// Update user mood status in Firestore
        /// </summary>
        public async Task UpdateUserMood(string mood)
        {
            if (!isInitialized) return;

            try
            {
                await db.Collection("users").Document(userId).UpdateAsync(
                    "mood", mood,
                    "lastMoodUpdate", FieldValue.ServerTimestamp
                );
                Debug.Log("[FirebaseManager] User mood updated: " + mood);
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("[FirebaseManager] Failed to update mood: " + ex.Message);
            }
        }

        /// <summary>
        /// Record gaming session (playtime + achievement detection)
        /// </summary>
        public async Task RecordGameSession(string gameName, long playtimeSeconds, bool won)
        {
            if (!isInitialized) return;

            try
            {
                var sessionData = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "gameName", gameName },
                    { "playtime", playtimeSeconds },
                    { "won", won },
                    { "timestamp", FieldValue.ServerTimestamp }
                };

                await db.Collection("users").Document(userId).Collection("sessions").AddAsync(sessionData);
                Debug.Log("[FirebaseManager] Game session recorded: " + gameName);
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("[FirebaseManager] Failed to record session: " + ex.Message);
            }
        }

        /// <summary>
        /// Update premium tier status after purchase
        /// </summary>
        public async Task UpdatePremiumTier(bool isPremium)
        {
            if (!isInitialized) return;

            try
            {
                await db.Collection("users").Document(userId).UpdateAsync("premiumTier", isPremium);
                Debug.Log("[FirebaseManager] Premium tier updated: " + isPremium);
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("[FirebaseManager] Failed to update premium: " + ex.Message);
            }
        }

        public string GetUserId() => userId;
        public bool IsInitialized => isInitialized;
    }
}
