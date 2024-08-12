using Mango.Web.Models;
namespace Mango.Web.Service.IService
{
    public interface ICouponService
    {
        Task<ResponseDto> GetCouponByCodeAsync(string code);
        Task<ResponseDto> GetCouponByIdAsync(int id);
        Task<ResponseDto> GetAllCouponsAsync();
        Task<ResponseDto> CreateCouponAsync(CouponDto couponDto);
        Task<ResponseDto> UpdateCouponAsync(CouponDto couponDto);
        Task<ResponseDto> DeleteCouponAsync(int id);


    }
}
