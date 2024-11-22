namespace Mango.Services.OrderAPI.Models.Dto
{
    public class OrderDetailsDto
    {
        public int OrderDetaildId { get; set; }
        public int OrderHeaderId { get; set; }

       // Commenting out the below one as it has IEnumerable, there might be chances of entering into infinite loop
        // public OrderHeader? OrderHeader { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product { get; set; }

        public int Count { get; set; }

        // If the price of the product got updated when the product placed, we want to include that
        public string ProductName { get; set; }
        public double Price { get; set; }
    }
}
