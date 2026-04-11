using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    // --- Constants ---
    public const int STAT_CAP = 20;
    public const int STARTING_STAT_POINTS = 10;
    public const int STARTING_MONEY = 5000;

    // Core stats
    public int graduationYear;
    public int currentYear;
    public int money;

    // Skills (default = 1)
    public int writing = 1;
    public int modeling = 1;
    public int gameplay = 1;
    public int programming = 1;
    public int personality = 1;
    public int fanbase = 1;

    // Stat allocation
    public int statPointsRemaining;

    // Seed (derived from graduation year + system clock)
    public int gameSeed;

    // Genre and trend data
    public static readonly string[] AllGenres = new string[]
    {
        "Shooter", "Action-Adventure", "Role-Playing", "Sports", "Simulation",
        "Racing", "Fighting", "Platformer", "Survival Craft Sandbox", "Strategy",
        "Survival Horror", "Open World", "Roguelike/lite", "Deck Builder",
        "Party/Casual", "Idle", "Narrative", "Rhythm", "Tower Defense", "Puzzle"
    };

    // Trend levels: 1 (!) to 6 (!!!!!!) per genre
    public Dictionary<string, int> genreTrends = new Dictionary<string, int>();

    // Game history
    [System.Serializable]
    public class GameRecord
    {
        public string gameName;
        public int devTime;
        public int profit;
        public int metacriticScore;
        public string genre;
        public string subGenre;
    }

    public List<GameRecord> gameHistory = new List<GameRecord>();

    // --- Initialization ---
    public void InitializePlayer(int gradYear)
    {
        graduationYear = gradYear;
        currentYear = gradYear;
        money = STARTING_MONEY;
        statPointsRemaining = STARTING_STAT_POINTS;

        // Seed from graduation year + system time
        gameSeed = gradYear * 1000 + System.DateTime.Now.Millisecond;
        Random.InitState(gameSeed);

        // Initialize trends randomly (2-5 range to start)
        genreTrends.Clear();
        foreach (string genre in AllGenres)
        {
            genreTrends[genre] = Random.Range(2, 6);
        }
    }

    // --- Stat Allocation ---
    public int GetStat(string statName)
    {
        switch (statName)
        {
            case "writing": return writing;
            case "modeling": return modeling;
            case "gameplay": return gameplay;
            case "programming": return programming;
            case "personality": return personality;
            case "fanbase": return fanbase;
            default: return 0;
        }
    }

    public void SetStat(string statName, int value)
    {
        switch (statName)
        {
            case "writing": writing = value; break;
            case "modeling": modeling = value; break;
            case "gameplay": gameplay = value; break;
            case "programming": programming = value; break;
            case "personality": personality = value; break;
            case "fanbase": fanbase = value; break;
        }
    }

    public bool TryIncreaseStat(string statName)
    {
        if (statPointsRemaining <= 0) return false;
        int current = GetStat(statName);
        if (current >= STAT_CAP) return false;

        SetStat(statName, current + 1);
        statPointsRemaining--;
        return true;
    }

    public bool TryDecreaseStat(string statName)
    {
        int current = GetStat(statName);
        if (current <= 1) return false; // minimum stat is 1

        SetStat(statName, current - 1);
        statPointsRemaining++;
        return true;
    }

    // --- Industry Trends ---
    public void AdvanceTrends()
    {
        List<string> keys = new List<string>(genreTrends.Keys);
        foreach (string genre in keys)
        {
            int shift = Random.Range(-1, 2); // -1, 0, or 1
            int newTrend = Mathf.Clamp(genreTrends[genre] + shift, 1, 6);
            genreTrends[genre] = newTrend;
        }
    }

    public float GetTrendMultiplier(string genre)
    {
        if (!genreTrends.ContainsKey(genre)) return 1f;
        // Trend 1 = 0.5x, Trend 3 = 1.0x, Trend 6 = 1.75x
        return 0.25f + genreTrends[genre] * 0.25f;
    }

    public string GetTrendDisplay(string genre)
    {
        if (!genreTrends.ContainsKey(genre)) return "?";
        int level = genreTrends[genre];
        return new string('!', level);
    }

    // --- Game Quality Calculation ---
    public int CalculateGameQuality(string genre, string subGenre)
    {
        // Weighted skill contributions based on genre
        float score = 0f;

        // Programming always matters
        score += programming * 2f;

        // Genre-specific weights
        switch (genre)
        {
            case "Role-Playing":
            case "Narrative":
                score += writing * 3f + gameplay * 1f + modeling * 1f;
                break;
            case "Shooter":
            case "Action-Adventure":
            case "Fighting":
                score += modeling * 3f + gameplay * 2f + writing * 0.5f;
                break;
            case "Platformer":
            case "Roguelike/lite":
            case "Tower Defense":
                score += gameplay * 3f + modeling * 1f + writing * 0.5f;
                break;
            case "Puzzle":
            case "Strategy":
            case "Deck Builder":
                score += programming * 2f + gameplay * 2f + writing * 1f;
                break;
            case "Simulation":
            case "Sports":
            case "Racing":
                score += modeling * 2f + gameplay * 2f + programming * 1f;
                break;
            case "Survival Horror":
            case "Survival Craft Sandbox":
            case "Open World":
                score += modeling * 2f + gameplay * 2f + writing * 1.5f;
                break;
            case "Party/Casual":
            case "Idle":
            case "Rhythm":
                score += gameplay * 2f + personality * 1.5f + modeling * 1f;
                break;
            default:
                score += gameplay * 2f + modeling * 1f + writing * 1f;
                break;
        }

        // Sub-genre adds a small bonus from its relevant skills too
        if (!string.IsNullOrEmpty(subGenre) && subGenre != genre)
        {
            score += gameplay * 0.5f;
        }

        // Normalize to 1-98 (Metacritic scale)
        // Max possible: ~20*2 + 20*3 + 20*2 + 20*1 = 160ish at max stats
        // Min possible: ~1*2 + 1*3 + 1*2 + 1*1 = 8ish at min stats
        float normalized = Mathf.Clamp01((score - 5f) / 155f);
        int metacritic = Mathf.Clamp(Mathf.RoundToInt(normalized * 97f) + 1, 1, 98);

        // Add some randomness (+/- 10)
        metacritic += Random.Range(-10, 11);
        metacritic = Mathf.Clamp(metacritic, 1, 98);

        return metacritic;
    }

    // --- Profit Calculation ---
    public int CalculateProfit(string genre, string subGenre, int devTime)
    {
        int quality = CalculateGameQuality(genre, subGenre);
        float trendMult = GetTrendMultiplier(genre);

        // Personality multiplier: small unless capped
        float personalityMult = (personality >= STAT_CAP) ? 2.0f : 1.0f + personality * 0.02f;

        // Previous work earnings bonus
        float previousEarnings = 0f;
        foreach (var record in gameHistory)
        {
            previousEarnings += record.profit * 0.5f;
        }

        // Base profit from quality score
        float baseProfit = quality * 50f; // score of 50 = $2500 base

        float totalProfit = (baseProfit * trendMult * personalityMult) + previousEarnings;

        // Dev time bonus: 2-year games get a quality bump
        if (devTime >= 2) totalProfit *= 1.25f;

        return Mathf.Max(100, Mathf.RoundToInt(totalProfit));
    }

    // --- Game Record ---
    public void AddGame(string name, int devTime, int profit, int score, string genre, string subGenre)
    {
        GameRecord record = new GameRecord();
        record.gameName = name;
        record.devTime = devTime;
        record.profit = profit;
        record.metacriticScore = score;
        record.genre = genre;
        record.subGenre = subGenre;

        gameHistory.Add(record);
    }

    // --- Accolades ---
    public List<string> GetAccolades()
    {
        List<string> accolades = new List<string>();

        if (programming >= STAT_CAP) accolades.Add("Bug-Free!");
        if (writing >= STAT_CAP) accolades.Add("Master Storyteller");
        if (modeling >= STAT_CAP) accolades.Add("Visual Virtuoso");
        if (gameplay >= STAT_CAP) accolades.Add("Fun Factory");
        if (personality >= STAT_CAP) accolades.Add("Fan Favorite");
        if (fanbase >= STAT_CAP) accolades.Add("Household Name");

        // Legend check: all accolades earned
        if (accolades.Count >= 6)
        {
            accolades.Add("LEGEND");
        }

        return accolades;
    }
}
