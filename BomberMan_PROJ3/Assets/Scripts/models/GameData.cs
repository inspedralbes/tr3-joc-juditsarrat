using System;
using UnityEngine;

[System.Serializable]
public class GameResponse
{
    public string _id;
    public string gameCode;
    public string hostId;
    public string status;
    public string[] players;
    public GameConfig config;
}

[System.Serializable]
public class GameConfig
{
    public int maxPlayers;
    public string mapType;
}

[System.Serializable]
public class GameData
{
    public string id;
    public string hostId;
    public string status;
    public string[] players;
    public ConfigData config;
    public DateTime createdAt;
    public DateTime updatedAt;
}

[System.Serializable]
public class ConfigData
{
    public int maxPlayers;
    public string mapType;
}

public static class GameCodeGenerator {
    public static string GenerateCode(string gameId) {
        if (gameId.Length >= 6) {
            return gameId.Substring(gameId.Length - 6).ToUpper();
        }
        return gameId.ToUpper();
    }
}

[System.Serializable]
public class PositionalMessage {
    public float x;
    public float y;
}