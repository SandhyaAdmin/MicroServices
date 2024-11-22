
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) 
        { 
            _cartService = cartService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }




        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto response = await _cartService.GetCartByUserIdAsync(userId);
            if (response != null && response.IsSuccess)
            {
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                return cartDto;
            }

            return new CartDto();
        }

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            ResponseDto? responseDto = await _cartService.ResmoveFromCartAsync(cartDetailsId);
            if (responseDto.Result != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Sucecfully deleted the cart";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            ResponseDto? responseDto = await _cartService.ApplyCouponAsync(cartDto);
            if (responseDto.Result != null && responseDto.IsSuccess) 
            {
                TempData["success"] = "Sucecfully updated the cart";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            cartDto.CartHeader.CouponCode = "";
            ResponseDto? responseDto = await _cartService.ApplyCouponAsync(cartDto);
            if (responseDto.Result != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Sucecfully updated the cart";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmailCartService(CartDto cartDto)
        {
            CartDto cart  = await LoadCartDtoBasedOnLoggedInUser();
            cart.CartHeader.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            ResponseDto? responseDto = await _cartService.EmailCartService(cart);
            if (responseDto.IsSuccess = true) 
            {
                TempData["Success"] = "Email will be processed and sent shortly";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
