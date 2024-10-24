using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private ResponseDto _response;
        private IMapper _mapper;
        private readonly AppDbContext _appDbContext;

        public CartAPIController(IMapper mapper, AppDbContext appDbContext)
        {
            _response = new ResponseDto();
            _mapper = mapper;
            _appDbContext = appDbContext;
        }

        [HttpPost("cartupsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cardHeaderFromDb =  await _appDbContext.CartHeaders.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);
                if (cardHeaderFromDb == null) 
                { 
                    // Create cart Header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                     _appDbContext.Add(cartHeader);
                    await _appDbContext.SaveChangesAsync();

                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;

                    CartDetails cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails.First());
                    _appDbContext.Add(cartDetails);
                    await _appDbContext.SaveChangesAsync();

                }
                else
                {
                    // If header is not null for that user,
                    // Check if details has same product or not 
                    var cartDetailFromDb = await _appDbContext.CartDetails.AsNoTracking()
                        .FirstOrDefaultAsync(
                        u => u.ProductId == cartDto.CartDetails.First().ProductId &&
                        u.CartHeaderId == cardHeaderFromDb.CartHeaderId);
                    if(cartDetailFromDb == null)
                    {
                        // create a cart details for that cart header Id
                        cartDto.CartDetails.First().CartHeaderId = cardHeaderFromDb.CartHeaderId;
                        CartDetails cardDetails = _mapper.Map<CartDetails>(cartDto.CartDetails.First());
                        _appDbContext.Add(cardDetails);
                        await _appDbContext.SaveChangesAsync();

                    }
                    else
                    {
                        // update count in cart details
                        cartDto.CartDetails.First().Count += cartDetailFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetaildId = cartDetailFromDb.CartDetaildId;

                        CartDetails cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails.First());
                        _appDbContext.Update(cartDetails);
                        await _appDbContext.SaveChangesAsync();
                    }
                    _response.Result = cartDto;
                    _response.IsSuccess = true;
                }

            }
            catch (Exception ex) 
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;

        }

        [HttpDelete("RemoveCart")]
        public async Task<ResponseDto> ReamoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails =  _appDbContext.CartDetails.First(u=> u.CartDetaildId == cartDetailsId);
                int totalCartDetailsForHeaderId =  _appDbContext.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();
                 _appDbContext.CartDetails.Remove(cartDetails);
                 if(totalCartDetailsForHeaderId == 1)
                 {
                    CartHeader cartHeader = await _appDbContext.CartHeaders.FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
                    _appDbContext.CartHeaders.Remove(cartHeader);

                 }

                await _appDbContext.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();   
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDto cartDto = new CartDto()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_appDbContext.CartHeaders.FirstOrDefaultAsync(u => u.UserId == userId))
                };

                var cartDetails =  _appDbContext.CartDetails.Where(u => u.CartHeaderId == cartDto.CartHeader.CartHeaderId);

                cartDto.CartDetails =  _mapper.Map<IEnumerable<CartDetailsDto>>(cartDetails);

                foreach (var item in cartDto.CartDetails) 
                {
                    cartDto.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }

                _response.Result = cartDto;
                _response.IsSuccess = true;
            }
            catch (Exception ex) 
            { 
                _response.Message= ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;   
        }
        
    }
}
