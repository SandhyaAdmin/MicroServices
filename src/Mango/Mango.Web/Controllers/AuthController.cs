using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text = StaticDetails.RoleAdmin, Value = StaticDetails.RoleAdmin},
                new SelectListItem{Text = StaticDetails.RoleCustomer, Value = StaticDetails.RoleCustomer},
            };

            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpPost]  
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            ResponseDto? result = await _authService.RegisterAsync(registrationRequestDto);
            ResponseDto? assignRole = null;
            if (result != null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(registrationRequestDto.Role))
                {
                    registrationRequestDto.Role = StaticDetails.RoleCustomer;  
                }
                assignRole = await _authService.AssignRoleAsync(registrationRequestDto);
                if (assignRole != null && assignRole.IsSuccess) 
                {
                    TempData["Success"] = "Registration Successfull";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = result.Message;
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text = StaticDetails.RoleAdmin, Value = StaticDetails.RoleAdmin},
                new SelectListItem{Text = StaticDetails.RoleCustomer, Value = StaticDetails.RoleCustomer},
            };

            ViewBag.RoleList = roleList;
            return View(registrationRequestDto);

        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View(loginRequestDto);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            ResponseDto? responseDto = await _authService.LoginAsync(loginRequestDto);

            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDto? loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));

                await SignInUser(loginResponseDto);

                _tokenProvider.SetToken(loginResponseDto.Token);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // In Login page, we have Display login summery, because of that we can add custom error
                //ModelState.AddModelError("CustomError", responseDto.Message);
                TempData["error"] = responseDto.Message;
                return View(loginRequestDto);
            }

        }
        public async Task<IActionResult>  Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();

            return RedirectToAction("Index","Home");
        }

        private async Task SignInUser(LoginResponseDto loginResponseDto)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(loginResponseDto.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                 jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                 jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value));


            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value));

            // The reason, we are using ClaimTypes.Role here is, bcz with the dotnet integration if you have added
            // claimType of role, you have optopn to add [Authorize(Role = StaticDetail.RoleAdmin])] attribute
            // this is automatically taken care of, because the claim is assigned when you are signing the user.

            identity.AddClaim(new Claim(ClaimTypes.Role,
                jwt.Claims.FirstOrDefault(x => x.Type == "role").Value));



            var principle = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principle);
        }   

    }

}
