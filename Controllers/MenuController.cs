using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Task4.Entities;
using Task4.Models;
using Task4.Services;

namespace Task4.Controllers
{
    [Authorize]
    public class MenuController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileAccessService _fileAccessService;

        public MenuController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IFileAccessService fileAccessService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _fileAccessService = fileAccessService;
        }

        [AllowAnonymous]
        public IActionResult Index(string returnUrl)
        {
            if (User.IsInRole("Player"))
            {
                return RedirectToAction("Index", nameof(Game));
            }

            var path = _webHostEnvironment.WebRootPath + "/App_Data/Data.txt";
            string broCount = "",
                sisCount = "";

            string[] lines = _fileAccessService.ReadLines(path);

            if (lines.Length >= 3)
            {
                broCount = lines[1];
                sisCount = lines[2];
            }

            return View(new MenuViewModel()
            {
                BroCount = broCount,
                SisCount = sisCount,
                ReturnUrl = Url.Action(nameof(Index))
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(string provider, string returnUrl)
        {
            var redirectUrl = "Menu/LoginCallback";
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        
        [AllowAnonymous]
        [ActionName(nameof(LoginCallback))]
        public async Task<IActionResult> LoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Game));
            }

            return RedirectToAction(nameof(RegisterExternal));
        }

        [AllowAnonymous]
        public IActionResult RegisterExternal()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ActionName(nameof(RegisterExternal))]
        public async Task<IActionResult> RegisterExternalConfirmed(RegisterExternalViewModel model)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = new ApplicationUser {UserName = model.UserName};

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var claimsResult = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "Player"));
                if (claimsResult.Succeeded)
                {
                    var identityResult = await _userManager.AddLoginAsync(user, info);
                    if (identityResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction("Game");
                    }
                    else
                        ModelState.AddModelError("UserName", "This account is already registered.");
                }
            }
            else
                ModelState.AddModelError("UserName", "This name is already in use.");


            return View(model);
        }
        
        public IActionResult Game()
        {
            return RedirectToAction("Index", "Game");
        }

        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
