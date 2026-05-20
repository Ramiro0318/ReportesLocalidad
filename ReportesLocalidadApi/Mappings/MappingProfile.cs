using AutoMapper;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Models.Entities;

namespace ReportesLocalidadApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegistroDto, Usuarios>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IdRolNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.Reportes, opt => opt.Ignore());

        CreateMap<SubirReporteDto, Reportes>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ImgUrl, opt => opt.Ignore())
            .ForMember(dest => dest.FechaSubida, opt => opt.Ignore())
            .ForMember(dest => dest.FechaEdicion, opt => opt.Ignore())
            .ForMember(dest => dest.IdEstado, opt => opt.Ignore())
            .ForMember(dest => dest.IdCategoriaNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.IdEstadoNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.IdUsuarioNavigation, opt => opt.Ignore());

        CreateMap<EditarReporteDto, Reportes>()
            .ForMember(dest => dest.ImgUrl, opt => opt.Ignore())
            .ForMember(dest => dest.FechaSubida, opt => opt.Ignore())
            .ForMember(dest => dest.FechaEdicion, opt => opt.Ignore())
            .ForMember(dest => dest.ClientRequestId, opt => opt.Ignore())
            .ForMember(dest => dest.IdEstado, opt => opt.Ignore())
            .ForMember(dest => dest.IdCategoriaNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.IdEstadoNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.IdUsuarioNavigation, opt => opt.Ignore());
    }
}
