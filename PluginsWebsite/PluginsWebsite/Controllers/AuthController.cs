using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PluginsWebsite.Controllers
{
    public class AuthController : Controller
    {
        readonly SignInManager<User> _signInManager;
        readonly UserManager<User> _userManager;

        readonly HttpClient _client = new HttpClient();

        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet("~/logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }

        [HttpGet("~/login")]
        public async Task<IActionResult> Login(string returnUrl = "/")
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var redirectUrl = Url.Action(nameof(LoginCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("GitHub", redirectUrl);

            return Challenge(properties, "GitHub");
        }

        [HttpGet("~/login-callback")]
        public async Task<IActionResult> LoginCallback(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info is null)
            {
                return Redirect("/login");
            }

            SignInResult result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (result.Succeeded)
            {
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                Console.WriteLine("Logged in");
                return Redirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                Console.WriteLine("Locked");

                return Redirect("Account/Lockout");
            }

            string id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);



            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            if (!int.TryParse(id, out int id2))
            {
                return BadRequest();
            }

            string userName = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userName))
                return BadRequest();

            User user = new User()
            {
                Id = id,
                UserName = userName,
                EmailConfirmed = true,
            };

            IdentityResult identityResult = await _userManager.CreateAsync(user);

            if (identityResult.Succeeded)
            {
                Console.WriteLine($"Created user {user.UserName}");

                identityResult = await _userManager.AddLoginAsync(user, info);
                if (identityResult.Succeeded)
                {
                    Console.WriteLine($"Link external login {info.LoginProvider} with account {user.UserName}");

                    await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                }
                Console.WriteLine("Creation ok");

                return Redirect(returnUrl);
            }

            Console.WriteLine("BAD REQ");
            return BadRequest();
        }
    }
}
