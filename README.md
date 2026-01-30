# GamerWingman - Your AI Gaming Companion

A premium Android app (Unity 6 LTS) that brings an intelligent AI avatar companion to gamers. Track playtime across platforms, get personalized game suggestions, find compatible squad mates, and enjoy an immersive gaming companion experience.

## Features

### 🤖 AI-Powered Companion
- **Local AI Brain**: Ollama-powered llama3.2:3b or phi-3:mini runs locally on your device
- **Playstyle Analysis**: Generates personalized gaming profile summaries
- **Squad Matching**: Finds compatible players based on gaming habits and playstyle
- **Voice Commands**: Always-listening "Hey Wingman" wake word using Porcupine
- **Dynamic Animations**: 20+ Mixamo animations triggered by game events

### 🎮 Multi-Platform Integration
- **Steam Web API** - Track games, playtime, achievements
- **Xbox Live** (MSAL) - Xbox achievements and gamerscore
- **PlayStation Network** - Trophy data and online status
- **Epic Games** - Fortnite stats and vault progress
- **GitHub** - For developer gamers tracking repositories

### 👥 Squad Finder & Matchmaking
- One-tap "Find squad" voice command
- AI analyzes current game and suggests 3-5 compatible players
- Platform-aware invites (Xbox Live party, PSN group, Discord, etc.)
- Mutual friend detection

### 😊 Mood-Based Interactions
- Avatar reacts to inactivity: 3+ days → sad_slump + "Missed playing with you"
- Win/loss detection triggers celebrations or comfort animations
- Daily mood calculation via Cloud Functions
- Streak tracking with mood bonuses

### 💰 Premium Monetization
- $4.99/month subscription via Google Play Billing
- Unlocks: custom avatar skins, premium animations, ElevenLabs voice packs
- Cross-device sync via Firebase Firestore

### 🎯 Data & Analytics
- Local SQLite database for offline functionality
- Firebase Firestore sync for cloud backup
- User mood counters and playstyle summaries
- Session history with win/loss tracking
- Anonymous Firebase auth (no email required)

---

## Project Structure

```
GamerWingman/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs         # Main entry point, orchestrates initialization
│   │   │   ├── MoodManager.cs         # Inactivity tracking, mood state
│   │   │   └── BootstrappedScene.unity
│   │   ├── Avatar/
│   │   │   └── AvatarManager.cs       # Ready Player Me integration, caching
│   │   ├── Animations/
│   │   │   └── AnimationManager.cs    # Animator controller, blendshape sync
│   │   ├── Services/
│   │   │   ├── VoiceManager.cs        # Wake word + TTS + lip-sync
│   │   │   └── PlatformIntegration.cs # OAuth for Steam, Xbox, PSN, Epic, GitHub
│   │   ├── Firebase/
│   │   │   └── FirebaseManager.cs     # Auth, Firestore CRUD, user profiles
│   │   ├── AI/
│   │   │   └── AIAgent.cs             # Ollama integration, playstyle analysis
│   │   ├── Matchmaking/
│   │   │   └── MatchmakingManager.cs  # Squad finding, player matching
│   │   ├── Monetization/
│   │   │   └── InAppPurchaseManager.cs # Google Play Billing
│   │   ├── UI/
│   │   │   └── UIManager.cs           # Immersive UI, floating mic button
│   │   └── Data/
│   │       └── UserProfile.cs         # Data models (Serializable)
│   ├── Animations/                     # Mixamo animation files
│   ├── Addressables/                   # Addressable groups for avatar skins, animations
│   ├── Prefabs/                        # UI prefabs (achievement popup, squad card, etc.)
│   ├── Materials/                      # Void background, particle shaders
│   ├── Shaders/                        # Custom shaders for glow effects
│   ├── Audio/                          # Voice pack assets
│   ├── Plugins/
│   │   └── Android/
│   │       ├── AndroidManifest.xml    # Permissions, activities, receivers
│   │       └── google-services.json    # Firebase config (placeholder)
│   └── StreamingAssets/               # Ollama model hints, config
├── ProjectSettings/
│   └── ProjectSettings.asset           # Unity 6 LTS, HDRP, Android config
├── Packages/
│   └── manifest.json                   # Firebase SDK, Porcupine, GLTFast, etc.
├── .github/
│   └── workflows/
│       └── build-apk.yml               # GitHub Actions: build, sign, deploy APK
├── functions/
│   ├── index.js                        # Cloud Functions (mood calc, session analysis)
│   └── package.json
├── firestore.rules                     # Firestore security rules
├── firebaserc                          # Firebase project config
├── package.json                        # Node scripts for deployment
├── .gitignore
└── README.md                           # This file
```

---

## Prerequisites

### Local Development
- **Unity 6 LTS** (2023.2.1f1 or later)
- **Android SDK** (API level 31+)
- **NDK** version 25.2.9519653 or later
- **Java Development Kit** (JDK 17+)
- **Ollama** (optional, for local AI testing)
- **Firebase CLI** (`npm install -g firebase-tools`)
- **Git** for version control

### Firebase Project Setup
1. Create Firebase project: https://console.firebase.google.com
2. Enable:
   - Authentication (Anonymous)
   - Firestore Database
   - Cloud Functions
   - Cloud Storage
3. Download `google-services.json` and place in `Assets/Plugins/Android/`

### Third-Party SDKs & APIs
- **Porcupine** (Picovoice) - Wake word detection
- **Steam Web API** - Requires Steamworks SDK
- **Xbox Live SDK** - Microsoft Authentication Library (MSAL)
- **PSN SDK** - PlayStation Network authentication
- **Epic Games SDK** - For Fortnite stats
- **ElevenLabs API** - Premium voice synthesis (optional)
- **GLTFast** - For loading Ready Player Me avatars

---

## Installation & Build

### 1. Clone Repository
```bash
git clone https://github.com/yourusername/GamerWingman.git
cd GamerWingman
```

### 2. Install Dependencies
```bash
# Firebase functions
cd functions
npm install
cd ..

# Root packages
npm install
```

### 3. Configure Firebase
```bash
# Copy placeholder and fill in your credentials
cp Assets/Plugins/Android/google-services.json.example Assets/Plugins/Android/google-services.json

# Edit with your Firebase project ID and private key
# Or login via Firebase CLI
firebase login
firebase init
```

### 4. Open in Unity
1. Open `Unity Hub` → Add Project → select `GamerWingman/`
2. Open with **Unity 6 LTS (2023.2.1f1)**
3. Wait for imports to complete
4. Select Android as target platform: `File` → `Build Settings` → Android

### 5. Build APK
```bash
# Option A: From command line (requires Unity activation)
unity -batchmode -buildTarget Android -executeMethod BuildScript.BuildAPK

# Option B: From Unity Editor
# File → Build Settings → Player Settings → Android → Build
```

### 6. Sign & Deploy
```bash
# Manual signing
jarsigner -verbose -sigalg SHA256withRSA -digestalg SHA-256 \
  -keystore your-keystore.jks \
  build/GamerWingman.apk release

# Or use GitHub Actions (see .github/workflows/build-apk.yml)
```

---

## Configuration

### AndroidManifest.xml Permissions
All required permissions are pre-configured:
- `INTERNET` - Firebase, platform APIs
- `RECORD_AUDIO` - Voice input (wake word + speech recognition)
- `CAMERA` - Avatar customization
- `READ/WRITE_EXTERNAL_STORAGE` - Avatar caching
- `USE_BIOMETRIC` - Future secure auth
- `QUERY_ALL_PACKAGES` - Platform app detection

### Firebase Firestore Collections
```
users/
├── {userId}/
│   ├── userId, createdAt, lastPlayTime
│   ├── totalPlaytime, streak, mood
│   ├── playstyleSummary, premiumTier
│   └── sessions/
│       └── {sessionId}/
│           ├── gameName, playtime, won, timestamp
│           └── squadMembers[]
```

### Project Settings
- **Render Pipeline**: High Definition Render Pipeline (HDRP)
- **Scripting Backend**: IL2CPP
- **API Level**: 31 (Android 12 minimum)
- **Target API**: 34 (Android 14)
- **Screen Orientation**: Portrait
- **Fullscreen Mode**: Immersive sticky (hides system UI)

---

## Usage

### First Launch
1. App opens → Immersive black screen
2. Anonymous Firebase auth → Creates user profile in Firestore
3. Prompts for avatar creation → Opens Ready Player Me WebView
4. Downloads avatar GLB → Caches locally
5. Shows avatar → Ready for interaction

### Voice Commands
Say "**Hey Wingman**" followed by:
- "Find squad" → AI finds compatible players
- "What games should I play?" → AI suggests titles
- "How's my mood?" → Returns current mood state
- "Show my stats" → Displays playtime summary
- "Invite [player name]" → Opens platform invite

### Avatar Animations
Trigger via events or voice:
- **Win detected**: `DanceVictory` + "That's what I'm talking about!"
- **Loss detected**: `ThumbsUp` + "You'll get them next time"
- **Idle 3+ days**: `SadSlump` + "Missed playing with you"
- **Squad found**: `SquadUp` + celebration animation

### Premium Subscription
- Tap premium button → Google Play Billing
- $4.99/month → Unlocks skins, animations, voice packs
- Synced to Firestore for cloud restore

---

## Deployment

### GitHub Actions (Automatic)
Push to `main` branch → Automatically:
1. Builds APK with Unity
2. Signs with keystore
3. Uploads to Firebase App Distribution (testers group)
4. Deploys Cloud Functions
5. Sends Slack notification

**Required secrets** in GitHub:
```
ANDROID_SIGNING_KEY         # Base64-encoded keystore
ANDROID_KEY_ALIAS           # Key alias
ANDROID_KEYSTORE_PASSWORD   # Keystore password
ANDROID_KEY_PASSWORD        # Key password
FIREBASE_APP_ID             # From Firebase Console
SLACK_WEBHOOK               # For notifications
```

### Manual Deployment
```bash
# Deploy Cloud Functions
firebase deploy --only functions

# Deploy Firestore rules
firebase deploy --only firestore

# Deploy to Google Play Console
# Upload signed APK via: https://play.google.com/console
```

---

## Architecture & Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     Avatar Display                          │
│                   (Ready Player Me)                         │
└──────────────────────────┬──────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼───┐        ┌─────▼────┐     ┌──────▼──────┐
   │ Voice  │        │Animation │     │   Mood      │
   │Manager │        │Manager   │     │ Manager     │
   └────┬───┘        └─────┬────┘     └──────┬──────┘
        │                  │                  │
        └──────────────────┼──────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼──────────┐  ┌────▼─────────┐  ┌────▼──────┐
   │  AI Agent     │  │ Matchmaking  │  │ Platform  │
   │  (Ollama)     │  │  Manager     │  │ Integration
   └────┬──────────┘  └────┬─────────┘  └────┬──────┘
        │                  │                  │
        └──────────────────┼──────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼──────────┐  ┌────▼──────────┐  ┌───▼──────────┐
   │ Firebase      │  │ SQLite        │  │ In-App Purch.│
   │ (Cloud)       │  │ (Local)       │  │ (Billing)    │
   └───────────────┘  └───────────────┘  └──────────────┘
```

---

## Cloud Functions

### calculateUserMood (Daily at 00:00 UTC)
- Checks inactivity duration for each user
- Updates mood: "happy" → "thinking" (1+ day) → "sad" (3+ days)
- Updates streak counters

### analyzeGameSession (On session end)
- Detects win/loss based on game data
- Updates total playtime
- Increments/resets streak
- Triggers celebration animations

### generatePlaystyleSummary (HTTP trigger)
- Aggregates recent game sessions
- Calculates win rate, top games, playtime
- Returns AI-generated summary for UI display

---

## Testing

### Unit Tests (TODO)
```bash
npm test
```

### Firebase Emulator
```bash
firebase emulators:start
```

### Building for Testing
```bash
# Build with Development Player (faster, debugging enabled)
unity -batchmode -buildTarget Android \
  -executeMethod BuildScript.BuildAPK -development
```

---

## Troubleshooting

### APK Build Fails
- Ensure Android SDK is installed: `sdkmanager --list_installed`
- Check NDK version: `ls ~/Android/ndk/`
- Verify Unity Android Tools: Window → TextMesh Pro → Import Essentials

### Voice Commands Not Working
- Check microphone permission granted: Settings → App Permissions
- Ensure Porcupine SDK is imported correctly
- Test wake word in logcat: `adb logcat | grep "WakeWord"`

### Avatar Won't Load
- Check internet connection (Ready Player Me download)
- Verify avatar cache path: `adb shell ls /data/data/com.gamerwingman.app/files/`
- Check GLTFast import settings

### Firebase Auth Fails
- Verify Firebase project is active: https://console.firebase.google.com
- Check google-services.json is in correct path
- Enable Anonymous authentication in Firebase Console

### Firestore Rules Block Writes
- Check security rules in `firestore.rules`
- Ensure request.auth.uid matches document userId
- Test rules in Firestore Emulator first

---

## API Keys & Secrets

**DO NOT commit secrets to git!** Use environment variables:

```bash
# .env (add to .gitignore)
PORCUPINE_ACCESS_KEY=your_key_here
STEAM_API_KEY=your_key_here
EPIC_CLIENT_ID=your_id_here
ELEVENLABS_API_KEY=your_key_here
```

Load in C# with:
```csharp
string apiKey = System.Environment.GetEnvironmentVariable("PORCUPINE_ACCESS_KEY");
```

---

## Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/your-feature`
3. Commit changes: `git commit -am 'Add new feature'`
4. Push to branch: `git push origin feature/your-feature`
5. Submit Pull Request

---

## License

MIT License - see LICENSE file for details

---

## Support

- **Issues**: GitHub Issues
- **Discord**: [Join our community]
- **Email**: support@gamerwingman.app

---

## Roadmap

- [ ] Implement Porcupine SDK (wake word detection)
- [ ] Integrate Ready Player Me avatar system
- [ ] Complete platform OAuth flows (Steam, Xbox, PSN, Epic)
- [ ] Ollama Android bridge for local LLM
- [ ] Cloud Functions deployment
- [ ] Google Play Billing integration
- [ ] ElevenLabs voice synthesis
- [ ] Matchmaking algorithm refinement
- [ ] Squad UI and player card system
- [ ] Public leaderboards

---

**Built with ❤️ for gamers by gamers**
