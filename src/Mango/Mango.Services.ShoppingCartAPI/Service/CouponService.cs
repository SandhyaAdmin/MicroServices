using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.Iservice;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class CouponService : ICouponService
    {
       private IHttpClientFactory _httpClientFactory;
       public CouponService(IHttpClientFactory httpClientFactory) 
       { 
            _httpClientFactory = httpClientFactory;
       }

        public async Task<CouponDto> GetCoupon(string couponCode)
        {
            CouponDto couponDto = new CouponDto();
            var httpClient = _httpClientFactory.CreateClient("Coupon");

            var response = await httpClient.GetAsync($"/api/coupon/GetByCode/{couponCode}");
            var content = await response.Content.ReadAsStringAsync();

            var res = JsonConvert.DeserializeObject<ResponseDto>(content.ToString());
            if (res.IsSuccess) {

                couponDto = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(res.Result));
            }

            return couponDto;


        }
    }
}
