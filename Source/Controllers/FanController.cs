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
        IConfiguration configuration,
        ILogger<FansController> logger
    ) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly ILogger<FansController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> CreateFan([FromForm] FanCreateDto fanDto)
        {
            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
            string? filePath = null; 


            try
            {
                _logger.LogInformation(JsonSerializer.Serialize(fanDto, jsonSerializerOptions));
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (fanDto.Document == null || fanDto.Document.Length == 0)
                {
                    return BadRequest("O arquivo do documento é obrigatório.");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(fanDto.Document.FileName).ToLowerInvariant();
                _logger.LogInformation(fileExtension);


                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Tipo de arquivo não permitido.");
                }

                _logger.LogWarning("chegou aq");
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "Storage");
                _logger.LogWarning("chegou aq2");

                _logger.LogInformation(uploadsFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid() + fileExtension;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fanDto.Document.CopyToAsync(fileStream);
                }

                var imageUrl = $"{Request.Scheme}://{Request.Host}/Storage/{uniqueFileName}";

                _httpClient.Timeout = TimeSpan.FromSeconds(3600);

                var endpoint = Environment.GetEnvironmentVariable("VALIDATION_BASE_URL") + "/validate";

                var requestBody = new { image_path = imageUrl };
                var imageValidationPayload = new StringContent(
                    JsonSerializer.Serialize(requestBody, jsonSerializerOptions),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var responseValidation = await _httpClient.PostAsync(endpoint, imageValidationPayload);

                var bodyResponse = await responseValidation.Content.ReadAsStringAsync();

                if (!responseValidation.IsSuccessStatusCode)
                {
                    var errorMessage = JsonSerializer.Deserialize<ErrorResponseDto>(bodyResponse);

                    throw new Exception(errorMessage.detail);
                }

                var message = JsonSerializer.Deserialize<DocumentValidationDto>(bodyResponse);

                if (!message.OcrContents.SeemsValid)
                {
                    DocumentPurge(filePath);

                    var errorResponse = new ErrorResponseDto
                    {
                        detail = "Análise indicou documento inválido. Tente tirar uma foto mais clara ou com melhor iluminação.",
                    };

                    return StatusCode(500, errorResponse);
                }

                if (message.OcrContents.DocumentNumber != fanDto.DocumentNumber)
                {
                    DocumentPurge(filePath);

                    var errorResponse = new ErrorResponseDto
                    {
                        detail = "O Número do documento não confere com o número informado.",
                    };

                    return StatusCode(500, errorResponse);
                };

                if (message.OcrContents.Name.ToLower() != fanDto.Name.ToLower())
                {
                    DocumentPurge(filePath);

                    var errorResponse = new ErrorResponseDto
                    {
                        detail = "O Nome do documento não confere com o nome informado.",
                    };

                    return StatusCode(500, errorResponse);
                }

                var fan = new Fan
                {
                    Name = fanDto.Name,
                    DocumentNumber = fanDto.DocumentNumber,
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
            catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("duplicate key") ?? false)
            {
                if (!string.IsNullOrEmpty(filePath)) DocumentPurge(filePath);

                var errorResponse = new ErrorResponseDto
                {
                    detail = "Já existe um registro com os mesmos dados. Verifique as informações e tente novamente.",
                };

                return StatusCode(400, errorResponse);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(filePath)) DocumentPurge(filePath);

                var errorResponse = new ErrorResponseDto
                {
                    detail = ex.Message,
                };

                return StatusCode(500, errorResponse);
            }
        }

        private void DocumentPurge(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

    }
}