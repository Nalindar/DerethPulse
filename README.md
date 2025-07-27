# DerethPulse

A player tracking system for ACE servers that monitors online player positions and exports them to DerethMaps for real-time map visualization.

## Features

- **Real-time Player Tracking**: Monitors all online players and their positions
- **Map Integration**: Exports player data to DerethMaps for visualization
- **Configurable**: Customizable update intervals, output paths, and logging

![Demo](https://imgur.com/a/o8Q0q6k)

## Installation

1. **Prerequisites**
   - ACE server installation
   - .NET 8.0 runtime
   - DerethMaps (for map visualization)

2. **Installation Steps**
   - Download the latest release zip file from the releases page
   - Extract the zip file to your ACE server's `Mods` directory
   - Ensure the extracted folder is named `DerethPulse`
   - Ensure the plugin is enabled in your ACE server configuration
   - Restart your ACE server



## Configuration

Edit `Settings.json` to customize the plugin behavior:

```json
{
  "updateIntervalSeconds": 10,
  "maxPlayersToTrack": 100,
  "outputFileName": "dynamicPlayers.json",
  "outputPath": "DerethMaps-master",
  "enableLogging": false
}
```

### Configuration Options

- **updateIntervalSeconds**: How often to update player positions (default: 10)
- **maxPlayersToTrack**: Maximum number of players to track (default: 100)
- **outputFileName**: Name of the output JSON file (default: "dynamicPlayers.json")
- **outputPath**: Directory to write the output file (supports absolute and relative paths)
- **enableLogging**: Enable/disable log output (default: false)

### Path Configuration

**Relative Path** (relative to ACE server directory):
```json
"outputPath": "DerethMaps-master"
```

**Absolute Path**:
```json
"outputPath": "C:\\MyMaps\\DerethMaps"
```

## Usage

1. **Start the Plugin**
   - The plugin automatically starts when the ACE server starts
   - Check server logs for initialization messages

2. **Monitor Output**
   - Player data is written to the configured output file
   - File is updated every `updateIntervalSeconds` seconds
   - Check server logs for activity and error messages

3. **Integration with DerethMaps**
   - Place the output file in your DerethMaps directory
   - DerethMaps will automatically display player positions
   - Players appear as markers on the map in real-time

## Output Format

The plugin generates a JSON file with player data:

```json
[
  {
    "Type": "Player",
    "LocationName": "PlayerName",
    "Race": "Aluvian",
    "Level": "50",
    "x": "16.9E",
    "y": "22.9S"
  }
]
```

## Troubleshooting

**Plugin not loading:**
- Check that the plugin is in the correct Mods directory
- Verify .NET 8.0 is installed
- Check server logs for error messages

**Settings not loading:**
- Ensure Settings.json is properly formatted
- Check for JSON syntax errors
- Verify file permissions

**No output file:**
- Check the configured output path exists
- Verify write permissions to the output directory
- Check server logs for path-related errors

**Performance issues:**
- Reduce `updateIntervalSeconds` for less frequent updates
- Lower `maxPlayersToTrack` to limit tracking
- Disable logging by setting `enableLogging` to false

## Dependencies

- ACE Server (with mod support)
- .NET 8.0 Runtime
- ACE.Shared library
- Lib.Harmony (for patching)

 