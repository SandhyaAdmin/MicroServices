using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stripe = Stripe;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    [Authorize]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private ResponseDto _response;
        private IMapper _mapper;

        public CouponAPIController(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _response = new ResponseDto();  
            _mapper = mapper;
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Coupon> couponList = _appDbContext.Coupons.ToList();

                _response.Result = _mapper.Map<IEnumerable<CouponDto>>(couponList);  
            }
            catch (Exception ex) 
            { 
                _response.IsSuccess = false;  
                _response.Message = ex.Message; 

            }

            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                Coupon coupon = _appDbContext.Coupons.First(x => x.CouponId == id);
                _response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("GetByCode/{code}")]
        public ResponseDto GetByCode(string code)
        {
            try
            {
                Coupon coupon = _appDbContext.Coupons.First(x => x.CouponCode.ToLower() == code.ToLower());
                _response.Result = _mapper.Map<CouponDto>(coupon); 
            }
            catch(Exception ex) 
            {
                _response.Result=false;
                _response.Message = ex.Message; 
            }
            return _response;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Post([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);

                _appDbContext.Coupons.Add(coupon);
                _appDbContext.SaveChanges();

                var options = new stripe.CouponCreateOptions
                {
                    Name = coupon.CouponCode,
                    Currency = "usd",
                    PercentOff = (long)(coupon.DiscountAmout),
                    Id = coupon.CouponCode
                };
                var service = new stripe.CouponService();
                service.Create(options);

                _response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex) 
            { 
                _response.Result=false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);
                _appDbContext.Coupons.Update(coupon);
                _appDbContext.SaveChanges();

                _response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex) 
            {
                _response.Result=false;
                _response.Message = ex.Message;
            }
            return _response;   
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Delete(int id)
        {
            try
            {
                Coupon coupon = _appDbContext.Coupons.First(x => x.CouponId == id);
                _appDbContext.Remove(coupon);
                _appDbContext.SaveChanges();

        
                var service = new stripe.CouponService();
                service.Delete(coupon.CouponCode);

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
