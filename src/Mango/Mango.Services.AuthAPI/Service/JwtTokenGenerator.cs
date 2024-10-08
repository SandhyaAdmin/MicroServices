﻿using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mango.Services.AuthAPI.Service
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JWTOptions _jwtOptions;
        public JwtTokenGenerator(IOptions<JWTOptions> jwtOptions) 
        {
            // Configured JwtOptions in program.cs which are required to generate the token, and retrive them in constructor using dependency injection
            _jwtOptions = jwtOptions.Value;

        }
        public string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles)
        {
            // Generate the token based on application user using JWTSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtOptions.secret);

            //Inside token what we typically have is claim like we want to store emailid, username or soemthing else in token
            // We can store all of them in a key value pair by something know as claim in token.
            // JWT Token consists of multiple claims    

            var claimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, applicationUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id),
                new Claim(JwtRegisteredClaimNames.Name, applicationUser.UserName)
            };

            // we are adding ClaimTypes.Role(dotnet type) as in the project we are accessing/checking if user has a role,
            //it automatically do that mapping.
            claimList.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // We need tokenDesctiptor along with key and claim that have some configuration properties of a token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.UtcNow.AddDays(360)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            //WriteToken : write and return thr final token that you needed
            return tokenHandler.WriteToken(token);  
        }
    }
}
