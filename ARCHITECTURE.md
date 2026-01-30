# GamerWingman Architecture

## High-Level System Design

```
┌─────────────────────────────────────────────────────┐
│              GAMERWINGMAN MOBILE APP                │
│          (Unity 6 LTS, Android 12+, HDRP)          │
└────────────────────┬────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
┌───────▼──┐  ┌──────▼───┐  ┌────▼─────┐
│   Avatar  │  │  Mood    │  │  Voice   │
│  System   │  │ Tracking │  │ Commands │
│ (RPM)     │  │          │  │(Porcupine)
└───────┬──┘  └──────┬───┘  └────┬─────┘
        │            │            │
        └────────────┼────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
┌───────▼──┐  ┌──────▼───┐  ┌────▼──────┐
│ Animation│  │   Game   │  │ Matchmak- │
│ Manager  │  │  Analyzer│  │   ing     │
│(20+anim) │  │  (Stats) │  │  (Squad)  │
└───────┬──┘  └──────┬───┘  └────┬──────┘
        │            │            │
        └────────────┼────────────┘
                     │
        ┌────────────┼────────────┬────────────┐
        │            │            │            │
┌───────▼──┐  ┌──────▼───┐  ┌────▼────┐  ┌───▼─────┐
│ Firebase │  │  SQLite  │  │Platform  │  │ In-App  │
│(Cloud)   │  │ (Local)  │  │ APIs     │  │Purchase │
│Auth/DB   │  │UserProfile  │(OAuth)   │  │(Billing)│
└──────────┘  └──────────┘  └──────────┘  └─────────┘
```

## Core Systems

### 1. Avatar System (Ready Player Me Integration)
**File**: `Assets/Scripts/Avatar/AvatarManager.cs`

```
AvatarManager
├── InitializeAsync()
│   ├── Check cache → HasCachedAvatar()
│   ├── If not cached → OpenReadyPlayerMe()
│   │   ├── WebView to readyplayer.me/avatar
│   │   ├── Listen for selection callback
│   │   └── Download GLB file
│   └── Cache & Load → LoadCachedAvatar()
│       └── GLTFast load → Humanoid rig
├── SetAvatarScale()
├── UpdateBlendshapes() → Facial expressions
└── GetAvatarInstance()
```

**Data Flow**:
- First launch → WebView opens
- User creates avatar
- GLB downloaded → Cached to `Application.persistentDataPath/avatar.glb`
- Loaded via GLTFast with Humanoid rig
- Blendshapes enabled for facial expressions

### 2. Animation System
**File**: `Assets/Scripts/Animations/AnimationManager.cs`

20 Pre-loaded Mixamo Animations:
```
Idle, Wave, Victory, ThumbsUp, ThumbsDown,
Thinking, HeadsetAdjust, ControllerHold, RageQuit,
Flex, PointForward, Shrug, DanceVictory, SadSlump,
ExcitedJump, SearchPose, SquadUp, ControllerThrow,
Laugh, NodYes, ShakeNo
```

**Architecture**:
```
AnimationManager
├── InitializeAnimator()
│   ├── Get Animator component
│   └── Cache blendshape indices
├── Play(stateName) → Public API
│   └── animator.SetTrigger(stateName)
├── SetBlendshape() → Facial expressions
│   └── skinnedMeshRenderer.SetBlendShapeWeight()
├── UpdateLipSync() → Audio-driven mouth
│   └── Analyze audio spectrum → mouth openness
└── GetAvailableAnimations()
```

**Animator Controller States**:
- Each state corresponds to one animation
- Blendshapes for smile, frown, surprised, angry, mouth_open
- Parameters: triggers for each animation state

### 3. Voice System
**File**: `Assets/Scripts/Services/VoiceManager.cs`

```
VoiceManager
├── Initialize()
│   └── InitializeWakeWordDetection()
│       └── Porcupine.init(wakeWord: "Hey Wingman")
├── OnWakeWordDetected()
│   └── CaptureVoiceCommand()
│       ├── Start SpeechRecognizer
│       ├── Play Thinking animation
│       └── Wait for speech result
├── Speak(text)
│   └── SpeakAsync(text)
│       ├── TTS generation (Android TTS or ElevenLabs)
│       └── PlayWithLipSync(clip)
│           ├── Play audio
│           ├── Analyze FFT spectrum
│           └── Drive mouth blendshape
└── StopListening() / ResumeListening()
```

**Wake Word Flow**:
1. Porcupine continuously listens (low power)
2. Detects "Hey Wingman" → OnWakeWordDetected()
3. Start speech recognition for command
4. Parse command intent (FindSquad, SuggestGames, etc.)
5. Execute action + speak response

**TTS with Lip-Sync**:
1. Convert text to speech (local or ElevenLabs)
2. Play audio clip through AudioSource
3. Sample FFT spectrum every frame
4. Map frequency intensity → mouth openness blendshape
5. Reset on completion

### 4. Firebase System
**File**: `Assets/Scripts/Firebase/FirebaseManager.cs`

```
FirebaseManager
├── InitializeAsync()
│   ├── Check Firebase dependencies
│   └── SignInAnonymouslyAsync()
│       └── Create user in Firestore
├── LoadUserProfile()
│   ├── Check if user exists
│   ├── If exists → Parse data
│   └── If new → CreateNewUserProfile()
├── UpdateUserMood(mood)
│   └── Firestore.Update("mood", mood)
├── RecordGameSession(gameName, playtimeSeconds, won)
│   └── Add to users/{userId}/sessions/
├── UpdatePremiumTier(isPremium)
│   └── Set premium flag
└── GetUserId()
```

**Firestore Structure**:
```
users/{userId}/
  - userId, createdAt, lastPlayTime
  - totalPlaytimeSeconds, currentStreak
  - mood (happy/thinking/sad/excited)
  - playstyleSummary, isPremium
  - playtimePerGame {}
  - friends []
  - moodCounters {}
  
  sessions/{sessionId}/
    - gameName, playtime, won, timestamp
    - squadMembers []
    
  platformAccounts/{platform}/
    - accountId, accessToken, refreshToken
    - profileData {}
```

### 5. AI Agent (Ollama Local LLM)
**File**: `Assets/Scripts/AI/AIAgent.cs`

```
AIAgent
├── Initialize()
│   ├── CheckOllamaConnection()
│   └── LoadModel(llama3.2:3b or phi-3:mini)
├── GeneratePlaystyleSummary()
│   ├── BuildPlaystylePrompt()
│   └── QueryAI()
│       └── POST /ollama/api/generate
├── SuggestGames()
│   ├── BuildGameSuggestionPrompt()
│   └── QueryAI()
│       └── Parse response into game list
└── FindSquadMates(candidateIds)
    ├── BuildSquadmatePrompt()
    └── QueryAI()
        └── Parse response into player IDs
```

**Ollama Integration**:
- Runs locally on device (Android bridge needed)
- Models: llama3.2:3b (1B parameters) or phi-3:mini
- HTTP endpoint: `http://localhost:11434/api/generate`
- Request/response streaming for fast feedback

**Prompts**:
```
PlaystyleSummary:
  "Based on this gaming profile:
   - Recent games: [list]
   - Total playtime: [hours]
   - Win rate: [%]
   
   Generate a 1-2 sentence playstyle summary."

GameSuggestions:
  "Player plays: [genres]
   Suggest 3-5 games this player would enjoy."

SquadmateFinding:
  "Match this player with compatible squadmates from: [IDs]
   Return top 3 compatible player IDs."
```

### 6. Mood Manager
**File**: `Assets/Scripts/Core/MoodManager.cs`

```
MoodManager
├── StartTracking()
│   ├── Load lastActivityTime from PlayerPrefs
│   ├── Load currentMood
│   └── SetupRepeatingChecks()
│       ├── CheckInactivity() every 60s
│       └── UpdateMoodState() every 300s
├── RecordActivity()
│   ├── Update lastActivityTime
│   ├── Save to PlayerPrefs
│   └── SetMood("happy") if sad
├── CheckInactivity()
│   ├── Calculate hours inactive
│   ├── If >= 72h → SetMood("sad")
│   │   └── Play SadSlump + "Missed playing with you"
│   └── Sync to Firebase
├── SetMood(mood)
│   ├── Update PlayerPrefs
│   └── FirebaseManager.UpdateUserMood()
├── RecordWin(gameName)
│   ├── RecordActivity()
│   ├── Play DanceVictory
│   └── Speak("That's what I'm talking about!")
└── RecordLoss(gameName)
    ├── RecordActivity()
    ├── Play ThumbsUp
    └── Speak("You'll get them next time!")
```

**Mood States**:
- **happy**: Active player (playtime today)
- **thinking**: Inactive 1+ day
- **sad**: Inactive 3+ days
- **excited**: Strong streak (7+ days)

**Cloud Function Integration**:
- Daily calculation at 00:00 UTC
- Updates mood for all users based on activity
- Triggers inactivity reminders

### 7. Platform Integration
**File**: `Assets/Scripts/Services/PlatformIntegration.cs`

```
PlatformIntegration
├── AuthenticateSteam()
│   ├── OAuth flow → authorization code
│   └── Exchange for token → Keystore
├── AuthenticateXbox()
│   ├── MSAL flow
│   └── Get Xbox Live token
├── AuthenticatePlayStation()
│   ├── PSN OAuth
│   └── Get PSN token
├── AuthenticateEpic()
│   ├── Device code flow
│   └── Get Epic token
├── AuthenticateGitHub()
│   ├── GitHub OAuth
│   └── Get GitHub token
├── AggregateUserStats()
│   ├── FetchSteamStats()
│   ├── FetchXboxStats()
│   ├── FetchPlayStationStats()
│   ├── FetchEpicStats()
│   └── FetchGitHubStats()
├── LoadTokensFromKeystore()
│   └── Decrypt stored tokens
├── SaveTokensToKeystore()
│   └── Securely save tokens
└── RefreshTokens()
    └── Called every 12 hours
```

**OAuth Flows**:
- **Steam**: Web API with API key (no user auth needed)
- **Xbox**: MSAL with device flow or web view
- **PSN**: OAuth redirect, handle OAuth callback
- **Epic**: Device code flow for better UX
- **GitHub**: OAuth with personal access token or web flow

**Token Storage**:
- Android Keystore for encryption
- Automatic refresh every 12 hours
- Secure credential storage

### 8. Matchmaking System
**File**: `Assets/Scripts/Matchmaking/MatchmakingManager.cs`

```
MatchmakingManager
├── StartSquadSearch()
│   ├── Play SearchPose animation
│   ├── AnalyzeCurrentGame()
│   │   ├── Query platform APIs
│   │   └── Determine active game
│   ├── GetCandidatePlayers()
│   │   ├── Query Firestore users
│   │   └── Get friends from platforms
│   ├── AIAgent.FindSquadMates(candidates)
│   │   └── AI ranks compatibility
│   ├── OnSquadFound()
│   │   ├── Play SquadUp
│   │   └── Show squad UI
│   └── OnNoSquadFound()
│       ├── Play Shrug
│       └── Encourage continue playing
└── InvitePlayer(playerId)
    └── Open platform-specific invite
        ├── Xbox Live party
        ├── PSN group
        ├── Discord server
        └── Generic invite link
```

**Squad Finding Algorithm**:
1. AI analyzes current game from platforms
2. Gets list of available players (friends + public)
3. AI scores compatibility (1-100):
   - Playstyle match (+40 if similar)
   - Skill level match (+30)
   - Timezone/online status (+20)
   - Mutual friends (+10)
4. Returns top 3-5 candidates
5. Player can invite with one tap

### 9. In-App Purchases
**File**: `Assets/Scripts/Monetization/InAppPurchaseManager.cs`

```
InAppPurchaseManager
├── Initialize()
│   └── InitializeGooglePlayBilling()
│       └── BillingClient.startConnection()
├── CheckPremiumStatus()
│   └── Query active subscriptions
│       └── Check PREMIUM_MONTHLY_SKU
├── StartPremiumSubscription()
│   └── BillingFlow(PREMIUM_MONTHLY_SKU)
│       └── Show Google Play dialog
├── OnPremiumPurchased()
│   ├── Verify purchase (optional)
│   ├── FirebaseManager.UpdatePremiumTier(true)
│   └── UnlockPremiumFeatures()
│       ├── Load premium skins
│       ├── Enable animations
│       └── Load ElevenLabs voice
└── IsPremium()
```

**Premium Features** ($4.99/month):
- Custom avatar skins (5+ options)
- Premium animations (10+ exclusive)
- ElevenLabs voice synthesis (better quality)
- Ad-free experience (future)
- Early access to features

---

## Data Persistence Strategy

### Local Storage (SQLite)
```
GamerWingman.db
├── users (userId, lastPlayTime, totalPlaytime, mood)
├── sessions (sessionId, gameName, playtime, won, timestamp)
├── platform_accounts (platform, accountId, token, refreshToken)
└── cached_avatars (userId, glbFilePath, hash)
```

### Cloud Storage (Firebase)
- **Firestore**: User profiles, sessions, stats
- **Cloud Storage**: Avatar backups, replay videos (future)
- **Realtime Database** (optional): Squad matchmaking queue

### File System Caching
```
Application.persistentDataPath/
├── avatar.glb (avatar model)
├── platform_tokens/ (encrypted in Keystore)
├── config.json
└── temp/
```

---

## Event Flow Examples

### Example 1: User Wins a Game
```
Game App (Fortnite) → GamerWingman
  ├── Achievement popup detected
  └── Victory event triggered
    
MoodManager.RecordWin("Fortnite")
  ├── Update lastActivityTime
  ├── AnimationManager.Play("DanceVictory")
  ├── VoiceManager.Speak("That's what I'm talking about!")
  └── FirebaseManager.RecordGameSession()
      ├── Add session to Firestore
      └── Increment streak
      
Cloud Functions
  ├── analyzeGameSession() triggered
  └── Update total playtime, mood → "excited"
```

### Example 2: "Find Squad" Command
```
User: "Hey Wingman, find squad"
  └── VoiceManager.OnWakeWordDetected()
      └── CaptureVoiceCommand() → "find squad"
        
MatchmakingManager.StartSquadSearch()
  ├── Play SearchPose animation
  ├── PlatformIntegration.AggregateUserStats()
  │   └── Fetch from Steam, Xbox, Epic, PSN
  ├── GetCandidatePlayers()
  │   └── Query Firestore + platforms
  └── AIAgent.FindSquadMates(candidates)
      └── Ollama: "Match player with compatible squadmates"
        
AIAgent.QueryAI()
  ├── Build prompt
  ├── POST http://localhost:11434/api/generate
  ├── llama3.2:3b processes
  └── Returns top 3 player IDs
    
MatchmakingManager.OnSquadFound()
  ├── Play SquadUp animation
  ├── VoiceManager.Speak("Found awesome players!")
  └── Show squad UI with invite buttons
    
User taps Invite
  └── MatchmakingManager.InvitePlayer(playerId)
      └── Open Xbox/PSN/Discord invite
```

### Example 3: Inactivity Detection (3+ Days)
```
Time: Day 1, User plays
  └── MoodManager.RecordActivity()
      └── lastActivityTime = now
        
Time: Day 4, App runs
  └── MoodManager.CheckInactivity()
      ├── Calculate: 96 hours > 72 hours threshold
      ├── AnimationManager.Play("SadSlump")
      ├── VoiceManager.Speak("I've missed playing with you!")
      └── MoodManager.SetMood("sad")
          └── FirebaseManager.UpdateUserMood("sad")
          
Cloud Functions (nightly)
  ├── calculateUserMood() runs
  ├── For each user, check lastPlayTime
  ├── If inactive 3+ days → mood = "sad"
  └── Update Firestore
```

---

## Technology Stack

| Component | Technology |
|-----------|----------|
| **Engine** | Unity 6 LTS (2023.2.1f1) |
| **Render** | HDRP (High Definition Render Pipeline) |
| **Avatar** | Ready Player Me SDK |
| **Animations** | Mixamo + Unity Animator |
| **Voice Input** | Porcupine + Android SpeechRecognizer |
| **Voice Output** | Android TTS / ElevenLabs API |
| **AI** | Ollama (llama3.2:3b / phi-3:mini) |
| **Backend** | Firebase (Auth, Firestore, Cloud Functions) |
| **Local DB** | SQLite |
| **Platform OAuth** | Steam API, MSAL, PSN SDK, Epic SDK, GitHub API |
| **Payments** | Google Play Billing Library |
| **Scripting** | C# (.NET 4.7.1+) |
| **Build Pipeline** | Gradle + GitHub Actions |

---

## Security Considerations

### Authentication
- **Anonymous Firebase Auth** → No email required
- **Platform OAuth** → Tokens stored in Android Keystore
- **Token Refresh** → Every 12 hours, before expiry

### Data Privacy
- **Firestore Security Rules** → Users can only read/write own data
- **HTTPS Only** → All API calls encrypted
- **Sensitive Data** → Tokens encrypted in Keystore
- **Analytics** → No personal data collection

### Permissions
- **Microphone** → Recording audio for voice commands
- **Internet** → Firebase + platform APIs
- **Storage** → Caching avatars locally
- **Camera** → Future avatar customization

---

## Scalability

### Current Limits
- **Users**: Firebase free tier → 1GB storage, 50K reads/day
- **Cloud Functions**: 2M invocations/month free
- **Storage**: 5GB free tier

### Scaling Path
1. **Tier 1**: 10K active users → Pay-as-you-go Firebase
2. **Tier 2**: 100K users → Dedicated Firebase instance
3. **Tier 3**: 1M+ users → Sharded Firestore, CDN for assets

### Optimization
- **Offline Mode**: SQLite sync queue when offline
- **Asset Streaming**: Addressables for animations/skins
- **Caching**: Avatar caching, platform token caching
- **Batching**: Group Firestore writes

---

For more details, see:
- `/Assets/Scripts/` - Implementation files
- `README.md` - Getting started
- `SETUP.md` - Development environment
