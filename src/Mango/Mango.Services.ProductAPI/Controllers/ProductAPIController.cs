using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    //[Authorize]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private IMapper _mapper;
        private ResponseDto _responseDto;


        public ProductAPIController(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _responseDto = new ResponseDto();
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Product> productList = _appDbContext.Products.ToList();
                _responseDto.Result = _mapper.Map<IEnumerable<ProductDto>>(productList);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
               Product product = _appDbContext.Products.First(x => x.ProductId == id);
               _responseDto.Result  =  _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            { 
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;  
            }
            return _responseDto;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Post([FromBody] ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);

                _appDbContext.Products.Add(product);
                _appDbContext.SaveChanges();

                 _responseDto.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex) 
            { 
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }

            return _responseDto;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put([FromBody] ProductDto productDto)
        {
            try
            {
               Product product = _mapper.Map<Product>(productDto);
                _appDbContext.Products.Update(product); 
                _appDbContext.SaveChanges();

                _responseDto.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex) 
            { 
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;  
            }
            return _responseDto;
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Delete(int id)
        {
            try
            {
               Product product =  _appDbContext.Products.First(x => x.ProductId == id);
                _appDbContext.Products.Remove(product);
                _appDbContext.SaveChanges();
            }
            catch (Exception ex)
            { 
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }


    }
}
