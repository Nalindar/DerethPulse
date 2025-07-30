namespace DerethPulse;

[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    private Timer? playerDataTimer;
    private Timer? landblockDataTimer;

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override void Init()
    {
        base.Init();
    }

    //public override Task OnStartSuccess()
    //{
    //    Settings = SettingsContainer?.Settings ?? new();
    //    StartServices();

    //    return Task.CompletedTask;
    //}

    public override Task OnWorldOpen()
    {
        Settings = SettingsContainer?.Settings ?? new();
        StartServices();

        return Task.CompletedTask;
    }

    private void StartServices()
    {
        try
        {
            if (Settings.EnablePlayerDataOutput)
            {
                Mod.Log("Initializing player tracking system...");

                var interval = TimeSpan.FromSeconds(Settings.PlayerDataUpdateIntervalSeconds);
                playerDataTimer = new Timer(UpdatePlayerData, null, TimeSpan.Zero, interval);

                Mod.Log($"Player activity tracking system online and scanning every {interval.TotalSeconds} seconds");
            }
            else
            {
                playerDataTimer = null;
            }

            if (Settings.EnableLandblockDataOutput)
            {
                Mod.Log("Initializing landblock activity tracking system...");

                var interval = TimeSpan.FromSeconds(Settings.LandblockDataUpdateIntervalSeconds);
                landblockDataTimer = new Timer(UpdateLandblockData, null, TimeSpan.Zero, interval);

                Mod.Log($"Landblock activity tracking system online and scanning every {interval.TotalSeconds} seconds");
            }
            else
            {
                landblockDataTimer = null;
            }
        }
        catch (Exception ex)
        {
            Mod.Log($"ERROR during initialization - {ex.Message}", ModManager.LogLevel.Error);
        }
    }

    protected override void SettingsChanged(object? sender, EventArgs e)
    {
        base.SettingsChanged(sender, e);
        Settings = SettingsContainer?.Settings ?? new();
        StartServices();
    }

    public override void Stop()
    {
        base.Stop();
        
        playerDataTimer?.Dispose();

        landblockDataTimer?.Dispose();

        Mod.Log("Player and Landblock activity tracking system stopped");
    }

    private string GetJsonFilePath(string outputPath, string outputFileName)
    {
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
            Mod.Log($"ERROR - Output directory does not exist: {directory}", ModManager.LogLevel.Error);
            Mod.Log($"Please update your Settings.json to use an existing path", ModManager.LogLevel.Error);
            return string.Empty; // Return empty string to indicate error
        }

        return fullPath;
    }

    private void UpdatePlayerData(object? state)
    {
        try
        {
            if (Settings == null)
            {
                Mod.Log("ERROR - Settings not available for player activity update", ModManager.LogLevel.Error);
                return;
            }

            var players = PlayerManager.GetAllOnline();
            if (Settings.EnableLogging)
                Mod.Log($"Player scan detected {players.Count} online player{(players.Count > 1 || players.Count == 0 ? "s" : "")}");

            var playerData = new List<PlayerData>();
            var playerCount = 0;

            foreach (var player in players)
            {
                if (playerCount >= Settings.PlayerDataMaxPlayersToOutput)
                {
                    if (Settings.EnableLogging)
                        Mod.Log($"Reached maximum player limit ({Settings.PlayerDataMaxPlayersToOutput}), skipping remaining players");
                    break;
                }

                var radarData = ExtractPlayerData(player);
                if (radarData != null)
                {
                    playerData.Add(radarData);
                    playerCount++;
                }
            }

            if (Settings.EnableLogging)
            {
                var dungeonPlayers = players.Count - playerData.Count;
                Mod.Log($"Player scan detected {dungeonPlayers} player{(dungeonPlayers > 1 || dungeonPlayers == 0 ? "s" : "")} indoors or in dungeons and skipped them");
            }

            ExportToJson(GetJsonFilePath(Settings.PlayerDataOutputPath, Settings.PlayerDataOutputFileName), "player", playerData);
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                Mod.Log($"Error during player scan - {ex.Message}", ModManager.LogLevel.Warn);
        }
    }

    private PlayerData? ExtractPlayerData(Player player)
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

            // Get LOC data for better precision
            var loc = player.Location.ToLOCString();

            return new PlayerData
            {
                //Type = "Player",
                LocationName = playerName,
                Race = race,
                Level = level,
                X = x,
                Y = y,
                LOC = loc
            };
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                Mod.Log($"Error extracting data for player {player.Name} - {ex.Message}", ModManager.LogLevel.Warn);
            return null;
        }
    }

    private void UpdateLandblockData(object? state)
    {
        try
        {
            if (Settings == null)
            {
                Mod.Log("ERROR - Settings not available for landblock activity update", ModManager.LogLevel.Error);
                return;
            }

            var landblocks = LandblockManager.GetLoadedLandblocks();
            if (Settings.EnableLogging)
                Mod.Log($"Landblock scan detected {landblocks.Count} loaded landblock{(landblocks.Count > 1 || landblocks.Count == 0 ? "s" : "")}");

            var landblockData = new List<LandblockData>();

            foreach (var landblock in landblocks)
            {
                var radarData = ExtractLandblockData(landblock);
                if (radarData != null)
                {
                    landblockData.Add(radarData);
                }
            }

            ExportToJson(GetJsonFilePath(Settings.LandblockDataOutputPath, Settings.LandblockDataOutputFileName), "landblock", landblockData);
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                Mod.Log($"Error during landblock scan - {ex.Message}", ModManager.LogLevel.Warn);
        }
    }

    private LandblockData? ExtractLandblockData(Landblock landblock)
    {
        try
        {
            // Get landblock id
            var id = "0x" + landblock.Id.ToString()[0..4];

            // Get landblock status
            var status = "Active";
            if (landblock.Permaload)
                status = "Permaload";
            else if (landblock.IsDormant)
                status = "Dormant";

            // Get Landblock X / Y
            var x = landblock.Id.LandblockX.ToString("x2");
            var y = landblock.Id.LandblockY.ToString("x2");

            // Get Landblock dungeon status
            var isDungeon = landblock.IsDungeon;
            var hasDungeon = landblock.HasDungeon;

            // Get Landblock Player and Creature counts
            var playerCount = landblock.GetPlayers().Count;
            var creatureCount = landblock.GetCreatures().Count;

            return new LandblockData
            {
                Id = id,
                Status = status,
                X = x,
                Y = y,
                IsDungeon = isDungeon,
                HasDungeon = hasDungeon,
                PlayerCount = playerCount,
                CreatureCount = creatureCount,
            };
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                Mod.Log($"Error extracting data for landblock {landblock.Id} - {ex.Message}", ModManager.LogLevel.Warn);
            return null;
        }
    }

    private void ExportToJson<T>(string jsonFilePath, string type, List<T> data)
    {
        try
        {
            // Check if we have a valid file path
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                Mod.Log("ERROR - Cannot export data: Invalid output path configured", ModManager.LogLevel.Error);
                return;
            }

            var jsonString = JsonSerializer.Serialize(data, jsonSerializerOptions);

            File.WriteAllText(jsonFilePath, jsonString);
            if (Settings?.EnableLogging == true)
                Mod.Log($"Exported activity data for {data.Count} {type}{(data.Count > 1 || data.Count == 0 ? "s" : "")} to {jsonFilePath}");
        }
        catch (Exception ex)
        {
            if (Settings?.EnableLogging == true)
                Mod.Log($"Error exporting {type} data - {ex.Message}", ModManager.LogLevel.Error);
        }
    }
}
