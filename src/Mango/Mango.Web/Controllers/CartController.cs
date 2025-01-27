
using Mango.Web.Models;
using Mango.Web.Models.Dto;
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
        private readonly IOrderService _orderService;
        public CartController(ICartService cartService, IOrderService orderService) 
        { 
            _cartService = cartService;
            _orderService = orderService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

       [Authorize]
        //view
        public async Task<IActionResult> Checkout()
        {

            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        [HttpPost]
        //[ActionName("Checkout")]
        public async Task<IActionResult> PlaceOrder(CartDto cartDto)
        {
            // we receive the cartDto with the Post, which we need to pass down the order api
            //cartDto : Post Request, which we get from view, once we click on checkout
            // Load latest cart dto details

            CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
            cart.CartHeader.Phone = cartDto.CartHeader.Phone;
            cart.CartHeader.Email = cartDto.CartHeader.Email;
            cart.CartHeader.Name = cartDto.CartHeader.Name;

            var response = await _orderService.CreateOrder(cart);
            OrderHeaderDto orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

            if (response != null && response.IsSuccess)
            {
                // If oder is save in db then we need to work on payment gate way
                // get stripe session and redirect to stripe to place an order

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                StripeRequestDto stripeRequestDto = new StripeRequestDto()
                {
                    ApprovedUrl = domain + "cart/Confirmation?orderId=" + orderHeader.OrderHeaderId,
                    CancelUrl = domain + "cart/Checkout",
                    OrderHeader = orderHeader
                };

                var stripeResponse = await _orderService.CreateStripeSession(stripeRequestDto);

                StripeRequestDto stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));

                // We need to redirect our application Session URL that way it will take us to stripe check out page
                Response.Headers.Add("Location", stripeResponseResult.StripeSessionUrl);

                return new StatusCodeResult(303); // It denotes that there is redirect to another page
            }

            return View();


        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            return View(orderId);
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
