using UnityEngine;
using UnityEngine.UI;

namespace GamerWingman.UI
{
    /// <summary>
    /// Minimal UI manager - only floating mic button and occasional text overlays.
    /// Immersive full-screen experience with black void background.
    /// Shows achievement popups, voice command feedback, squad invites.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Button micButton;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Text feedbackText;
        [SerializeField] private CanvasGroup fadeGroup;

        private void Start()
        {
            SetupImmersiveMode();
            SetupMicButton();
        }

        /// <summary>
        /// Enable immersive full-screen mode (no status bar, no nav bar)
        /// </summary>
        private void SetupImmersiveMode()
        {
            Debug.Log("[UIManager] Setting up immersive mode...");

            // Hide Android system UI
            int flags = (int)(
                AndroidJavaClass.GetStatic<int>("android.view.View", "SYSTEM_UI_FLAG_LAYOUT_STABLE") |
                AndroidJavaClass.GetStatic<int>("android.view.View", "SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION") |
                AndroidJavaClass.GetStatic<int>("android.view.View", "SYSTEM_UI_FLAG_HIDE_NAVIGATION") |
                AndroidJavaClass.GetStatic<int>("android.view.View", "SYSTEM_UI_FLAG_IMMERSIVE_STICKY")
            );

            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var window = activity.Call<AndroidJavaObject>("getWindow");
            window.Call("getDecorView").Call("setSystemUiVisibility", flags);

            Screen.fullScreen = true;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        /// <summary>
        /// Setup floating mic button (single tap/hold interface)
        /// </summary>
        private void SetupMicButton()
        {
            if (micButton != null)
            {
                micButton.onClick.AddListener(OnMicButtonPressed);
                Debug.Log("[UIManager] Mic button initialized");
            }
        }

        /// <summary>
        /// User tapped/held mic button
        /// </summary>
        private void OnMicButtonPressed()
        {
            Debug.Log("[UIManager] Mic button pressed - wake word listening active");

            // Voice manager should be listening for wake word "Hey Wingman"
            // This button visually confirms that listening is active
        }

        /// <summary>
        /// Show feedback text overlay (command recognized, etc.)
        /// </summary>
        public void ShowFeedback(string message, float duration = 2f)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                StartCoroutine(FadeOutAfter(duration));
                Debug.Log("[UIManager] Showing feedback: " + message);
            }
        }

        /// <summary>
        /// Show achievement popup
        /// </summary>
        public void ShowAchievementPopup(string title, string description)
        {
            Debug.Log("[UIManager] Achievement: " + title + " - " + description);

            // TODO: Instantiate achievement popup prefab
            // Animate in from top, auto-dismiss after 3 seconds
        }

        /// <summary>
        /// Show squad invitation UI
        /// </summary>
        public void ShowSquadInvite(List<string> playerNames)
        {
            Debug.Log("[UIManager] Showing squad invite UI for " + playerNames.Count + " players");

            // TODO: Instantiate squad UI panel
            // Display player cards with accept/decline buttons
        }

        /// <summary>
        /// Fade out UI element after delay
        /// </summary>
        private System.Collections.IEnumerator FadeOutAfter(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (fadeGroup != null)
            {
                float fadeDuration = 0.5f;
                float elapsed = 0f;

                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    fadeGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    yield return null;
                }

                fadeGroup.alpha = 0f;
            }
        }

        public void HideUI()
        {
            mainCanvas.enabled = false;
        }

        public void ShowUI()
        {
            mainCanvas.enabled = true;
        }
    }
}
