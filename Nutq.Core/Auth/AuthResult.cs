namespace Nutq.Core.Auth
{
    public class AuthResult
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = null!;

        public AuthResult(string token, DateTime expires, int userId, string email)
        {
            Token = token;
            Expires = expires;
            UserId = userId;
            Email = email;
        }
    }
}
