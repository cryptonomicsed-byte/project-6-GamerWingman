using UnityEngine;
using System;
using GamerWingman.Firebase;
using GamerWingman.Animations;

namespace GamerWingman.Core
{
    /// <summary>
    /// Tracks user activity and avatar mood.
    /// After 3 days of inactivity, triggers sad_slump animation and "Missed playing with you" message.
    /// Updates mood based on daily play, win/loss events, and streak tracking.
    /// </summary>
    public class MoodManager : MonoBehaviour
    {
        [SerializeField] private AnimationManager animationManager;
        [SerializeField] private VoiceManager voiceManager;
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private float inactivityThresholdHours = 72f; // 3 days

        private DateTime lastActivityTime;
        private string currentMood = "happy";
        private bool isTracking = false;

        private const string LAST_ACTIVITY_KEY = "gw_last_activity";
        private const string MOOD_STATE_KEY = "gw_mood_state";

        /// <summary>
        /// Start tracking user activity and mood
        /// </summary>
        public void StartTracking()
        {
            Debug.Log("[MoodManager] Starting activity tracking...");

            // Load last activity time from PlayerPrefs
            if (PlayerPrefs.HasKey(LAST_ACTIVITY_KEY))
            {
                long ticks = long.Parse(PlayerPrefs.GetString(LAST_ACTIVITY_KEY));
                lastActivityTime = new DateTime(ticks);
            }
            else
            {
                lastActivityTime = DateTime.Now;
            }

            currentMood = PlayerPrefs.GetString(MOOD_STATE_KEY, "happy");
            isTracking = true;

            InvokeRepeating(nameof(CheckInactivity), 60f, 60f); // Check every minute
            InvokeRepeating(nameof(UpdateMoodState), 10f, 300f); // Update every 5 minutes
        }

        /// <summary>
        /// Record user activity (keeps avatar happy)
        /// </summary>
        public void RecordActivity()
        {
            lastActivityTime = DateTime.Now;
            PlayerPrefs.SetString(LAST_ACTIVITY_KEY, lastActivityTime.Ticks.ToString());
            PlayerPrefs.Save();

            // Reset to happy mood when user plays
            if (currentMood != "happy")
            {
                SetMood("happy");
            }

            Debug.Log("[MoodManager] Activity recorded");
        }

        /// <summary>
        /// Check if user has been inactive for 3+ days
        /// </summary>
        private void CheckInactivity()
        {
            if (!isTracking) return;

            TimeSpan inactivityDuration = DateTime.Now - lastActivityTime;
            float inactivityHours = (float)inactivityDuration.TotalHours;

            if (inactivityHours >= inactivityThresholdHours && currentMood != "sad")
            {
                Debug.Log("[MoodManager] User inactive for " + inactivityHours + " hours - triggering sad mood");
                SetMood("sad");
                animationManager.Play("SadSlump");
                voiceManager.Speak("I've missed playing with you!");
            }
        }

        /// <summary>
        /// Set mood and sync to Firebase
        /// </summary>
        private void SetMood(string mood)
        {
            currentMood = mood;
            PlayerPrefs.SetString(MOOD_STATE_KEY, mood);
            PlayerPrefs.Save();

            // Sync to Firebase asynchronously
            _ = firebaseManager.UpdateUserMood(mood);

            Debug.Log("[MoodManager] Mood changed to: " + mood);
        }

        /// <summary>
        /// Update mood based on various factors
        /// Called periodically to adjust mood state
        /// </summary>
        private void UpdateMoodState()
        {
            if (!isTracking) return;

            // This would be called by Cloud Functions in production
            // For now, just log the update
            Debug.Log("[MoodManager] Mood state updated: " + currentMood);
        }

        /// <summary>
        /// Record achievement/win and trigger celebration
        /// </summary>
        public void RecordWin(string gameName)
        {
            RecordActivity();
            animationManager.Play("DanceVictory");
            voiceManager.Speak("That's what I'm talking about! Great win!");

            Debug.Log("[MoodManager] Win recorded for: " + gameName);
        }

        /// <summary>
        /// Record loss and trigger comfort animation
        /// </summary>
        public void RecordLoss(string gameName)
        {
            RecordActivity();
            animationManager.Play("ThumbsUp");
            voiceManager.Speak("You'll get them next time. Keep your head up!");

            Debug.Log("[MoodManager] Loss recorded for: " + gameName);
        }

        public string GetCurrentMood() => currentMood;

        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
