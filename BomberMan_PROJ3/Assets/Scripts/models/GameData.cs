using System;

[System.Serializable]
public class GameResponse {
    public string _id;
    public string hostId;
    public string status;
    public string[] players;
    public ConfigData config;
    public string createdAt;
    public string updatedAt;
}

[System.Serializable]
public class GameData {
    public string _id;
    public string hostId;
    public string status;
    public string[] players;
    public ConfigData config;
    public DateTime createdAt;
    public DateTime updatedAt;
}

[System.Serializable]
public class ConfigData {
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