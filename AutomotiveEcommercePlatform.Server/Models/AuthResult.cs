namespace AutomotiveEcommercePlatform.Server.Models
{
    public class AuthResult
    {
        public string Token { get; set; }
        public bool Result { get; set; }
        public string  Role { get; set; } = string.Empty;
        public List<string>Errors { get; set; }
    }
}
