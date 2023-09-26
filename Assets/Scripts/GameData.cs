
public enum LobbySpawn
{
    MAIN,
    RAT,
    CAT,
    OWL
}
public static class GameData
{
    public static bool beatRat = false;
    public static int timesLostToRat = 0;
    public static int timesWonToRat = 0;

    public static bool beatCat = false;
    public static int timesLostToCat = 0;
    public static int timesWonToCat = 0;

    public static bool beatOwl = false;
    public static int timesLostToOwl = 0;
    public static int timesWonToOwl = 0;

    public static LobbySpawn lobbySpawn = LobbySpawn.MAIN;
    public static bool justLost = false;
    // Allows opening of Return to Lobby doors
    public static bool justWon = false;
    // Unlocks Sewer door
    public static bool hasKey = false;
}
