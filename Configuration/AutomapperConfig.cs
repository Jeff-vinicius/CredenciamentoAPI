using AutoMapper;
using CredenciamentoAPI.ViewModels;
using CredenciamentoAPI.Business.Models;

namespace CredenciamentoAPI.Configuration
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<Convenio, ConvenioViewModel>().ReverseMap();
            CreateMap<Convenio, ConvenioViewModel>().ReverseMap();
            CreateMap<EspecialidadeViewModel, Especialidade>();

            CreateMap<EspecialidadeImagemViewModel, Especialidade>().ReverseMap();

            CreateMap<Especialidade, EspecialidadeViewModel>()
                .ForMember(dest => dest.NomeConvenio, opt => opt.MapFrom(src => src.Convenio.Nome));
        }
    }
}
