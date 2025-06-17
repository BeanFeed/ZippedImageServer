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
            Issuer = config["Auth:Authority"],
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(config["Auth:KeySecret"])),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256)
        };
        
        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return token;
    }
}