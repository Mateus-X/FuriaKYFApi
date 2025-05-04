using Microsoft.AspNetCore.Mvc;
using FuriaKYFApi.Source.Models;
using FuriaKYFApi.Source.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using FuriaKYFApi.Source.Models.Dtos;

namespace FuriaKYFApi.Source.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FansController(
        AppDbContext context,
        IWebHostEnvironment env,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    ) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly IConfiguration _configuration = configuration;


        [HttpPost]
        public async Task<IActionResult> CreateFan([FromBody] FanCreateDto fanDto)
        {
            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                var fileExtension = Path.GetExtension(fanDto.Document.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Tipo de arquivo não permitido.");
                }

                if (!IsValidCpf(fanDto.CPF))
                    return BadRequest("Cpf inválido");

                var uploadsFolder = Path.Combine(_env.WebRootPath, "Storage");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fanDto.Document.CopyToAsync(fileStream);
                }

                var imageUrl = $"{Request.Scheme}://{Request.Host}/Storage/{uniqueFileName}";

                _httpClient.Timeout = TimeSpan.FromSeconds(3600);

                var endpoint = _configuration.GetValue<string>("Validation:BaseUrl") + "validate";

                var requestBody = new { image_path = imageUrl };
                var imageValidationPayload = new StringContent(
                    JsonSerializer.Serialize(requestBody, jsonSerializerOptions),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(endpoint, imageValidationPayload);

                var bodyResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = JsonSerializer.Deserialize<ErrorResponseDto>(bodyResponse);

                    throw new Exception("Erro ao validar o documento: " + errorMessage.Details);
                }

                var message = JsonSerializer.Deserialize<DocumentValidationDto>(bodyResponse);

                if (!message.OcrContents.SeemsValid)
                {
                    return StatusCode(500, "Análise indicou documento inválido. Tente tirar uma foto mais clara ou com melhor iluminação.");
                }

                if (message.OcrContents.DocumentNumber != fanDto.CPF && message.OcrContents.DocumentNumber != fanDto.RG) return StatusCode(500, "O Número do documento não confere com o número informado.");

                if (message.OcrContents.Name.ToLower() != fanDto.Name.ToLower()) return StatusCode(500, "O Nome do documento não confere com o nome informado.");

                var fan = new Fan
                {
                    Name = fanDto.Name,
                    Cpf = fanDto.CPF,
                    Rg = fanDto.RG,
                    Email = fanDto.Email,
                    Interests = fanDto.Interests.ToList(),
                    Events = fanDto.Events.ToList(),
                    AboutYou = fanDto.AboutYou,
                    Document = imageUrl
                };

                _context.Fans.Add(fan);

                await _context.SaveChangesAsync();

                return Ok(new { fan });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        private bool IsValidCpf(string cpf)
        {
            return !string.IsNullOrWhiteSpace(cpf) && cpf.Length == 11;
        }
    }
}