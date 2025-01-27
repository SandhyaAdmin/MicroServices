﻿using Mango.Web.Models;
using Mango.Web.Service.IService;

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
    }
}
