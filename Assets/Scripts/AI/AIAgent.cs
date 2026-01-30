using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerWingman.AI
{
    /// <summary>
    /// Local AI agent powered by Ollama (llama3.2:3b or phi-3:mini).
    /// Analyzes user gaming profile, suggests games, finds squad mates.
    /// Runs locally on device for privacy and offline capability.
    /// </summary>
    public class AIAgent : MonoBehaviour
    {
        [SerializeField] private string ollamaEndpoint = "http://localhost:11434/api/generate";
        [SerializeField] private string modelName = "llama3.2:3b";
        [SerializeField] private VoiceManager voiceManager;

        private Dictionary<string, object> userProfile = new();
        private bool isModelLoaded = false;

        public async void Initialize()
        {
            Debug.Log("[AIAgent] Initializing AI agent with model: " + modelName);

            // Check if Ollama is available locally
            bool ollamaAvailable = await CheckOllamaConnection();
            if (!ollamaAvailable)
            {
                Debug.LogWarning("[AIAgent] Ollama not available. Running in offline mode.");
                return;
            }

            // Load model if not already loaded
            isModelLoaded = await LoadModel();
        }

        /// <summary>
        /// Check if Ollama bridge is accessible on device
        /// </summary>
        private async Task<bool> CheckOllamaConnection()
        {
            // TODO: Try connecting to Ollama local bridge
            // This assumes Ollama is running via Android bridge (Termux or similar)

            Debug.Log("[AIAgent] Checking Ollama connection...");
            await Task.Delay(500); // Placeholder
            return false; // Placeholder
        }

        /// <summary>
        /// Load LLM model locally
        /// </summary>
        private async Task<bool> LoadModel()
        {
            Debug.Log("[AIAgent] Loading model: " + modelName);

            // TODO: Pull and load model via Ollama API
            // POST /api/pull with model name
            // This happens once, then model stays in memory

            await Task.Delay(1000); // Placeholder
            return true;
        }

        /// <summary>
        /// Generate playstyle summary from user gaming profile
        /// </summary>
        public async Task<string> GeneratePlaystyleSummary(Dictionary<string, object> profile)
        {
            userProfile = profile;

            string prompt = BuildPlaystylePrompt(profile);
            string summary = await QueryAI(prompt);

            Debug.Log("[AIAgent] Generated playstyle summary: " + summary);
            return summary;
        }

        /// <summary>
        /// Suggest games based on user profile and recent activity
        /// </summary>
        public async Task<List<string>> SuggestGames()
        {
            string prompt = BuildGameSuggestionPrompt();
            string response = await QueryAI(prompt);

            // Parse response into game list
            var games = ParseGameList(response);
            Debug.Log("[AIAgent] Game suggestions: " + string.Join(", ", games));

            return games;
        }

        /// <summary>
        /// Find compatible squad mates from available pool
        /// Analyzes playstyle, communication style, skill level
        /// </summary>
        public async Task<List<string>> FindSquadMates(List<string> candidateIds)
        {
            string prompt = BuildSquadmatePrompt(candidateIds);
            string response = await QueryAI(prompt);

            var compatible = ParseSquadmateList(response);
            Debug.Log("[AIAgent] Compatible squadmates: " + string.Join(", ", compatible));

            return compatible;
        }

        /// <summary>
        /// Query local Ollama model with streaming response
        /// </summary>
        private async Task<string> QueryAI(string prompt)
        {
            if (!isModelLoaded)
            {
                Debug.LogError("[AIAgent] Model not loaded");
                return "I'm offline right now. Try again later!";
            }

            Debug.Log("[AIAgent] Querying: " + prompt.Substring(0, Mathf.Min(50, prompt.Length)) + "...");

            // TODO: Send POST request to Ollama with prompt
            // Parse streaming JSON responses
            // Aggregate into full response

            await Task.Delay(1000); // Placeholder for API call
            return "Placeholder response from AI";
        }

        private string BuildPlaystylePrompt(Dictionary<string, object> profile)
        {
            return $@"
Based on this gaming profile:
- Recent games: {profile.ContainsKey("recentGames") ? profile["recentGames"] : "Unknown"}
- Total playtime: {profile.ContainsKey("totalPlaytime") ? profile["totalPlaytime"] : "0"} hours
- Win rate: {profile.ContainsKey("winRate") ? profile["winRate"] : "Unknown"}

Generate a 1-2 sentence playstyle summary that describes their gaming type and preferences.
";
        }

        private string BuildGameSuggestionPrompt()
        {
            return $@"
Based on a player who plays: {string.Join(", ", userProfile.Keys)}

Suggest 3-5 games that would appeal to this player.
List only game names, one per line.
";
        }

        private string BuildSquadmatePrompt(List<string> candidateIds)
        {
            return $@"
Match this player with compatible squadmates from:
{string.Join("\n", candidateIds)}

Return the IDs of the 3 most compatible players, one per line.
";
        }

        private List<string> ParseGameList(string response)
        {
            var games = new List<string>();
            foreach (var line in response.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    games.Add(line.Trim());
            }
            return games;
        }

        private List<string> ParseSquadmateList(string response)
        {
            var squadmates = new List<string>();
            foreach (var line in response.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    squadmates.Add(line.Trim());
            }
            return squadmates;
        }
    }
}
