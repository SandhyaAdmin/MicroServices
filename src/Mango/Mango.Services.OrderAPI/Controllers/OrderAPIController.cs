using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Service.Iservice;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mango.Services.OrderAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Mango.Services.OrderAPI.Utility;
using Mango.Services.OrderAPI.Models;
using Newtonsoft.Json;
using Stripe;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        protected ResponseDto _response;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private IProductService _productService;

        public OrderAPIController(IMapper mapper, AppDbContext db, IProductService productService)
        {
            _response = new ResponseDto();
            _mapper = mapper;
            _db = db;
            _productService = productService;
        }

        //[Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateDto([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = StaticDetails.Status_Pending;
                // Initially, status is pending, once the payment is processed, will change the status to approved

                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);


                // If want to access the Entity, I cam use entity
                OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;

                await _db.SaveChangesAsync();

               orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
               _response.Result = orderHeaderDto;

            }
            catch (Exception ex) 
            { 
                _response.IsSuccess = false;
                _response.Message = ex.Message;             
            }

            return _response;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {
                StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = "https://example.com/success",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                           {
                               new Stripe.Checkout.SessionLineItemOptions
                               {
                                   Price = "price_1MotwRLkdIwHu7ixYcPLm5uZ",
                                   Quantity = 2,
                               },
                           },
                    Mode = "payment",
                };
                var service = new Stripe.Checkout.SessionService();
                service.Create(options);
            }
            catch (Exception ex) { 
            
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

    }
}
