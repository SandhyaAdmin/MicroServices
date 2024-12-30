using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.Iservice;
using Newtonsoft.Json;

namespace Mango.Services.OrderAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ProductService(IHttpClientFactory httpClientFactory) 
        { 
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            IEnumerable<ProductDto> productDtos = new List<ProductDto>();

            var client = _httpClientFactory.CreateClient("Product");
            var response = await client.GetAsync($"api/product");
            var apiContent = await response.Content.ReadAsStringAsync();   
            
            var res = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            if (res.IsSuccess) 
            {
                productDtos =  JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(res.Result)).ToList();
            }

            return productDtos; 


        }
    }
}
