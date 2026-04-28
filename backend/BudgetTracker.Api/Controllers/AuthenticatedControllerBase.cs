using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTracker.Api.Controllers;

public abstract class AuthenticatedControllerBase : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User id claim is missing.");
        }
    }
}
