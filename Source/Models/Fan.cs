namespace FuriaKYFApi.Source.Models
{
    public class Fan
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Cpf { get; set; }
        public string? Rg { get; set; }
        public required string Email { get; set; }
        public required List<string> Interests { get; set; }
        public required List<string> Events { get; set; }
        public string? AboutYou { get; set; }
        public required string Document { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}