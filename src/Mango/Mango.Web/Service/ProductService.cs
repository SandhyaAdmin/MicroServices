using Mango.Web.Models;
using Mango.Web.Service.IService;

namespace Mango.Web.Service
{
    public class ProductService : IProductService
    {
        public Task<ResponseDto?> GetAllProductsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto?> GetProductByIdAsync()
        {
            throw new NotImplementedException();
        }
        public Task<ResponseDto?> CreateProductAsync(ProductDto productDto)
        {
            throw new NotImplementedException();
        }


        public Task<ResponseDto?> UpdateProductAsync(ProductDto productDto)
        {
            throw new NotImplementedException();
        }
        public Task<ResponseDto?> DeleteProductByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
