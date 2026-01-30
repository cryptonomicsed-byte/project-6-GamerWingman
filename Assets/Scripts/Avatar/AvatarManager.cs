using UnityEngine;
using System.Collections;
using System.IO;

namespace GamerWingman.Avatar
{
    /// <summary>
    /// Manages avatar lifecycle: Ready Player Me SDK integration, caching, and runtime loading.
    /// Handles avatar creation via WebView and persistent caching in Application.persistentDataPath.
    /// </summary>
    public class AvatarManager : MonoBehaviour
    {
        [SerializeField] private Transform avatarSpawnPoint;
        [SerializeField] private float avatarScale = 1f;

        private GameObject currentAvatarInstance;
        private string cachedAvatarPath;
        private const string AVATAR_CACHE_KEY = "rpm_avatar_url";

        private void OnEnable()
        {
            // Subscribe to Ready Player Me avatar selection
            // Note: This would integrate with RPM SDK callbacks
        }

        /// <summary>
        /// Initialize avatar system: check cache, open RPM if needed, load avatar
        /// </summary>
        public IEnumerator InitializeAsync()
        {
            Debug.Log("[AvatarManager] Initializing avatar system...");

            // Check if avatar is cached
            if (HasCachedAvatar())
            {
                Debug.Log("[AvatarManager] Loading cached avatar...");
                yield return StartCoroutine(LoadCachedAvatar());
            }
            else
            {
                Debug.Log("[AvatarManager] No cached avatar, opening Ready Player Me...");
                yield return StartCoroutine(OpenReadyPlayerMe());
            }
        }

        /// <summary>
        /// Check if avatar GLB file exists in cache
        /// </summary>
        private bool HasCachedAvatar()
        {
            cachedAvatarPath = Path.Combine(Application.persistentDataPath, "avatar.glb");
            return File.Exists(cachedAvatarPath);
        }

        /// <summary>
        /// Open Ready Player Me WebView for avatar creation
        /// Implementation requires RPM SDK (com.readyplayerme.sdk)
        /// </summary>
        private IEnumerator OpenReadyPlayerMe()
        {
            Debug.Log("[AvatarManager] Opening Ready Player Me WebView...");
            
            // TODO: Integrate Ready Player Me SDK
            // Should open WebView pointing to: https://readyplayer.me/avatar?quickstart=true
            // Listen for avatar selection callback and download GLB file

            yield return new WaitForSeconds(1f); // Placeholder
        }

        /// <summary>
        /// Load avatar from cache and instantiate in scene
        /// </summary>
        private IEnumerator LoadCachedAvatar()
        {
            if (!File.Exists(cachedAvatarPath))
            {
                Debug.LogWarning("[AvatarManager] Cached avatar not found at: " + cachedAvatarPath);
                yield break;
            }

            // TODO: Load GLB file using GLTFast or equivalent
            // Instantiate at avatarSpawnPoint, apply Humanoid rig
            Debug.Log("[AvatarManager] Avatar loaded from cache: " + cachedAvatarPath);
            
            yield return new WaitForSeconds(0.5f); // Placeholder for actual loading
        }

        /// <summary>
        /// Cache downloaded avatar GLB file
        /// </summary>
        public void CacheAvatarFile(byte[] glbData)
        {
            string savePath = Path.Combine(Application.persistentDataPath, "avatar.glb");
            File.WriteAllBytes(savePath, glbData);
            Debug.Log("[AvatarManager] Avatar cached at: " + savePath);
        }

        /// <summary>
        /// Get current avatar instance
        /// </summary>
        public GameObject GetAvatarInstance() => currentAvatarInstance;

        /// <summary>
        /// Scale avatar for display
        /// </summary>
        public void SetAvatarScale(float scale)
        {
            avatarScale = scale;
            if (currentAvatarInstance != null)
            {
                currentAvatarInstance.transform.localScale = Vector3.one * scale;
            }
        }
    }
}
