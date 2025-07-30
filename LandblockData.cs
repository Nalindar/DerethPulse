namespace DerethPulse
{
    public class LandblockData
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; } = "Landblock";

        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("Status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("x")]
        public string X { get; set; } = "";

        [JsonPropertyName("y")]
        public string Y { get; set; } = "";

        [JsonPropertyName("isDungeon")]
        public bool IsDungeon { get; set; } = false;

        [JsonPropertyName("hasDungeon")]
        public bool HasDungeon { get; set; } = false;

        [JsonPropertyName("playerCount")]
        public int PlayerCount { get; set; } = 0;

        [JsonPropertyName("creatureCount")]
        public int CreatureCount { get; set; } = 0;
    }
}
