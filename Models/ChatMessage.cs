using System;

namespace OllamaBlazorWasm.Models
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsProcessing { get; set; } = false;
    }
}