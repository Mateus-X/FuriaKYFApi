using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("api/reddit")]
public class RedditAuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private const string ClientId = "SEU_CLIENT_ID";
    private const string ClientSecret = "SEU_CLIENT_SECRET";
    private const string RedirectUri = "https://suaapi.com/reddit-callback";

    public RedditAuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpGet("auth")]
    public IActionResult StartAuth()
    {
        var authUrl = $"https://www.reddit.com/api/v1/authorize?client_id={ClientId}" +
                      $"&response_type=code&redirect_uri={RedirectUri}" +
                      "&duration=temporary&scope=identity,history";

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
        request.Headers.Add("User-Agent", "furia-analytics/1.0");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"grant_type", "authorization_code"},
            {"code", code},
            {"redirect_uri", RedirectUri}
        });


        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

        var response = await _httpClient.SendAsync(request);
        var tokenData = await response.Content.ReadFromJsonAsync<RedditTokenResponse>();


        return Ok(new { tokenData.AccessToken });
    }


    [HttpGet("data")]
    public async Task<IActionResult> GetUserData([FromQuery] string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://oauth.reddit.com/api/v1/me");
        request.Headers.Add("User-Agent", "FURIA-Analytics/1.0");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        var userData = await response.Content.ReadFromJsonAsync<RedditUserResponse>();

        return Ok(userData);
    }
}


public class RedditTokenResponse
{
    public string AccessToken { get; set; }
}

public class RedditUserResponse
{
    public string Name { get; set; }
    public int TotalKarma { get; set; }
}