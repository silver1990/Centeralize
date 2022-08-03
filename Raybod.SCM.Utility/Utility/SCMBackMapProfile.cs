//using AutoMapper;
//using Raybod.SCM.DataTransferObject.Customer;
//using Raybod.SCM.DataTransferObject.User;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Raybod.SCM.Utility.Utility
//{
//    public class SCMBackMapProfile : Profile
//    {
//        public SCMBackMapProfile()
//        {
//            CreateMap<AddCustomerUserDto, AddUserDto>()
//            .ForMember(destination => destination.Password, option => option.MapFrom(source => source.UserCustomerInfo.Password))
//            .ForMember(destination => destination.Mobile, option => option.MapFrom(source => source.UserCustomerInfo.Mobile))
//            .ForMember(destination => destination.Telephone, option => option.MapFrom(source => source.UserCustomerInfo.Telephone))
//            .ForMember(destination => destination.UserName, option => option.MapFrom(source => source.UserCustomerInfo.UserName))
//            .ForMember(destination => destination.Image, option => option.MapFrom(source => source.UserCustomerInfo.Image));
//            CreateMap<AddUserDto,Raybod.SCM.Domain.Model.User>();
//        }
        
        
//    }
//}
