using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
        public IActionResult Logout()
        {
            return View();
        }

    }

}
