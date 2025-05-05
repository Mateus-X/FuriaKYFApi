using Azure.Core;
using FuriaKYFApi.Source.Data;
using FuriaKYFApi.Source.Models;
using FuriaKYFApi.Source.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/reddit")]
public class RedditAuthController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    AppDbContext dbContext,
    ILogger<RedditAuthController> logger
) : ControllerBase
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly ILogger<RedditAuthController> _logger = logger;



    [HttpGet("auth")]
    public IActionResult StartRedditAuth(int state)
    {
        var callbackUrl = Environment.GetEnvironmentVariable("REDDIT_REDIRECT_URI");
        var clientId = Environment.GetEnvironmentVariable("REDDIT_CLIENT_ID");

        _logger.LogWarning("auth");
        _logger.LogWarning(clientId + " " + callbackUrl);

        var authUrl = "https://www.reddit.com/api/v1/authorize?" +
                      $"client_id={clientId}&" +
                      "response_type=code&" +
                      $"redirect_uri={callbackUrl}&" +
                      "scope=identity,history&" +
                      $"state={state}";

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> RedditCallback([FromQuery] string code, [FromQuery] string state)
    {
        _logger.LogWarning("callback");
        _logger.LogWarning("chegou1 " + code +" state "+ state);
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return BadRequest("Code ou state ausente.");

        if (!int.TryParse(state, out var fanId))
            return BadRequest("State inválido.");

        var callbackUrl = Environment.GetEnvironmentVariable("REDDIT_REDIRECT_URI");
        var clientId = Environment.GetEnvironmentVariable("REDDIT_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("REDDIT_CLIENT_SECRET");

        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", callbackUrl),
            
        });

        var content2 = await content.ReadAsStringAsync();

        _logger.LogWarning(content2);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token")
        {
            Content = content
        };

        request.Headers.Add("User-Agent", "furia-analytics/1.0");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

        var response = await _httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode(500, "Erro ao obter token do Reddit: " + responseString);

        var tokenObj = System.Text.Json.JsonDocument.Parse(responseString).RootElement;
        var accessToken = tokenObj.GetProperty("access_token").GetString();

        if (string.IsNullOrEmpty(accessToken))
            return StatusCode(500, "Access token não encontrado na resposta do Reddit.");

        var fan = await dbContext.Fans.FindAsync(fanId);

        if (fan == null)
            return NotFound("Fan não encontrado.");

        fan.RedditAccessToken = accessToken;

        dbContext.Fans.Update(fan);

        await dbContext.SaveChangesAsync();

        return Ok("Autenticação Reddit concluída! Você pode fechar esta janela.");
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetUserData([FromQuery] int id)
    {
        var fan = await dbContext.Fans.FindAsync(id);

        if (fan == null)
        {
            var errorResponse = new ErrorResponseDto
            {
                detail = "Usuário associado a conta Reddit não encontrado",
            };

            return StatusCode(500, errorResponse);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "https://oauth.reddit.com/api/v1/me/");
        request.Headers.Add("User-Agent", "furia-analytics/1.0");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", fan.RedditAccessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var contentAwait = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, contentAwait);
        }

        var userData = await response.Content.ReadFromJsonAsync<JsonDocument>();

        if (userData == null)
        {
            var errorResponse = new ErrorResponseDto
            {
                detail = "Erro ao processar os dados do Reddit.",
            };

            return StatusCode(500, errorResponse);
        }

        var username = userData.RootElement.GetProperty("name").GetString();

        var requestComments = new HttpRequestMessage(HttpMethod.Get, $"https://oauth.reddit.com/user/{username}/comments");
        requestComments.Headers.Add("User-Agent", "furia-analytics/1.0");
        requestComments.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", fan.RedditAccessToken);


        var responseComments = await _httpClient.SendAsync(requestComments);

        if (!responseComments.IsSuccessStatusCode)
        {
            var contentAwait = await responseComments.Content.ReadAsStringAsync();

            return StatusCode((int)responseComments.StatusCode, contentAwait);
        }

        var commentsData = await responseComments.Content.ReadFromJsonAsync<JsonDocument>();

        if (commentsData == null)
        {
            var errorResponse = new ErrorResponseDto
            {
                detail = "Usuário associado a conta Reddit não encontrado",
            };

            return StatusCode(500, errorResponse);
        }

        var comments = commentsData.RootElement.GetProperty("data").GetProperty("children");
        int furiaMentionsCount = 0;

        foreach (var comment in comments.EnumerateArray())
        {
            var body = comment.GetProperty("data").GetProperty("body").GetString();

            if (body != null && body.Contains("Furia", StringComparison.OrdinalIgnoreCase))
            {
                furiaMentionsCount++;
            }
        }

        return Ok(new
        {
            access_token = fan.RedditAccessToken,
            furia_mentions = furiaMentionsCount
        });
    }
}