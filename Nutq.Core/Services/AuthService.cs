using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Nutq.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;

        // مفتاح JWT طويل لتجنب مشاكل الطول
        private readonly string _jwtSecret = "ThisIsAVeryLongSecretKeyForNutq1234567890!@#";

        public AuthService(IDoctorRepository doctorRepository, IPatientRepository patientRepository)
        {
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        public async Task<AuthResult?> LoginDoctorAsync(string email, string password)
        {
            var doctor = await _doctorRepository.GetByEmailAsync(email);
            if (doctor == null || doctor.Password != password) return null;

            var token = GenerateJwt(doctor.Id, doctor.Email);
            return new AuthResult(token, DateTime.UtcNow.AddHours(1), doctor.Id, doctor.Email);
        }

        public async Task<AuthResult?> LoginPatientAsync(string email, string password)
        {
            var patient = await _patientRepository.GetByEmailAsync(email);
            if (patient == null || patient.Password != password) return null;

            var token = GenerateJwt(patient.Id, patient.Email);
            return new AuthResult(token, DateTime.UtcNow.AddHours(1), patient.Id, patient.Email);
        }

        private string GenerateJwt(int userId, string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim("id", userId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
