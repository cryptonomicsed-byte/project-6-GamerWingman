using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using System.Threading.Tasks;

namespace GamerWingman.Core
{
    /// <summary>
    /// Main game manager that orchestrates initialization, Firebase auth, and avatar setup.
    /// Handles singleton pattern and bootstraps all core services.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private AvatarManager avatarManager;
        [SerializeField] private VoiceManager voiceManager;
        [SerializeField] private AnimationManager animationManager;
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private MoodManager moodManager;

        private bool isInitialized = false;

        private void Awake()
        {
            // Implement singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Screen.fullScreen = true;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        private void Start()
        {
            StartCoroutine(InitializeGame());
        }

        /// <summary>
        /// Sequential initialization of all systems: Firebase auth → Avatar → Voice → Mood tracking
        /// </summary>
        private IEnumerator InitializeGame()
        {
            Debug.Log("[GameManager] Starting initialization...");

            // Step 1: Initialize Firebase with anonymous auth
            yield return StartCoroutine(firebaseManager.InitializeAsync());

            // Step 2: Load or create user profile
            yield return StartCoroutine(LoadUserProfile());

            // Step 3: Initialize avatar (Ready Player Me or cached)
            yield return StartCoroutine(avatarManager.InitializeAsync());

            // Step 4: Initialize voice system (wake word listening + TTS)
            voiceManager.Initialize();

            // Step 5: Start mood tracking and inactivity detection
            moodManager.StartTracking();

            isInitialized = true;
            Debug.Log("[GameManager] Initialization complete!");
        }

        /// <summary>
        /// Load user profile from Firestore or create new one
        /// </summary>
        private IEnumerator LoadUserProfile()
        {
            var task = firebaseManager.LoadUserProfile();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError("[GameManager] Failed to load user profile: " + task.Exception);
            }
            else
            {
                Debug.Log("[GameManager] User profile loaded successfully");
            }
        }

        public bool IsInitialized => isInitialized;
    }
}
