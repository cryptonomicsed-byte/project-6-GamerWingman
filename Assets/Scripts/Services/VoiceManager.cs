using UnityEngine;
using System.Collections;

namespace GamerWingman.Services
{
    /// <summary>
    /// Manages voice input (wake word detection) and voice output (TTS with lip-sync).
    /// Uses Porcupine SDK for offline wake word detection and Android TTS or ElevenLabs for synthesis.
    /// </summary>
    public class VoiceManager : MonoBehaviour
    {
        [SerializeField] private AnimationManager animationManager;
        [SerializeField] private bool useElevenLabs = false;
        [SerializeField] private string wakeWord = "Hey Wingman";

        private AudioSource audioSource;
        private bool isListeningForWakeWord = false;
        private bool isProcessingCommand = false;

        /// <summary>
        /// Initialize voice systems (wake word detection + TTS)
        /// </summary>
        public void Initialize()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            Debug.Log("[VoiceManager] Initializing voice systems...");
            StartCoroutine(InitializeWakeWordDetection());
        }

        /// <summary>
        /// Initialize Porcupine wake word detection
        /// Requires com.picovoice.porcupine plugin and access key
        /// </summary>
        private IEnumerator InitializeWakeWordDetection()
        {
            Debug.Log("[VoiceManager] Starting wake word detection for: " + wakeWord);
            
            // TODO: Initialize Porcupine SDK with wake word model
            // Should listen continuously in background and invoke OnWakeWordDetected()
            
            isListeningForWakeWord = true;
            yield return new WaitForSeconds(1f);
        }

        /// <summary>
        /// Callback when wake word is detected
        /// </summary>
        private void OnWakeWordDetected()
        {
            if (isProcessingCommand) return;

            Debug.Log("[VoiceManager] Wake word detected!");
            StartCoroutine(CaptureVoiceCommand());
        }

        /// <summary>
        /// Capture voice command from user
        /// Uses Android SpeechRecognizer or Porcupine continuous recognition
        /// </summary>
        private IEnumerator CaptureVoiceCommand()
        {
            isProcessingCommand = true;
            animationManager.Play("Thinking");

            Debug.Log("[VoiceManager] Listening for voice command...");
            
            // TODO: Start speech recognition, wait for result
            // String transcript = await speechRecognizer.Listen(timeoutMs: 5000);
            
            yield return new WaitForSeconds(5f); // Placeholder
            isProcessingCommand = false;
        }

        /// <summary>
        /// Speak text response with synchronized lip-sync
        /// </summary>
        public void Speak(string text)
        {
            Debug.Log("[VoiceManager] Speaking: " + text);
            StartCoroutine(SpeakAsync(text));
        }

        /// <summary>
        /// Generate speech using TTS and play with lip-sync animation
        /// </summary>
        private IEnumerator SpeakAsync(string text)
        {
            AudioClip clip;

            if (useElevenLabs)
            {
                // TODO: Call ElevenLabs API for high-quality TTS
                clip = null; // await ElevenLabsTTS(text);
            }
            else
            {
                // Use Android TextToSpeech
                clip = GenerateTTSAudio(text);
            }

            if (clip != null)
            {
                yield return StartCoroutine(PlayWithLipSync(clip));
            }
        }

        /// <summary>
        /// Generate audio from text using Android TTS
        /// </summary>
        private AudioClip GenerateTTSAudio(string text)
        {
            Debug.Log("[VoiceManager] Generating TTS audio for: " + text);
            
            // TODO: Use Android.Speech.TextToSpeech or similar
            // This is a placeholder; real implementation needs TTS bridge
            
            return null;
        }

        /// <summary>
        /// Play audio while driving lip-sync animation
        /// </summary>
        private IEnumerator PlayWithLipSync(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);

            float duration = clip.length;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Get frequency from audio spectrum to drive mouth blendshape
                float[] spectrum = new float[256];
                audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);
                float mouthOpenness = spectrum.Length > 0 ? spectrum[0] : 0f;

                animationManager.UpdateLipSync(mouthOpenness);
                yield return null;
            }

            // Reset lip-sync
            animationManager.UpdateLipSync(0f);
        }

        /// <summary>
        /// Stop listening for voice input
        /// </summary>
        public void StopListening()
        {
            isListeningForWakeWord = false;
            Debug.Log("[VoiceManager] Stopped listening");
        }

        /// <summary>
        /// Resume listening for voice input
        /// </summary>
        public void ResumeListening()
        {
            isListeningForWakeWord = true;
            Debug.Log("[VoiceManager] Resumed listening");
        }
    }
}
