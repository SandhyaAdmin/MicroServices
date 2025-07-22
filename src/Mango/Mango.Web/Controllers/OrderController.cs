using Mango.Web.Models.Dto;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Mango.Web.Models;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;   
        }
        public IActionResult OrderIndex()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll(string status) 
        {
            IEnumerable<OrderHeaderDto>? list;

            string userId = string.Empty;

            if (!User.IsInRole(StaticDetails.RoleAdmin))
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            }

            ResponseDto?  responseDto = _orderService.GetAllOrders(userId).GetAwaiter().GetResult();

            if (responseDto != null && responseDto.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(responseDto?.Result));
                switch (status) 
                {
                    case "approved":
                        list = list.Where(u => u.Status == StaticDetails.Status_Approved).ToList();
                        break;
                    case "readyforpickup":
                        list = list.Where(u=>u.Status == StaticDetails.Status_ReadyForPickup).ToList();
                        break;
                    case "cancelled":
                        list = list.Where(u => u.Status == StaticDetails.Status_Cancelled || u.Status == StaticDetails.Status_Refunded).ToList();
                        break;
                    default:
                        break;


                }
            }
            else
            {
                list = new List<OrderHeaderDto>();
            }

            return Json(new { data = list });

        }


        public async Task<IActionResult> OrderDetail(int orderId)
        {
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();

            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            var response = await _orderService.GetOrder(orderId);

            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            }
            if (!User.IsInRole(StaticDetails.RoleAdmin))
            {
                return NotFound();
            }

            return View(orderHeaderDto);


        }

        [HttpPost("OrderReadyForPickUp")]

        public async Task<IActionResult> OrderReadyForPickUp(int orderHeaderId)
        {
            var response = await _orderService.UpdateOrderStaus(orderHeaderId, StaticDetails.Status_ReadyForPickup);
            if (response != null && response.IsSuccess) 
            {
                TempData["success"] = "Order status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new {orderId = orderHeaderId});
            }
            return View();
        }

        [HttpPost("CompleteOrder")]
        public async Task<IActionResult> CompleteOrder(int orderHeaderId)
        {
            var reponse = await _orderService.UpdateOrderStaus(orderHeaderId,StaticDetails.Status_Completed);

            if (reponse != null && reponse.IsSuccess) 
            {
                TempData["success"] = "Order status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new {orderId = orderHeaderId});
            }
            return View();
        }

        [HttpPost("CancelOrder")]

        public async Task<IActionResult> CancelOrder(int orderHeaderId)
        {
            var response = await _orderService.UpdateOrderStaus(orderHeaderId,StaticDetails.Status_Cancelled);

            if(response != null && response.IsSuccess)
            {
                TempData["success"] = "Order status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new {orderId = orderHeaderId});
            }

            return View();
        }
    }
}
