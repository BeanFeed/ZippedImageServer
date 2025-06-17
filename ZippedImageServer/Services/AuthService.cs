using System.Security.Claims;
using DAL.Context;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using ZippedImageServer.Models;

namespace ZippedImageServer.Services;

public class AuthService(ServerContext context, IConfiguration config)
{
    public async Task<string> Login(LoginModel loginModel)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Username == loginModel.Username);
        
        if (user == null)
        {
            throw new Exception("User not found or password is incorrect.");
        }
        
        if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash))
        {
            throw new Exception("User not found or password is incorrect.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Username),
            new Claim(ClaimTypes.Role, "Administrator"),
        };
        
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(config["Jwt:Key"])),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        
        return tokenString;
    }
}