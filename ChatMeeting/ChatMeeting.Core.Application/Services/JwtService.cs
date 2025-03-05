using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Interfaces.Services;
using ChatMeeting.Core.Domain.Models;
using ChatMeeting.Core.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Application.Services
{
    public class JwtService : IJwtService
    {

        private readonly JwtSettingsOption _jwtSettingsOption;

        public JwtService(IOptions<JwtSettingsOption> jwtSettingOption)
        {
           _jwtSettingsOption = jwtSettingOption.Value;
        }

        public AuthDto GenerateJwtToken(User user)
        {
            var claims = GetClaims(user);

            var expireDate = DateTime.Now.AddMinutes( Convert.ToDouble(_jwtSettingsOption.ExpireInMinutes));

            var signingCredential = GetCredentials();

            var token = new JwtSecurityToken(
                claims: claims, 
                expires: expireDate,
                signingCredentials: signingCredential
                );

            return new AuthDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiredDate = expireDate,
            };
        }

        private SigningCredentials GetCredentials()
        {
            var byteSecurityKey = Encoding.ASCII.GetBytes(_jwtSettingsOption.SecretKey);
            var key = new SymmetricSecurityKey(byteSecurityKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return creds;
        }

        private Claim[] GetClaims(User user)
        {
            return new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
            };
        }
    }
}
