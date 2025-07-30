using System;
namespace DerethPulse
{
    public class PlayerData
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; } = "Player";

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

        [JsonPropertyName("loc")]
        public string LOC { get; set; } = "";
    }
}
