using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using auth_dotnet_api.ViewModels;
using auth_dotnet_api.Repositories;
using System.Net.Http.Headers;
using System.Text.Json;
using auth_dotnet_api.Models;
using auth_dotnet_api.Enum;

namespace auth_dotnet_api.Controllers;

[ApiController]
[Route("api/[action]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly UserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public LoginController(ILogger<LoginController> logger, UserRepository userRepository, IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _logger = logger;
        _userRepository = userRepository;
        _httpClient = httpClient;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] RequestLogin request)
    {
        var user = await _userRepository.GetUserByLoginAsync(request);

        if (user == null)
        {
            _logger.LogWarning("Login failed for user: {Username}", request.Username);
            return Unauthorized("Invalid credentials");
        }

        // Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Firstname", user.Username),
                new Claim("Lastname", user.Lastname),
                new Claim("Username", user.Username),
            }),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"])),
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return Ok(new
        {
            token = jwt,
            expiresIn = Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"]),
            user = new
            {
                user.Id,
                user.Email,
                user.Username,
                user.Role,
                user.Lastname,
                user.Firstname
            }
        });
    }
    [HttpPost]
    public async Task<string> LoginOauth(ResponeGetProfileLine request)
    {
        var user = await _userRepository.GetUserByLoginOauthAsync(request.userId);

        var createUser = new User();
        if (user == null)
        {
            createUser.Username = request.displayName;
            createUser.Firstname = request.displayName.Split(" ")[0];
            createUser.Lastname = request.displayName.Split(" ")[1];
            createUser.Email = request.email;
            createUser.Role = RoleType.User;
            createUser.UserID = request.userId;
            createUser.Password = "1234";
            _userRepository.AddUserAsync(createUser);
            user = createUser;
        }


        // Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Firstname", user.Username),
                new Claim("Lastname", user.Lastname),
                new Claim("Username", user.Username),
            }),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"])),
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        var url = $"{_config["FrontendUrl"]}?" +
              $"token={Uri.EscapeDataString(jwt)}" +
              $"&expiresIn={_config["JwtSettings:ExpiryMinutes"]}" +
              $"&id={Uri.EscapeDataString(user.Id.ToString())}" +
              $"&email={user.Email ?? "test@gmail.com"}" +
              $"&username={Uri.EscapeDataString(user.Username)}" +
              $"&role={user.Role}" +
              $"&lastname={Uri.EscapeDataString(user.Lastname)}" +
              $"&firstname={Uri.EscapeDataString(user.Firstname)}";

        return url;
    }
    [HttpGet]
    public IActionResult CheckToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token is required");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // Validate the token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured"))),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config["JwtSettings:Issuer"],
                ValidAudience = _config["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No clock skew for simplicity
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return Ok(new { IsValid = true, Claims = principal.Claims.Select(c => new { c.Type, c.Value }) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return Unauthorized("Invalid token");
        }
    }
    [HttpGet]
    public async Task<IActionResult> LineCallbackLogin()
    {
        string code = Request.Query["code"];

        if (code == null) return BadRequest();

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.line.me/oauth2/v2.1/token");

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _config["Linelogin:RedirectUri"]),
            new KeyValuePair<string, string>("client_id", _config["Linelogin:ClientId"]),
            new KeyValuePair<string, string>("client_secret", _config["Linelogin:ClientSecret"]),
        };

        request.Content = new FormUrlEncodedContent(formData);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode) return BadRequest();

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseContentDeserialize = JsonSerializer.Deserialize<ResponeGetTokenLine>(responseContent);


        var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.line.me/v2/profile");
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseContentDeserialize?.access_token);
        var profileRequestResponse = await _httpClient.SendAsync(profileRequest);
        var responseProfile = await profileRequestResponse.Content.ReadAsStringAsync();
        var responseProfileDeserialize = JsonSerializer.Deserialize<ResponeGetProfileLine>(responseProfile);

        var url = "";
        if (responseProfileDeserialize.userId != null)
        {
            url = await LoginOauth(responseProfileDeserialize);
            return Redirect(url);
        }

        return Redirect(_config["FrontendUrl"]);
    }
    [HttpGet]
    public async Task<IActionResult> MsADCallbackLogin()
    {
        string code = Request.Query["code"];

        if (code == null) return BadRequest();

        var request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/common/oauth2/v2.0/token");

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _config["MSlogin:RedirectUri"]),
            new KeyValuePair<string, string>("client_id", _config["MSlogin:ClientId"]),
            new KeyValuePair<string, string>("client_secret", _config["MSlogin:ClientSecret"]),
        };

        request.Content = new FormUrlEncodedContent(formData);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode) return BadRequest();

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseContentDeserialize = JsonSerializer.Deserialize<ResponeGetTokenLine>(responseContent);


        var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseContentDeserialize?.access_token);
        var profileRequestResponse = await _httpClient.SendAsync(profileRequest);

        if (!profileRequestResponse.IsSuccessStatusCode) return BadRequest();

        var responseProfile = await profileRequestResponse.Content.ReadAsStringAsync();
        var responseProfileDeserialize = JsonSerializer.Deserialize<ResponeGetProfileLine>(responseProfile);

        var url = "";
        if (responseProfileDeserialize.userId != null)
        {
            url = await LoginOauth(responseProfileDeserialize);
            return Redirect(url);
        }

        return Redirect(_config["FrontendUrl"]);
    }

    [Authorize(Roles = "User")]
    [HttpGet]
    public IActionResult GetDataUser()
    {

        return Ok(new { Data = "User" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult GetDataAdmin()
    {

        return Ok(new { Data = "Admin" });
    }

    [Authorize(Roles = "User,Admin")]
    [HttpGet]
    public IActionResult GetDataAny()
    {

        return Ok(new { Data = "Any" });
    }
}

