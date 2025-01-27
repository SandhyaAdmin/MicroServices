using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;


namespace Mango.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config => {
                // As the order to created of type shopping cart, CartTotal must go to Order total
                config.CreateMap<OrderHeaderDto, CartHeaderDto>()
                .ForMember(dest => dest.CartTotal, u=> u.MapFrom(src => src.OrderTotal)).ReverseMap();

                // Retrive Product name and price from CartDeatils Dto and assigned to OrderDetailsDto
                config.CreateMap<CartDetailsDto, OrderDetailsDto>()
                .ForMember(dest => dest.ProductName, u => u.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, u => u.MapFrom(src => src.Product.Price));

                // In reverse mapping we dont care about the above scanarion, So, we can do directly
                config.CreateMap<OrderDetailsDto, CartDetailsDto>();

                config.CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();

                config.CreateMap<OrderDetails,OrderDetailsDto>().ReverseMap();
            });

            return mappingConfig;
        }
    }
}
