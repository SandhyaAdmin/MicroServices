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
using Mango.MessageBus;
using Microsoft.EntityFrameworkCore;

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
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;


        public OrderAPIController(IMapper mapper, AppDbContext db, IProductService productService, IMessageBus messageBus, IConfiguration configuration)
        {
            _response = new ResponseDto();
            _mapper = mapper;
            _db = db;
            _productService = productService;
            _messageBus = messageBus;
            _configuration = configuration; 
        }

        [Authorize]
        [HttpGet("GetOrders")]
        public ResponseDto? Get(string? userId = "")
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders;   

                if (User.IsInRole(StaticDetails.RoleAdmin))
                {
                    //Admin role : Where UserId is null, we need to get all the orders from order header table, queried by order descending to get latest orders first

                    orderHeaders =  _db.OrderHeaders.Include(u => u.OrderDetails).OrderByDescending(u => u.OrderHeaderId).ToList();

                }
                else
                {
                    //Customer role : Where UserId is not null, we need to orders based on UserId from OrderHeader table
                    
                    orderHeaders = _db.OrderHeaders.Include(u => u.OrderDetails).Where(u => u.UserId == userId).OrderByDescending(u => u.OrderHeaderId).ToList();
                }


                _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(orderHeaders);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [Authorize]
        [HttpGet("GetOrder/{id:int}")]
        public ResponseDto? Get(int id)
        {
            try
            {
                // May be we use this end in Orders module UI, where if admin or user click on order id, he will get the UI that displays the order details
                // For that we need to reterieve the data from OrderHeader table which also includes orderdeatails tables as it has foriegn key relation ship

                OrderHeader orderHeader =  _db.OrderHeaders.Include(u => u.OrderDetails).First(u => u.OrderHeaderId == id);
                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]

        public async Task<ResponseDto?> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            try
            {
                // To update the order status in the db, first we need to get the orderheader where we want to update the status
                OrderHeader orderHeader = _db.OrderHeaders.First(u=> u.OrderHeaderId == orderId);
                if (orderHeader != null)
                {
                    if(newStatus == StaticDetails.Status_Cancelled)
                    {
                        // if admin is updating the order placed by customer as cancelled then need to provide the refund, before updating the status

                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId
                        };

                        var refundService = new RefundService();
                        Refund refund = refundService.Create(options);

                    }
                    orderHeader.Status = newStatus;
                    await _db.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }



        [Authorize]
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

                var discountObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions
                    {
                        Coupon = stripeRequestDto.OrderHeader.CouponCode
                    }
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

                if (stripeRequestDto.OrderHeader.Discount > 0)
                {
                    options.Discounts = discountObj;
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

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderHeaderId);

                var service = new Stripe.Checkout.SessionService();
                // we want get the session from here, using session id which we already saved in db
                Session session = service.Get(orderHeader.StripeSessionId);

                // Get the Payment intent and check if the order is successfull or not
                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntetent = paymentIntentService.Get(session.PaymentIntentId);

                if(paymentIntetent.Status == "succeeded")
                {
                    // then payment was successful
                    orderHeader.PaymentIntentId = paymentIntetent.Id;
                    orderHeader.Status = StaticDetails.Status_Approved;
                    _db.SaveChanges();

                    RewardDto rewardDto = new()
                    {
                        UserId = orderHeader.UserId,
                        OrderId = orderHeader.OrderHeaderId,
                        RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal)

                    };

                    string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");

                    await _messageBus.PublishMessage(rewardDto, topicName);
                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }
             
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

    }
}
