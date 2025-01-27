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
using Stripe.Checkout;

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
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(item.Price* 100), //$20.99 -> 2099
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            }
                        },
                        Quantity = item.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                var service = new Stripe.Checkout.SessionService();
                // the below is not a dotnet core session, its a stripe session
                Session session = service.Create(options);

                //based on the below url, our web application know where it has to redirected to capture the payment
                stripeRequestDto.StripeSessionUrl = session.Url;

                //but we will also get the stripe session id somewhere, its is best to store that in DB, like if we need to work with the
                // same session like providing the refund or tracking if the payment was successfull we can do all of that
                // if we have session id that was initiated

                OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;

                _db.SaveChanges();

                _response.Result = stripeRequestDto;

            }
            catch (Exception ex) { 
            
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

    }
}
