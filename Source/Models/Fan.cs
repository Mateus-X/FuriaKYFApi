using System.ComponentModel.DataAnnotations;

namespace FuriaKYFApi.Source.Models
{
    public class Fan
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string DocumentNumber { get; set; }
        public required string Email { get; set; }
        public List<string>? Interests { get; set; } = [];
        public List<string>? Events { get; set; } = [];
        public string? AboutYou { get; set; }
        public required string Document { get; set; }
        public string? RedditAccessToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}