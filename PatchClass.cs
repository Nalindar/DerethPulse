namespace DerethPulse;

[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    private Timer? playerTimer;
    private Timer? landblockTimer;
    private string playerJsonFilePath = string.Empty;
    private string landblockJsonFilePath = string.Empty;

    public override Task OnStartSuccess()
    {
        try
        {
            // Ensure settings are loaded
            if (Settings == null)
            {
                ModManager.Log("DerethPulse: ERROR - Settings not loaded, using defaults");
                return Task.CompletedTask;
            }

            if (Settings.EnableLogging)
                ModManager.Log("DerethPulse: Initializing tracking systems...");
            
            // Set up JSON file paths
            playerJsonFilePath = GetPlayerJsonFilePath();
            landblockJsonFilePath = GetLandblockJsonFilePath();
            
            // Start player timer if enabled
            if (Settings.EnablePlayerTracking)
            {
                var interval = TimeSpan.FromSeconds(Settings.UpdateIntervalSeconds);
                playerTimer = new Timer(UpdatePlayerData, null, TimeSpan.Zero, interval);
                
                if (Settings.EnableLogging)
                    ModManager.Log($"DerethPulse: Player tracking system online and scanning every {Settings.UpdateIntervalSeconds} seconds");
            }
            
            // Start landblock timer if enabled
            if (Settings.EnableLandblockTracking)
            {
                var landblockInterval = TimeSpan.FromSeconds(Settings.LandblockUpdateIntervalSeconds);
                landblockTimer = new Timer(UpdateLandblockData, null, TimeSpan.Zero, landblockInterval);
                
                if (Settings.EnableLogging)
                    ModManager.Log($"DerethPulse: Landblock tracking system online and scanning every {Settings.LandblockUpdateIntervalSeconds} seconds");
            }
        }
        catch (Exception ex)
        {
            ModManager.Log($"DerethPulse: ERROR during initialization - {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    public override void Stop()
    {
        // Dispose timers to stop new callbacks
        playerTimer?.Dispose();
        landblockTimer?.Dispose();
        
        if (Settings?.EnableLogging == true)
            ModManager.Log("DerethPulse: Player and landblock tracking systems stopped");
        
        base.Stop();
    }

    private string GetPlayerJsonFilePath()
    {
        // Use the configured output path from settings
        var outputPath = Settings?.PlayerOutputPath ?? "DerethMaps-master";
        var outputFileName = Settings?.OutputFileName ?? "dynamicPlayers.json";
        
        // Determine if the path is absolute or relative
        string fullPath;
        if (Path.IsPathRooted(outputPath))
        {
            // Absolute path - use as-is
            fullPath = Path.Combine(outputPath, outputFileName);
        }
        else
        {
            // Relative path - make it relative to ACE server directory
            var aceServerDir = Directory.GetCurrentDirectory();
            fullPath = Path.Combine(aceServerDir, outputPath, outputFileName);
        }
        
        if (Settings?.EnableLogging == true)
        {
            ModManager.Log($"DerethPulse: Player output path: {outputPath}");
        }
        
        // Check if directory exists and log warning if it doesn't
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            if (Settings?.EnableLogging == true)
            {
                ModManager.Log($"DerethPulse: WARNING - Player output directory does not exist: {directory}");
                ModManager.Log($"DerethPulse: Player files may not be written. Please ensure the directory exists.");
            }
        }
        
        return fullPath;
    }

    private string GetLandblockJsonFilePath()
    {
        // Use the configured output path from settings
        var outputPath = Settings?.LandblockOutputPath ?? "DerethMaps-master";
        var outputFileName = Settings?.LandblockOutputFileName ?? "dynamicLandblocks.json";
        
        // Determine if the path is absolute or relative
        string fullPath;
        if (Path.IsPathRooted(outputPath))
        {
            // Absolute path - use as-is
            fullPath = Path.Combine(outputPath, outputFileName);
        }
        else
        {
            // Relative path - make it relative to ACE server directory
            var aceServerDir = Directory.GetCurrentDirectory();
            fullPath = Path.Combine(aceServerDir, outputPath, outputFileName);
        }
        
        if (Settings?.EnableLogging == true)
        {
            ModManager.Log($"DerethPulse: Landblock output path: {outputPath}");
        }
        
        // Check if directory exists and log warning if it doesn't
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            if (Settings?.EnableLogging == true)
            {
                ModManager.Log($"DerethPulse: WARNING - Landblock output directory does not exist: {directory}");
                ModManager.Log($"DerethPulse: Landblock files may not be written. Please ensure the directory exists.");
            }
        }
        
        return fullPath;
    }

    private void UpdatePlayerData(object? state)
    {
        try
        {
            if (Settings == null)
            {
                ModManager.Log("DerethPulse: ERROR - Settings not available for player tracking update");
                return;
            }

            var players = PlayerManager.GetAllOnline();
            var playerData = new List<PlayerRadarData>();
            var playerCount = 0;

            foreach (var player in players)
            {
                if (playerCount >= Settings.MaxPlayersToTrack)
                {
                    if (Settings.EnableLogging)
                        ModManager.Log($"DerethPulse: Reached maximum player limit ({Settings.MaxPlayersToTrack}), skipping remaining players");
                    break;
                }

                var playerRadarData = ExtractPlayerData(player);
                if (playerRadarData != null)
                {
                    playerData.Add(playerRadarData);
                    playerCount++;
                }
            }

            if (Settings.EnableLogging)
                ModManager.Log($"DerethPulse: Player scan detected {players.Count} active targets. Attempting to write {playerData.Count} players to: {playerJsonFilePath}");

            ExportPlayerDataToJson(playerData);
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Error during player scan - {ex.Message}");
        }
    }

    private void UpdateLandblockData(object? state)
    {
        try
        {
            if (Settings == null)
            {
                ModManager.Log("DerethPulse: ERROR - Settings not available for landblock tracking update");
                return;
            }

            var landblocks = LandblockManager.GetLoadedLandblocks();
            var landblockData = new List<LandblockRadarData>();

            foreach (var landblock in landblocks)
            {
                var landblockRadarData = ExtractLandblockData(landblock);
                if (landblockRadarData != null)
                {
                    landblockData.Add(landblockRadarData);
                }
            }

            if (Settings.EnableLogging)
                ModManager.Log($"DerethPulse: Landblock scan detected {landblocks.Count} loaded landblocks. Attempting to write {landblockData.Count} landblocks to: {landblockJsonFilePath}");

            ExportLandblockDataToJson(landblockData);
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Error during landblock scan - {ex.Message}");
        }
    }

    private PlayerRadarData? ExtractPlayerData(Player player)
    {
        try
        {
            // Get player name
            var playerName = player.Name ?? "Unknown";

            // Get player race from HeritageGroup
            var heritage = player.HeritageGroup;
            var race = heritage switch
            {
                HeritageGroup.Aluvian => "Aluvian",
                HeritageGroup.Sho => "Sho",
                HeritageGroup.Gharundim => "Gharu'ndim",
                HeritageGroup.Viamontian => "Viamontian",
                HeritageGroup.Shadowbound => "Shadowbound",
                HeritageGroup.Gearknight => "Gearknight",
                HeritageGroup.Tumerok => "Tumerok",
                HeritageGroup.Lugian => "Lugian",
                HeritageGroup.Empyrean => "Empyrean",
                HeritageGroup.Penumbraen => "Penumbraen",
                HeritageGroup.Undead => "Undead",
                HeritageGroup.Olthoi => "Olthoi",
                HeritageGroup.OlthoiAcid => "Olthoi",
                _ => "Unknown"
            };

            // Get player level
            var level = player.Level?.ToString() ?? "1";

            // Get map coordinates
            var mapCoords = player.Location.GetMapCoords();
            if (!mapCoords.HasValue)
            {
                // Player is indoors or in a dungeon - skip
                return null;
            }

            // Format coordinates as "16.9E", "22.9S" style
            var x = mapCoords.Value.X >= 0 ? $"{mapCoords.Value.X:F1}E" : $"{Math.Abs(mapCoords.Value.X):F1}W";
            var y = mapCoords.Value.Y >= 0 ? $"{mapCoords.Value.Y:F1}N" : $"{Math.Abs(mapCoords.Value.Y):F1}S";

            return new PlayerRadarData
            {
                Type = "Player",
                LocationName = playerName,
                Race = race,
                Level = level,
                X = x,
                Y = y
            };
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Error extracting data for player {player.Name} - {ex.Message}");
            return null;
        }
    }

    private LandblockRadarData? ExtractLandblockData(Landblock landblock)
    {
        try
        {
            // Get landblock status
            string status;
            if (landblock.IsDormant)
                status = "Dormant";
            else if (landblock.Permaload)
                status = "Permaload";
            else
                status = "Active";

            // Convert landblock coordinates to map coordinates
            // Landblock coordinates are 0-255 for X and Y
            var lbX = landblock.Id.LandblockX;
            var lbY = landblock.Id.LandblockY;

            // Convert to map coordinates (similar to coordsFromLandblock function in derethMaps.js)
            var xfract = lbX / 256.0;
            var yfract = (lbY + 1) / 256.0;
            var mapX = -101.9 + (102 - (-101.9)) * xfract;
            var mapY = -102 + (101.9 - (-102)) * (1 - yfract);

            // Format coordinates as hex strings (like in the existing dynamicLandblocks.json)
            var x = lbX.ToString("x2");
            var y = lbY.ToString("x2");

            return new LandblockRadarData
            {
                Type = "Landblock",
                Status = status,
                X = x,
                Y = y
            };
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Error extracting data for landblock {landblock.Id.Raw:X8} - {ex.Message}");
            return null;
        }
    }

    private void ExportPlayerDataToJson(List<PlayerRadarData> playerData)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(playerData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(playerJsonFilePath, jsonString);
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
            {
                ModManager.Log($"DerethPulse: Error exporting player data - {ex.Message}");
                ModManager.Log($"DerethPulse: File path was: {playerJsonFilePath}");
            }
        }
    }

    private void ExportLandblockDataToJson(List<LandblockRadarData> landblockData)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(landblockData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(landblockJsonFilePath, jsonString);
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Error exporting landblock data - {ex.Message}");
        }
    }
}

public class PlayerRadarData
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("LocationName")]
    public string LocationName { get; set; } = "";

    [JsonPropertyName("Race")]
    public string Race { get; set; } = "";

    [JsonPropertyName("Level")]
    public string Level { get; set; } = "";

    [JsonPropertyName("x")]
    public string X { get; set; } = "";

    [JsonPropertyName("y")]
    public string Y { get; set; } = "";
}

public class LandblockRadarData
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = "Landblock";

    [JsonPropertyName("Status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("x")]
    public string X { get; set; } = "";

    [JsonPropertyName("y")]
    public string Y { get; set; } = "";
}

public class Settings
{
    [JsonPropertyName("updateIntervalSeconds")]
    public int UpdateIntervalSeconds { get; set; } = 10;

    [JsonPropertyName("outputFileName")]
    public string OutputFileName { get; set; } = "dynamicPlayers.json";

    [JsonPropertyName("enableLogging")]
    public bool EnableLogging { get; set; } = false;

    [JsonPropertyName("maxPlayersToTrack")]
    public int MaxPlayersToTrack { get; set; } = 100;

    [JsonPropertyName("enableLandblockTracking")]
    public bool EnableLandblockTracking { get; set; } = true;

    [JsonPropertyName("landblockUpdateIntervalSeconds")]
    public int LandblockUpdateIntervalSeconds { get; set; } = 30;

    [JsonPropertyName("landblockOutputFileName")]
    public string LandblockOutputFileName { get; set; } = "dynamicLandblocks.json";

    [JsonPropertyName("playerOutputPath")]
    public string PlayerOutputPath { get; set; } = "DerethMaps-master";

    [JsonPropertyName("landblockOutputPath")]
    public string LandblockOutputPath { get; set; } = "DerethMaps-master";

    [JsonPropertyName("enablePlayerTracking")]
    public bool EnablePlayerTracking { get; set; } = true;
} 