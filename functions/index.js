const functions = require("firebase-functions");
const admin = require("firebase-admin");

admin.initializeApp();
const db = admin.firestore();

/**
 * Calculate user mood based on activity and streaks
 * Triggered daily via Cloud Scheduler
 */
exports.calculateUserMood = functions
  .pubsub.schedule("every day 00:00")
  .timeZone("UTC")
  .onRun(async (context) => {
    console.log("[calculateUserMood] Running mood calculation...");

    const usersSnapshot = await db.collection("users").get();
    let moodUpdates = 0;

    for (const userDoc of usersSnapshot.docs) {
      const userId = userDoc.id;
      const userData = userDoc.data();

      // Check last activity time
      const lastPlayTime = userData.lastPlayTime?.toDate() || new Date(0);
      const hoursInactive =
        (Date.now() - lastPlayTime.getTime()) / (1000 * 60 * 60);

      let newMood = "happy";

      if (hoursInactive >= 72) {
        // 3+ days inactive
        newMood = "sad";
      } else if (hoursInactive >= 24) {
        // 1+ day inactive
        newMood = "thinking";
      } else if (userData.streak >= 7) {
        // Strong streak
        newMood = "excited";
      }

      // Update mood in Firestore
      if (newMood !== userData.mood) {
        await db
          .collection("users")
          .doc(userId)
          .update({
            mood: newMood,
            lastMoodCalculation: admin.firestore.FieldValue.serverTimestamp(),
          });

        moodUpdates++;
      }
    }

    console.log(
      `[calculateUserMood] Updated ${moodUpdates} user moods`
    );
    return null;
  });

/**
 * Analyze session and detect win/loss from game data
 * Triggered when game session ends
 */
exports.analyzeGameSession = functions.firestore
  .document("users/{userId}/sessions/{sessionId}")
  .onCreate(async (snap, context) => {
    const { userId } = context.params;
    const sessionData = snap.data();

    console.log(`[analyzeGameSession] User: ${userId}, Game: ${sessionData.gameName}`);

    // Update total playtime
    const userRef = db.collection("users").doc(userId);
    const userDoc = await userRef.get();
    const currentPlaytime = userDoc.data().totalPlaytimeSeconds || 0;

    await userRef.update({
      totalPlaytimeSeconds:
        currentPlaytime + sessionData.playtime,
      lastPlayTime: admin.firestore.FieldValue.serverTimestamp(),
    });

    // Update streak
    if (sessionData.won) {
      const currentStreak = userDoc.data().currentStreak || 0;
      await userRef.update({
        currentStreak: currentStreak + 1,
        mood: "excited", // Win triggers excitement
      });
    } else {
      // Reset streak on loss
      await userRef.update({
        currentStreak: 0,
      });
    }

    console.log(`[analyzeGameSession] Session processed successfully`);
    return null;
  });

/**
 * Generate playstyle summary from aggregated session data
 * Can be called via HTTP trigger from app
 */
exports.generatePlaystyleSummary = functions.https.onRequest(
  async (req, res) => {
    const { userId } = req.body;

    if (!userId) {
      res.status(400).json({ error: "userId required" });
      return;
    }

    console.log(`[generatePlaystyleSummary] User: ${userId}`);

    try {
      const userRef = db.collection("users").doc(userId);
      const sessionsRef = userRef.collection("sessions");
      const sessionsSnapshot = await sessionsRef.limit(20).get();

      const games = {};
      const stats = {
        totalPlaytime: 0,
        wins: 0,
        losses: 0,
        winRate: 0,
      };

      sessionsSnapshot.forEach((doc) => {
        const session = doc.data();
        games[session.gameName] =
          (games[session.gameName] || 0) + session.playtime;
        stats.totalPlaytime += session.playtime;
        if (session.won) stats.wins++;
        else stats.losses++;
      });

      stats.winRate =
        stats.wins + stats.losses > 0
          ? (stats.wins / (stats.wins + stats.losses)) * 100
          : 0;

      // Generate summary based on top games
      const topGames = Object.entries(games)
        .sort((a, b) => b[1] - a[1])
        .slice(0, 3)
        .map(([name]) => name);

      const summary = `${
        stats.winRate > 60
          ? "Competitive gamer, high win rate,"
          : "Casual enjoyer, plays for fun,"
      } loves ${topGames.join(", ")}`;

      res.json({
        summary,
        stats,
        topGames,
      });
    } catch (error) {
      console.error("[generatePlaystyleSummary] Error:", error);
      res.status(500).json({ error: error.message });
    }
  }
);
