using Microsoft.IdentityModel.Tokens;
using RealTime_APIDev.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RealTime_APIDev.JWT
{
    public class MissingFields : Exception
    {
        public MissingFields(string msg) : base(msg) { }

    }

    public class GenerateToken
    {
        private readonly IConfiguration _config;
        public GenerateToken(IConfiguration config)
        {
            this._config = config;
        }

        public string generateToken(EmployeeDTO employeeDTO)
        {
            try
            {
                var key = _config["jwt:key"];
                var audience = _config["jwt:audience"];
                var issuer = _config["jwt:issuer"];

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(issuer))
                {
                    throw new MissingFieldException("Some Fields in the appSettings.json are missing or Null");
                }
                List<Claim> claims = new List<Claim>
            {
                 new Claim(ClaimTypes.Name, employeeDTO.EmployeeName),
                 new Claim(ClaimTypes.Role, employeeDTO.EmployeeRole)
            };
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credential = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: credential
                    );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                return $"Token generation fails " + ex.Message;
            }
        }
    }
}
