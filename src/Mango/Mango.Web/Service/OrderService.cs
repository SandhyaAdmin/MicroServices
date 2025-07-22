using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;   
        }
        public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                Url = Utility.StaticDetails.OrderAPIBase + "/api/order/CreateOrder",
                ApiType = Utility.StaticDetails.ApiType.POST,
                Data = cartDto
            });
        }

        public async Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                Url = Utility.StaticDetails.OrderAPIBase + "/api/order/CreateStripeSession",
                ApiType = Utility.StaticDetails.ApiType.POST,
                Data = stripeRequestDto
            });
        }

        public async Task<ResponseDto?> ValidateStripeSession(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = Utility.StaticDetails.ApiType.POST,
                Data = orderHeaderId,
                Url = StaticDetails.OrderAPIBase + "/api/order/ValidateStripeSession"
            });
        }

        public async Task<ResponseDto?> GetOrder(int orderId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = StaticDetails.ApiType.GET,
                Url = StaticDetails.OrderAPIBase + "/api/order/GetOrder/" + orderId
            });


        }

        public async Task<ResponseDto?> GetAllOrders(string? userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = StaticDetails.ApiType.GET,
                Data = userId,
                Url = StaticDetails.OrderAPIBase + "/api/order/GetAllOrders"
            });
        }

        public async Task<ResponseDto?> UpdateOrderStaus(int orderId, string newStatus)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = newStatus,
                Url = StaticDetails.OrderAPIBase + "/api/order/UpdateOrderStatus/" + orderId

            });
        }
    }
}
