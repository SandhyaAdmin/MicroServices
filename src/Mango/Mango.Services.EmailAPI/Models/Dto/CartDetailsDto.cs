using System.Net.Http.Headers;

namespace Mango.Services.EmailAPI.Models.Dto
{
    public class CartDetailsDto
    {
        public int CartDetaildId { get; set; }
        public int CartHeaderId { get; set; }
        public CartHeaderDto? CartHeader { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product { get; set; }

        public int Count { get; set; }
    }
}
