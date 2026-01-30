using UnityEngine;
using System.Collections.Generic;

namespace GamerWingman.Animations
{
    /// <summary>
    /// Controls avatar animations via Animator Controller blendshapes.
    /// Manages 20 state animations loaded as Addressables.
    /// Drives facial expressions and lip-sync through blendshape parameters.
    /// </summary>
    public class AnimationManager : MonoBehaviour
    {
        private Animator animator;
        private SkinnedMeshRenderer skinnedMeshRenderer;

        // Animation state parameters
        private readonly List<string> AnimationStates = new()
        {
            "Idle", "Wave", "Victory", "ThumbsUp", "ThumbsDown",
            "Thinking", "HeadsetAdjust", "ControllerHold", "RageQuit",
            "Flex", "PointForward", "Shrug", "DanceVictory", "SadSlump",
            "ExcitedJump", "SearchPose", "SquadUp", "ControllerThrow",
            "Laugh", "NodYes", "ShakeNo"
        };

        // Blendshape indices for facial expressions
        private Dictionary<string, int> blendshapeIndices = new();

        private void Start()
        {
            InitializeAnimator();
        }

        /// <summary>
        /// Initialize animator and cache blendshape indices
        /// </summary>
        private void InitializeAnimator()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("[AnimationManager] Animator component not found on avatar!");
                return;
            }

            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                CacheBlendshapeIndices();
            }

            Debug.Log("[AnimationManager] Animator initialized with " + AnimationStates.Count + " animation states");
        }

        /// <summary>
        /// Cache blendshape indices for fast access during runtime
        /// </summary>
        private void CacheBlendshapeIndices()
        {
            Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
            if (sharedMesh == null) return;

            string[] blendshapeNames = { "Smile", "Frown", "Surprised", "Angry", "Neutral", "Open_Mouth" };

            foreach (string blendName in blendshapeNames)
            {
                int index = sharedMesh.GetBlendShapeIndex(blendName);
                if (index >= 0)
                {
                    blendshapeIndices[blendName] = index;
                }
            }
        }

        /// <summary>
        /// Play animation by state name
        /// Public API for other systems to trigger animations
        /// </summary>
        public void Play(string stateName)
        {
            if (animator == null)
            {
                Debug.LogError("[AnimationManager] Animator not initialized!");
                return;
            }

            if (!AnimationStates.Contains(stateName))
            {
                Debug.LogWarning("[AnimationManager] Unknown animation state: " + stateName);
                return;
            }

            animator.SetTrigger(stateName);
            Debug.Log("[AnimationManager] Playing animation: " + stateName);
        }

        /// <summary>
        /// Set blendshape weight for facial expression (0-100)
        /// </summary>
        public void SetBlendshape(string blendName, float weight)
        {
            if (skinnedMeshRenderer == null || !blendshapeIndices.ContainsKey(blendName))
                return;

            int index = blendshapeIndices[blendName];
            skinnedMeshRenderer.SetBlendShapeWeight(index, Mathf.Clamp01(weight) * 100f);
        }

        /// <summary>
        /// Drive lip-sync from audio analysis (called by VoiceManager)
        /// </summary>
        public void UpdateLipSync(float mouthOpenness)
        {
            if (skinnedMeshRenderer == null) return;

            // Animate mouth blendshape based on audio frequency
            SetBlendshape("Open_Mouth", mouthOpenness);
        }

        /// <summary>
        /// Get list of available animation states
        /// </summary>
        public List<string> GetAvailableAnimations() => new(AnimationStates);
    }
}
