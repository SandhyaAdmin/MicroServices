using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ProductAPI.Models
{
    public class Product
    {
        [Key]
        public int ProductId {  get; set; }

        [Required]
        public string Name { get; set; }

        [Range(1,1000)]
        public double Price { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        // Image Url is the Global URL, with the domain name
        public string? ImageUrl { get; set; }

        // Local path is the path respective to the www root folder.
        // We are not using this field when we are creating but we will pass that on when we are updating
        public string? ImageLocalPath { get; set; }
    }
}
