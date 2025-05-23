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

        public IActionResult GetAll() 
        {
            IEnumerable<OrderHeaderDto>? list;

            string userId = string.Empty;

            if (!User.IsInRole(StaticDetails.RoleAdmin))
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().ToString();

            }

            ResponseDto?  responseDto = _orderService.GetAllOrders(userId).GetAwaiter().GetResult();

            if (responseDto != null && responseDto.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(responseDto?.Result));
            }
            else
            {
                list = new List<OrderHeaderDto>();
            }

            return Json(new { data = list });

        }
    }
}
