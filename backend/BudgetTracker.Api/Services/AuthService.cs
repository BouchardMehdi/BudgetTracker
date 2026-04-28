using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Services;

public class AuthService
{
    private readonly BudgetTrackerDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(
        BudgetTrackerDbContext context,
        PasswordHasher passwordHasher,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var username = dto.Username.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        var userExists = await _context.Users
            .AnyAsync(user => user.Username == username || user.Email == email);

        if (userExists)
        {
            return ServiceResult<AuthResponseDto>.Failure(
                "user_already_exists",
                "Username or email already exists.",
                StatusCodes.Status409Conflict);
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

        return ServiceResult<AuthResponseDto>.Success(CreateAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var identifier = dto.Identifier.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.Email == identifier || item.Username.ToLower() == identifier);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            return ServiceResult<AuthResponseDto>.Failure(
                "invalid_credentials",
                "Invalid credentials.",
                StatusCodes.Status401Unauthorized);
        }

        return ServiceResult<AuthResponseDto>.Success(CreateAuthResponse(user));
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
