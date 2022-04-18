using AutoMapper;
using CredenciamentoAPI.Controllers;
using CredenciamentoAPI.Extensions;
using CredenciamentoAPI.ViewModels;
using CredenciamentoAPI.Business.Intefaces;
using CredenciamentoAPI.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CredenciamentoAPI.V1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/convenios")]
    public class ConveniosController : MainController
    {
        private readonly IConvenioRepository _convenioRepository;
        private readonly IConvenioService _convenioService;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IMapper _mapper;

        public ConveniosController(IConvenioRepository convenioRepository,
                                      IMapper mapper,
                                      IConvenioService convenioService,
                                      INotificador notificador,
                                      IEnderecoRepository enderecoRepository,
                                      IUser user) : base(notificador, user)
        {
            _convenioRepository = convenioRepository;
            _mapper = mapper;
            _convenioService = convenioService;
            _enderecoRepository = enderecoRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<ConvenioViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ConvenioViewModel>>(await _convenioRepository.ObterTodos());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ConvenioViewModel>> ObterPorId(Guid id)
        {
            var convenio = await ObterConvenioEspecialidadesEndereco(id);

            if (convenio == null) return NotFound();

            return convenio;
        }

        [ClaimsAuthorize("Convenio", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<ConvenioViewModel>> Adicionar(ConvenioViewModel convenioViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _convenioService.Adicionar(_mapper.Map<Convenio>(convenioViewModel));

            return CustomResponse(convenioViewModel);
        }

        [ClaimsAuthorize("Convenio", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ConvenioViewModel>> Atualizar(Guid id, ConvenioViewModel convenioViewModel)
        {
            if (id != convenioViewModel.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(convenioViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _convenioService.Atualizar(_mapper.Map<Convenio>(convenioViewModel));

            return CustomResponse(convenioViewModel);
        }

        [ClaimsAuthorize("Convenio", "Excluir")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ConvenioViewModel>> Excluir(Guid id)
        {
            var convenioViewModel = await ObterConvenioEndereco(id);

            if (convenioViewModel == null) return NotFound();

            await _convenioService.Remover(id);

            return CustomResponse(convenioViewModel);
        }

        [HttpGet("endereco/{id:guid}")]
        public async Task<EnderecoViewModel> ObterEnderecoPorId(Guid id)
        {
            return _mapper.Map<EnderecoViewModel>(await _enderecoRepository.ObterPorId(id));
        }

        [ClaimsAuthorize("Convenio", "Atualizar")]
        [HttpPut("endereco/{id:guid}")]
        public async Task<IActionResult> AtualizarEndereco(Guid id, EnderecoViewModel enderecoViewModel)
        {
            if (id != enderecoViewModel.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(enderecoViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _convenioService.AtualizarEndereco(_mapper.Map<Endereco>(enderecoViewModel));

            return CustomResponse(enderecoViewModel);
        }

        private async Task<ConvenioViewModel> ObterConvenioEspecialidadesEndereco(Guid id)
        {
            return _mapper.Map<ConvenioViewModel>(await _convenioRepository.ObterConvenioEspecialidadesEndereco(id));
        }

        private async Task<ConvenioViewModel> ObterConvenioEndereco(Guid id)
        {
            return _mapper.Map<ConvenioViewModel>(await _convenioRepository.ObterConvenioEndereco(id));
        }
    }
}
