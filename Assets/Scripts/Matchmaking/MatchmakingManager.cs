using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamerWingman.AI;
using GamerWingman.Animations;
using GamerWingman.Services;

namespace GamerWingman.Matchmaking
{
    /// <summary>
    /// Orchestrates squad finding and player matching.
    /// Uses AI Agent to analyze compatibility, supports voice command "Find squad".
    /// Triggers squad_up animation and celebration when match is found.
    /// </summary>
    public class MatchmakingManager : MonoBehaviour
    {
        [SerializeField] private AIAgent aiAgent;
        [SerializeField] private AnimationManager animationManager;
        [SerializeField] private VoiceManager voiceManager;
        [SerializeField] private PlatformIntegration platformIntegration;

        private bool isSearching = false;

        /// <summary>
        /// User says "Find squad" - trigger matching process
        /// </summary>
        public async void StartSquadSearch()
        {
            if (isSearching)
            {
                Debug.Log("[MatchmakingManager] Already searching...");
                return;
            }

            isSearching = true;
            animationManager.Play("SearchPose");
            voiceManager.Speak("Alright, let me find some compatible players for you!");

            Debug.Log("[MatchmakingManager] Starting squad search...");

            // Step 1: Analyze current gaming session
            var gameAnalysis = await AnalyzeCurrentGame();

            // Step 2: Aggregate available players from platforms
            var candidatePlayers = await GetCandidatePlayers();

            // Step 3: Use AI to find compatible matches
            var recommendedSquad = await aiAgent.FindSquadMates(candidatePlayers);

            // Step 4: Present results to user
            if (recommendedSquad.Count > 0)
            {
                OnSquadFound(recommendedSquad);
            }
            else
            {
                OnNoSquadFound();
            }

            isSearching = false;
        }

        /// <summary>
        /// Analyze current game from platform APIs
        /// </summary>
        private async Task<Dictionary<string, object>> AnalyzeCurrentGame()
        {
            Debug.Log("[MatchmakingManager] Analyzing current game...");

            var stats = await platformIntegration.AggregateUserStats();
            
            // TODO: Determine which game is currently running
            // Parse stats to understand playstyle in that game

            return stats;
        }

        /// <summary>
        /// Get list of potential squad mates
        /// Can come from friends list, public lobbies, or platform APIs
        /// </summary>
        private async Task<List<string>> GetCandidatePlayers()
        {
            Debug.Log("[MatchmakingManager] Fetching candidate players...");

            var candidates = new List<string>();

            // TODO: Query Firestore for users in same region/game
            // TODO: Query platform APIs for friends list
            // TODO: Add public lobby players if available

            await Task.Delay(1000); // Placeholder
            return candidates;
        }

        /// <summary>
        /// Squad found - show results and enable invite
        /// </summary>
        private void OnSquadFound(List<string> squad)
        {
            Debug.Log("[MatchmakingManager] Squad found! " + squad.Count + " recommendations");

            animationManager.Play("SquadUp");
            voiceManager.Speak("Found " + squad.Count + " awesome players! Ready to squad up?");

            // TODO: Display squad UI with player cards
            // Show invite button that opens native party invite
        }

        /// <summary>
        /// No compatible players found
        /// </summary>
        private void OnNoSquadFound()
        {
            Debug.Log("[MatchmakingManager] No compatible players found");

            animationManager.Play("Shrug");
            voiceManager.Speak("Couldn't find any players right now, but keep playing and more will join!");
        }

        /// <summary>
        /// One-tap invite to compatible player
        /// Opens native platform party/squad invite if available
        /// </summary>
        public void InvitePlayer(string playerId)
        {
            Debug.Log("[MatchmakingManager] Inviting player: " + playerId);

            // TODO: Open native invite dialog for the platform
            // Could be Xbox Live party invite, PSN group invite, Discord server, etc.
            // Use platform-specific APIs or deep links

            voiceManager.Speak("Sending invite!");
        }

        public bool IsSearching => isSearching;
    }
}
