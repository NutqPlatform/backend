namespace Nutq.Core.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
        public string Role { get; set; } = "User";
        public string Message { get; set; } = null!;

        public AuthResult() { }

        public AuthResult(string token, DateTime expires, int userId, string email)
        {
            Token = token;
            Expires = expires;
            UserId = userId;
            Email = email;
            Success = true;
        }
    }
}
