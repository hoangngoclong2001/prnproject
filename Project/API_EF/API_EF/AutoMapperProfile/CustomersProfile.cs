using API_EF.Dtos;
using API_EF.Models;
using AutoMapper;

namespace API_EF.AutoMapperProfile
{
    public class CustomersProfile : Profile
    {
        public CustomersProfile()
        {
            //CreateMap<CustomerDto, Customer>()
            //    .ForMember(
            //        dest => dest.CustomerId,
            //        opt => opt.MapFrom(src => $"{src.CustomerId}")
            //    )
            //    .ForMember(
            //        dest => dest.CompanyName,
            //        opt => opt.MapFrom(src => $"{src.CompanyName}")
            //    )
            //    .ForMember(
            //        dest => dest.ContactName,
            //        opt => opt.MapFrom(src => $"{src.ContactName}")
            //    )
            //    .ForMember(
            //        dest => dest.Address,
            //        opt => opt.MapFrom(src => $"{src.Address}")
            //    )
            //    .ForMember(
            //        dest => dest.Orders,
            //        opt => opt.MapFrom(src => $"{src.Orders}")
            //    );

            //CreateMap<Customer, CustomerDto>()
            //    .ForMember(
            //        dest => dest.CustomerId,
            //        opt => opt.MapFrom(src => $"{src.CustomerId}")
            //    )
            //    .ForMember(
            //        dest => dest.CompanyName,
            //        opt => opt.MapFrom(src => $"{src.CompanyName}")
            //    )
            //    .ForMember(
            //        dest => dest.ContactName,
            //        opt => opt.MapFrom(src => $"{src.ContactName}")
            //    )
            //    .ForMember(
            //        dest => dest.Address,
            //        opt => opt.MapFrom(src => $"{src.Address}")
            //    )
            //    .ForMember(
            //        dest => dest.Orders,
            //        opt => opt.MapFrom(src => $"{src.Orders}")
            //    );
        }
    }
}
