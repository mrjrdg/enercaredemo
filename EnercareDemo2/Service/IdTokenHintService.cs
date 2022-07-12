using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EnercareDemo2.Service
{
    public class IdTokenHintService
    {
        private readonly IConfiguration _configuration;
        private readonly TokenSigningCertificateService _tokenSigningCertificateService;

        public IdTokenHintService(IConfiguration configuration, TokenSigningCertificateService tokenSigningCertificateManager)
        {
            _configuration = configuration;
            _tokenSigningCertificateService = tokenSigningCertificateManager;
        }

        public string BuildIdToken(string userEmail)
        {
            string audience = _configuration
                .GetSection("IdTokenHintBuilderConfiguration")["Audience"];

            double.TryParse(_configuration
                .GetSection("IdTokenHintBuilderConfiguration")["TokenLifeTimeInMinutes"],
                    out double tokenExpirationTime);

            string issuer = _configuration
                .GetSection("IdTokenHintBuilderConfiguration")["Issuer"];

            // All parameters send to Azure AD B2C needs to be sent as claims:
            var claims = new List<Claim>
            {
                new Claim("email", userEmail, ClaimValueTypes.String, issuer),
            };

            var signingCredential = _tokenSigningCertificateService
                .GetSigningCredentials();

            var token = new JwtSecurityToken
            (
                issuer,
                audience,
                claims,
                DateTime.Now,
                DateTime.Now.AddMinutes(tokenExpirationTime),
                signingCredential
            );

            JwtSecurityTokenHandler jwtHandler =
                new JwtSecurityTokenHandler();

            return jwtHandler.WriteToken(token);
        }

        public string BuildIdTokenForSignup(string userEmail, string contactId)
        {
            string audience = _configuration
                .GetSection("IdTokenHintBuilderConfiguration")["Audience"];

            double.TryParse(_configuration
                .GetSection("IdTokenHintBuilderConfiguration")["TokenLifeTimeInMinutes"],
                    out double tokenExpirationTime);

            string issuer = _configuration
                .GetSection("IdTokenHintBuilderConfiguration")["Issuer"];

            // All parameters send to Azure AD B2C needs to be sent as claims:
            var claims = new List<Claim>
            {
                new Claim("email", userEmail, ClaimValueTypes.String, issuer),
                new Claim("contact_id", contactId, ClaimValueTypes.String, issuer),
            };

            var signingCredential = _tokenSigningCertificateService
                .GetSigningCredentials();

            var token = new JwtSecurityToken
            (
                issuer,
                audience,
                claims,
                DateTime.Now,
                DateTime.Now.AddMinutes(tokenExpirationTime),
                signingCredential
            );

            JwtSecurityTokenHandler jwtHandler =
                new JwtSecurityTokenHandler();

            return jwtHandler.WriteToken(token);
        }
    }
}
