namespace DerethPulse;

public class Settings
{
    /// <summary>
    /// Enables output of more verbose logging
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    
    /// <summary>
    /// Enables output of Player location data
    /// </summary>
    public bool EnablePlayerDataOutput { get; set; } = true;

    /// <summary>
    /// The maximum number of players to include in output per each update
    /// <para>Use -1 to have no limit</para>
    /// </summary>
    public int PlayerDataMaxPlayersToOutput { get; set; } = 100;

    /// <summary>
    /// The number of seconds between player data updates
    /// </summary>
    public int PlayerDataUpdateIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// The path to write to for Player Data
    /// </summary>
    public string PlayerDataOutputPath { get; set; } = "DerethMaps-master";

    /// <summary>
    /// The file to write to for Player Data
    /// </summary>
    public string PlayerDataOutputFileName { get; set; } = "dynamicPlayers.json";


    /// <summary>
    /// Enables output of Landblock activity data
    /// </summary>
    public bool EnableLandblockDataOutput { get; set; } = true;

    /// <summary>
    /// The number of seconds between landblock data updates
    /// </summary>
    public int LandblockDataUpdateIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// The path to write to for Landblock Data
    /// </summary>
    public string LandblockDataOutputPath { get; set; } = "DerethMaps-master";

    /// <summary>
    /// The file to write to for Landblock Data
    /// </summary>
    public string LandblockDataOutputFileName { get; set; } = "dynamicLandblocks.json";
}
