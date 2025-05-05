namespace FuriaKYFApi.Source.Models.Dtos
{
    public class FanCreateDto
    {
        public required string Name { get; set; }
        public required string DocumentNumber { get; set; }
        public required string Email { get; set; }
        public string[]? Interests { get; set; }
        public string[]? Events { get; set; }
        public string? AboutYou { get; set; }
        public required IFormFile Document { get; set; }
    }
}