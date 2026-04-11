using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
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

    // Game history
    [System.Serializable]
    public class GameRecord
    {
        public string gameName;
        public int devTime;
        public int profit;
    }

    public List<GameRecord> gameHistory = new List<GameRecord>();

    // Add game to history
    public void AddGame(string name, int devTime, int profit)
    {
        GameRecord record = new GameRecord();
        record.gameName = name;
        record.devTime = devTime;
        record.profit = profit;

        gameHistory.Add(record);
    }
}