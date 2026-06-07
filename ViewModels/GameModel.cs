using System.Text.Json.Serialization;

namespace ATC4_HQ.ViewModels
{
    public class GameModel
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("ArchivePath")]
        public string ArchivePath { get; set; } = string.Empty;
    }
}