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
    [Route("api/v{version:apiVersion}/especialidades")]
    public class EspecialidadesController : MainController
    {
        private readonly IEspecialidadeRepository _especialidadeRepository;
        private readonly IEspecialidadeService _especialidadeService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EspecialidadesController(INotificador notificador,
                                    IEspecialidadeRepository especialidadeRepository,
                                    IEspecialidadeService especialidadeService,
                                    IMapper mapper,
                                    IUser user,
                                    IHttpContextAccessor httpContextAccessor) : base(notificador, user)
        {
            _especialidadeRepository = especialidadeRepository;
            _especialidadeService = especialidadeService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IEnumerable<EspecialidadeViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<EspecialidadeViewModel>>(await _especialidadeRepository.ObterEspecialidadesConvenios());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EspecialidadeViewModel>> ObterPorId(Guid id)
        {
            var especialidadeViewModel = await ObterEspecialidade(id);

            if (especialidadeViewModel == null) return NotFound();

            return especialidadeViewModel;
        }

        [ClaimsAuthorize("Especialidade", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<EspecialidadeViewModel>> Adicionar(EspecialidadeViewModel especialidadeViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + especialidadeViewModel.Imagem;
            if (!UploadArquivo(especialidadeViewModel.ImagemUpload, imagemNome))
            {
                return CustomResponse(especialidadeViewModel);
            }

            especialidadeViewModel.Imagem = imagemNome;
            await _especialidadeService.Adicionar(_mapper.Map<Especialidade>(especialidadeViewModel));

            return CustomResponse(especialidadeViewModel);
        }

        [ClaimsAuthorize("Especialidade", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, EspecialidadeViewModel especialidadeViewModel)
        {
            if (id != especialidadeViewModel.Id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var especialidadeAtualizacao = await ObterEspecialidade(id);

            if (string.IsNullOrEmpty(especialidadeViewModel.Imagem))
                especialidadeViewModel.Imagem = especialidadeAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (especialidadeViewModel.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + especialidadeViewModel.Imagem;
                if (!UploadArquivo(especialidadeViewModel.ImagemUpload, imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                especialidadeAtualizacao.Imagem = imagemNome;
            }

            especialidadeAtualizacao.ConvenioId = especialidadeViewModel.ConvenioId;
            especialidadeAtualizacao.Nome = especialidadeViewModel.Nome;
            especialidadeAtualizacao.Descricao = especialidadeViewModel.Descricao;
            especialidadeAtualizacao.Valor = especialidadeViewModel.Valor;
            especialidadeAtualizacao.Ativo = especialidadeViewModel.Ativo;

            await _especialidadeService.Atualizar(_mapper.Map<Especialidade>(especialidadeAtualizacao));

            return CustomResponse(especialidadeViewModel);
        }

        [ClaimsAuthorize("Especialidade", "Excluir")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<EspecialidadeViewModel>> Excluir(Guid id)
        {
            var especialidade = await ObterEspecialidade(id);

            if (especialidade == null) return NotFound();

            await _especialidadeService.Remover(id);

            return CustomResponse(especialidade);
        }

        private async Task<EspecialidadeViewModel> ObterEspecialidade(Guid id)
        {
            return _mapper.Map<EspecialidadeViewModel>(await _especialidadeRepository.ObterEspecialidadeConvenio(id));
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para esta Especialidade!");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }

        #region UploadAlternativo

        [ClaimsAuthorize("Especialidade", "Adicionar")]
        [HttpPost("Adicionar")]
        public async Task<ActionResult<EspecialidadeViewModel>> AdicionarAlternativo(
            [ModelBinder(BinderType = typeof(EspecialidadeModelBinder))]
        EspecialidadeImagemViewModel especialidadeViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";
            if (!await UploadArquivoAlternativo(especialidadeViewModel.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(ModelState);
            }

            especialidadeViewModel.Imagem = imgPrefixo + especialidadeViewModel.ImagemUpload.FileName;
            await _especialidadeService.Adicionar(_mapper.Map<Especialidade>(especialidadeViewModel));

            return CustomResponse(especialidadeViewModel);
        }

        [RequestSizeLimit(40000000)]
        [HttpPost("imagem")]
        public ActionResult AdicionarImagem(IFormFile file)
        {
            return Ok(file);
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para esta Especialidade!");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }

        #endregion
    }
}

