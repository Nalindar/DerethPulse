namespace DerethPulse;

[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    private Timer? radarTimer;
    private string jsonFilePath = string.Empty;

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
                ModManager.Log("DerethPulse: Initializing player tracking system...");
            
            // Set up JSON file path
            jsonFilePath = GetJsonFilePath();
            
            // Start radar timer - update based on configuration
            var interval = TimeSpan.FromSeconds(Settings.UpdateIntervalSeconds);
            radarTimer = new Timer(UpdateRadarData, null, TimeSpan.Zero, interval);
            
            if (Settings.EnableLogging)
                ModManager.Log($"DerethPulse: Player tracking system online and scanning every {Settings.UpdateIntervalSeconds} seconds");
        }
        catch (Exception ex)
        {
            ModManager.Log($"DerethPulse: ERROR during initialization - {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    public override void Stop()
    {
        base.Stop();
        
        radarTimer?.Dispose();
        if (Settings?.EnableLogging == true)
            ModManager.Log("DerethPulse: Player tracking system stopped");
    }

    private string GetJsonFilePath()
    {
        // Use the configured output path from settings
        var outputPath = Settings?.OutputPath ?? "DerethMaps-master";
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
        
        // Check if directory exists
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            ModManager.Log($"DerethPulse: ERROR - Output directory does not exist: {directory}");
            ModManager.Log($"DerethPulse: Please update your Settings.json to use an existing path");
            return string.Empty; // Return empty string to indicate error
        }
        
        return fullPath;
    }

    private void UpdateRadarData(object? state)
    {
        try
        {
            if (Settings == null)
            {
                ModManager.Log("DerethPulse: ERROR - Settings not available for player tracking update");
                return;
            }

            var players = PlayerManager.GetAllOnline();
            if (Settings.EnableLogging)
                ModManager.Log($"DerethPulse: Player scan detected {players.Count} active targets");

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

                var radarData = ExtractPlayerData(player);
                if (radarData != null)
                {
                    playerData.Add(radarData);
                    playerCount++;
                }
            }

            ExportToJson(playerData);
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Error during player scan - {ex.Message}");
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

    private void ExportToJson(List<PlayerRadarData> playerData)
    {
        try
        {
            // Check if we have a valid file path
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                ModManager.Log("DerethPulse: ERROR - Cannot export data: Invalid output path configured");
                return;
            }

            var jsonString = JsonSerializer.Serialize(playerData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(jsonFilePath, jsonString);
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Exported {playerData.Count} player positions to DerethMaps at: {jsonFilePath}");
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                ModManager.Log($"DerethPulse: Error exporting player data - {ex.Message}");
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

    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = "DerethMaps-master";
} 