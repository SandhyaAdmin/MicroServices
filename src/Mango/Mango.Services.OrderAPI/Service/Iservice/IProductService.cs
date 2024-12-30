using Mango.Services.OrderAPI.Models.Dto;

namespace Mango.Services.OrderAPI.Service.Iservice
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
