# GamerWingman Setup Guide

Complete step-by-step guide to set up your development environment and prepare the project for building.

## Step 1: Install Development Tools

### Unity 6 LTS
1. Download **Unity Hub**: https://unity.com/download
2. Install **Unity 6 LTS** (2023.2.1f1 or later)
3. During installation, select:
   - Android Build Support
   - Android SDK & NDK Tools
   - OpenJDK

### Java Development Kit (JDK)
```bash
# macOS
brew install openjdk@17

# Linux (Ubuntu/Debian)
sudo apt-get install openjdk-17-jdk

# Windows
# Download from: https://www.oracle.com/java/technologies/downloads/
```

### Android SDK & NDK
Unity should handle this automatically. Verify:
```bash
# Check Android SDK location
echo $ANDROID_HOME

# Or install manually via Android Studio
# https://developer.android.com/studio
```

### Firebase CLI
```bash
npm install -g firebase-tools
firebase login
```

## Step 2: Clone & Configure Project

```bash
git clone https://github.com/yourusername/GamerWingman.git
cd GamerWingman

# Install Firebase Functions dependencies
cd functions
npm install
cd ..

# Install root dependencies
npm install
```

## Step 3: Firebase Project Setup

### Create Firebase Project
1. Go to https://console.firebase.google.com
2. Click "Create Project"
3. Name: `gamerwingman-dev`
4. Disable Google Analytics (optional)
5. Create project

### Enable Services
In Firebase Console:
1. **Authentication**
   - Enable: Anonymous
   - Optional: Google, GitHub sign-in

2. **Firestore Database**
   - Create database
   - Start in test mode
   - Region: us-central1
   - Create

3. **Cloud Functions**
   - Enable Cloud Functions API
   - Enable Cloud Pub/Sub API
   - Enable Cloud Scheduler API

4. **Cloud Storage** (for avatar backups)
   - Create bucket
   - Default location

### Download Credentials
1. Project Settings → Service Accounts
2. Download JSON key
3. Copy to `Assets/Plugins/Android/google-services.json`

**Alternative**: Let Firebase plugin auto-download:
```bash
firebase init android
# Select your project
# Follow prompts
```

## Step 4: Configure Android Build

### In Unity Editor
1. Open project in Unity
2. **File** → **Build Settings**
3. Select **Android** as platform
4. Player Settings:
   - **Other Settings**
     - Minimum API Level: 31
     - Target API Level: 34
   - **Android**
     - Enable Automatic Graphics API
     - Select Vulkan + OpenGLES3

### Keystore Setup (for signing)
1. **Edit** → **Project Settings** → **Player** → **Android**
2. **Publishing Settings**
3. Create new Keystore:
   ```
   Location: your-keystore.jks
   Password: your-password
   Key Name: release
   Key Password: your-password
   Validity: 10000 days
   ```
4. Save keystore file in project root (add to `.gitignore`)

## Step 5: Install Required Plugins

### Via Package Manager
In Unity: **Window** → **TextMesh Pro** → **Import Essentials**

### Download SDKs
Manual setup for third-party SDKs:

#### Porcupine (Wake Word Detection)
1. Download: https://console.picovoice.ai
2. Get Access Key
3. Place SDK in `Assets/Plugins/Porcupine/`
4. Add to `Assets/Scripts/Services/VoiceManager.cs`

#### Steam SDK (Optional)
1. Download Steamworks SDK: https://partner.steamgames.com/downloads/
2. Extract to `Assets/Plugins/Steam/`
3. Add to PlatformIntegration.cs

#### Microsoft MSAL (Xbox)
```bash
cd Assets/Plugins
git clone https://github.com/AzureAD/microsoft-authentication-library-for-dotnet.git
```

#### Ready Player Me SDK
Install via NuGet or manual:
1. Download: https://github.com/readyplayerme/rpm-unity-sdk
2. Extract to `Assets/`
3. Import package

## Step 6: Configure Cloud Functions

```bash
cd functions
npm install

# Set environment variables
export FIREBASE_PROJECT_ID=gamerwingman-dev
export FIREBASE_REGION=us-central1

# Deploy (requires Firebase CLI auth)
firebase deploy --only functions
```

### Cloud Scheduler Setup
In Firebase Console:
1. **Cloud Scheduler** → **Create Job**
2. Name: `daily-mood-calculation`
3. Frequency: `0 0 * * *` (daily at 00:00 UTC)
4. Execute: HTTP POST
5. URL: `https://us-central1-gamerwingman-dev.cloudfunctions.net/calculateUserMood`

## Step 7: Configure Firestore Rules

Deploy security rules:
```bash
firebase deploy --only firestore:rules
```

Test in Firestore Emulator:
```bash
firebase emulators:start
# Test reads/writes in emulator UI
```

## Step 8: GitHub Setup (Optional)

### Create GitHub Repository
1. Go to https://github.com/new
2. Repository name: `GamerWingman`
3. Add `.gitignore` → Unity
4. Create repository

### Push Initial Commit
```bash
git remote add origin https://github.com/yourusername/GamerWingman.git
git push -u origin main
```

### Configure Secrets for Actions
Go to **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Add these secrets:
```
ANDROID_SIGNING_KEY         # Base64 of your .jks file
ANDROID_KEY_ALIAS           # "release"
ANDROID_KEYSTORE_PASSWORD   # Your keystore password
ANDROID_KEY_PASSWORD        # Your key password
FIREBASE_APP_ID             # From Firebase Console
```

### Get Base64 of Keystore
```bash
base64 your-keystore.jks | tr -d '\n' | pbcopy
# Paste into GitHub secret
```

## Step 9: Configure Third-Party APIs

### Steam Web API
1. https://steamcommunity.com/dev
2. Request API key
3. Add to `config.json` or environment variable

### Xbox Live (MSAL)
1. https://portal.azure.com
2. Register app
3. Get Client ID
4. Add to PlatformIntegration.cs

### Epic Games
1. https://dev.epicgames.com
2. Register application
3. Get Client ID
4. Add to config

### Porcupine Wake Word
1. https://console.picovoice.ai
2. Create Access Key
3. Add to VoiceManager.cs

## Step 10: Verify Installation

### Test Build
```bash
# Quick development build
unity -batchmode -buildTarget Android \
  -executeMethod BuildScript.BuildAPK \
  -development
```

### Logcat Debugging
```bash
adb logcat | grep "GamerWingman"
```

### Firebase Connection
```bash
# In Unity Console, you should see:
# [FirebaseManager] Anonymous auth successful
```

## Troubleshooting

### Build Fails: "Android SDK not found"
```bash
# Set ANDROID_HOME
export ANDROID_HOME=~/Android/Sdk
export PATH=$PATH:$ANDROID_HOME/tools:$ANDROID_HOME/platform-tools
```

### Keystore Error: "Key password does not match keystore password"
- Ensure key password and keystore password are both set correctly
- Regenerate keystore if needed: `keytool -genkey -v -keystore new-keystore.jks -alias release`

### Firebase Auth Fails
1. Check google-services.json is in correct path
2. Verify Firebase project in console
3. Enable Anonymous authentication in Firebase
4. Check internet connection on device

### Microphone Permission Denied
- Grant permission in Settings → Apps → GamerWingman → Permissions
- Check AndroidManifest has `RECORD_AUDIO`

### Avatar Won't Load
- Check internet (Ready Player Me CDN)
- Verify avatar cache path exists
- Check GLTFast is properly imported

## Next Steps

1. **Start Development**: Open in Unity and begin editing scenes
2. **Add Animations**: Import Mixamo animations to `Assets/Animations/`
3. **Test Voice**: Set up Porcupine wake word testing
4. **Connect Platforms**: Implement OAuth flows for Steam, Xbox, etc.
5. **Deploy Functions**: Push Cloud Functions to Firebase
6. **Build APK**: Generate signed APK for testing
7. **Publish**: Upload to Google Play Console

---

For issues or questions, check:
- Firebase Docs: https://firebase.google.com/docs
- Unity Docs: https://docs.unity.com
- GitHub Issues: https://github.com/yourusername/GamerWingman/issues
