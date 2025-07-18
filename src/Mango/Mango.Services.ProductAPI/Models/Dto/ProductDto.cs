using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ProductAPI.Models.Dto
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }

        // Image Url is the Global URL, with the domain name
        public string? ImageUrl { get; set; }

        // Local path is the path respective to the www root folder.
        // We are not using this field when we are creating but we will pass that on when we are updating
        public string? ImageLocalPath { get; set; } 

        // In both api project and web project, we will be uploading a file, when uploading a file there are 2 way,
        // we can convert that to bytes and pass that or we can directly pass that, I will take the approach of passing
        // the form file directly, this is where we have actual image when user uploads from web project
        // we will be saving that file in www root folder, then we will be having url which is sufficiaent to
        //store in product table.

        public IFormFile? Image {  get; set; }  

    }
}
