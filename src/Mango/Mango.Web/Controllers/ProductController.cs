using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private ResponseDto _responseDto;

        public ProductController(IProductService productService)
        {
            _productService = productService;
            _responseDto = new ResponseDto();
        }
        public async Task<IActionResult> ProductIndex()
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

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductDto productDto) 
        {
            if (ModelState.IsValid)
            {
                _responseDto = await _productService.CreateProductAsync(productDto);

                if (_responseDto != null && _responseDto.IsSuccess) 
                {
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    TempData["error"] = _responseDto?.Message;
                }
            }

            return View(productDto);

        }

        public async Task<IActionResult> ProductDelete(int productId)
        {
            _responseDto = await _productService.GetProductByIdAsync(productId);

            if (_responseDto != null && _responseDto.IsSuccess)
            {
                ProductDto product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(_responseDto.Result));

                return View(product);
            }
            else 
            {
                TempData["error"] = _responseDto?.Message;
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDto product)
        {
            _responseDto = await _productService.DeleteProductByIdAsync(product.ProductId);

            if(_responseDto.IsSuccess)
            {
                TempData["success"] = "Prodcut deleted successfully";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["error"] = _responseDto?.Message;
            }

            return View(product);
        }

        public async Task<IActionResult> ProductEdit(int productId)
        {
            _responseDto = await _productService.GetProductByIdAsync(productId);

            if (_responseDto != null && _responseDto.IsSuccess)
            {
                ProductDto product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(_responseDto.Result));

                return View(product);
            }
            else
            {
                TempData["error"] = _responseDto?.Message;
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductDto product)
        {
            if (ModelState.IsValid)
            {
                _responseDto = await _productService.UpdateProductAsync(product);

                if (_responseDto.Result != null && _responseDto.IsSuccess)
                {
                    TempData["success"] = "Prodcut edited successfully";
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    TempData["error"] = _responseDto?.Message;
                }
            }

            return View(product);
        }

    }
}
