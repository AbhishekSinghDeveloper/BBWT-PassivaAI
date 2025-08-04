using AutoMapper;

using BBWM.Demo.IdHashing.DTO;
using BBWM.Demo.Northwind.Model;

namespace BBWM.Demo.IdHashing;

public class IdHashingMappingProfile : Profile
{
    public IdHashingMappingProfile()
    {
        CreateMap<Customer, CustomerHashingDTO>().ReverseMap();

        CreateMap<OrderDetails, OrderDetailHashingDTO>().ReverseMap();

        CreateMap<Order, OrderHashingDTO>()
                .ForMember(
                    dto => dto.HasResellerItems,
                    conf => conf.MapFrom(
                        order => order.OrderDetails.Any(detail => detail.IsReseller)))
            .ReverseMap();

        CreateMap<Order, SimpleOrderHashingDTO>();
    }
}
