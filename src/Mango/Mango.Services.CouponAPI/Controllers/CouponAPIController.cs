using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public CouponAPIController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;   
        }

        [HttpGet]

        public object Get()
        {
            try
            {
                IEnumerable<Coupon> couponList = _appDbContext.Coupons.ToList();

                return couponList;
            }
            catch (Exception ex) 
            { 
            }

            return null;
        }

        [HttpGet]
        [Route("{id:int}")]
        public object Get(int id)
        {
            try
            {
                Coupon coupon = _appDbContext.Coupons.First(x => x.CouponId == id);
                return coupon;
            }
            catch (Exception ex)
            { 
            }
            return null;
        }


    }
}
