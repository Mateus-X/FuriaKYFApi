namespace FuriaKYFApi.Source.Models.Dtos
{
    public class FanCreateDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? CPF { get; set; }
        public string? RG { get; set; }
        public required string Email { get; set; }
        public required string[] Interests { get; set; }
        public required string[] Events { get; set; }
        public string? AboutYou { get; set; }
        public required IFormFile Document { get; set; }
    }
}