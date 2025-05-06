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
    }
}
