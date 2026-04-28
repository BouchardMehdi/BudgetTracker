using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using BudgetTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BudgetTrackerDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        BudgetTrackerDbContext context,
        PasswordHasher passwordHasher,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var username = dto.Username.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        var userExists = await _context.Users
            .AnyAsync(user => user.Username == username || user.Email == email);

        if (userExists)
        {
            return Conflict("Username or email already exists.");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        AddDefaultCategories(user.Id);
        await _context.SaveChangesAsync();

        return Ok(CreateAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var identifier = dto.Identifier.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.Email == identifier || item.Username.ToLower() == identifier);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials.");
        }

        return Ok(CreateAuthResponse(user));
    }

    private void AddDefaultCategories(int userId)
    {
        var categories = new[]
        {
            new Category { Name = "Salaire", Type = TransactionTypes.Income, UserId = userId },
            new Category { Name = "Freelance", Type = TransactionTypes.Income, UserId = userId },
            new Category { Name = "Courses", Type = TransactionTypes.Expense, UserId = userId },
            new Category { Name = "Logement", Type = TransactionTypes.Expense, UserId = userId },
            new Category { Name = "Transport", Type = TransactionTypes.Expense, UserId = userId },
            new Category { Name = "Loisirs", Type = TransactionTypes.Expense, UserId = userId }
        };

        _context.Categories.AddRange(categories);
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        return new AuthResponseDto
        {
            Token = _jwtTokenService.CreateToken(user),
            User = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            }
        };
    }
}
