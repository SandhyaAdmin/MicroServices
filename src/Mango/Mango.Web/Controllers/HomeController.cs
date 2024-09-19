using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private ResponseDto _responseDto;
           
        public HomeController(IProductService productService)
        {
            _productService = productService;
            _responseDto = new ResponseDto();
        }

        public async Task<IActionResult> Index()
        {
            List<ProductDto> productList = new List<ProductDto>();

             _responseDto = await _productService.GetAllProductsAsync();

            if (_responseDto != null && _responseDto.IsSuccess)
            {
                productList = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(_responseDto.Result));
            }
            else
            {
                TempData["error"] = _responseDto?.Message;
            }

            return View(productList);
        }

        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDto productDto = new ProductDto();

            _responseDto = await _productService.GetProductByIdAsync(productId);

            if (_responseDto != null && _responseDto.IsSuccess)
            {
                productDto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(_responseDto.Result));
            }
            else
            {
                TempData["error"] = _responseDto?.Message;
            }

            return View(productDto);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
