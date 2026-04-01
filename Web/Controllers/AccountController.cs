using Application.DTO;
using Application.Services.Users;
// using Application.Services.Users;
using Microsoft.AspNetCore.Mvc;



namespace Web.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AccountController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password, [FromForm] bool rememberMe, [FromForm] string? returnUrl)
        {
            var result = await _identityService.LoginAsync(new LoginDTO
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe
            });

            if (result)
            {
                var redirect = string.IsNullOrEmpty(returnUrl) ? "/home" : returnUrl;
                return Redirect(redirect);
            }

            var errorUrl = string.IsNullOrEmpty(returnUrl)
                ? "/account/login?error=Invalid+email+or+password"
                : $"/account/login?error=Invalid+email+or+password&returnUrl={Uri.EscapeDataString(returnUrl)}";
            return Redirect(errorUrl);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _identityService.LogoutAsync();
            return Redirect("/account/logout");
        }
    }
}