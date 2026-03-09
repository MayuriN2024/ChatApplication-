using System.ComponentModel.DataAnnotations;

namespace PingMe.Server.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public string? Content { get; set; }

        [Required]
        public required string Sender { get; set; }

        public string? Recipient { get; set; }

        public string? Color { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public MessageType Type { get; set; }
    }

    public enum MessageType
    {
        CHAT, 
        PRIVATE_MESSAGE, 
        JOIN, 
        LEAVE, 
        TYPING
    }
}
