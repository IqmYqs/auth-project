using System;
namespace auth_dotnet_api.ViewModels
{
    public class RequestLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class RequestLoginOauth
    {
        public string UserId { get; set; }
    }
    public class ResponeGetTokenLine
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string id_token { get; set; }
    }
    public class ResponeGetProfileLine
    {
        public string userId { get; set; }
        public string displayName { get; set; }
        public string statusMessage { get; set; }
        public string pictureUrl { get; set; }
        public string email { get; set; }
    }
}