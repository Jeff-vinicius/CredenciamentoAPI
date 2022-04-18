using CredenciamentoAPI.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace CredenciamentoAPI.Extensions
{
    public class EspecialidadeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true
            };

            var especialidadeImagemViewModel = JsonSerializer.Deserialize<EspecialidadeImagemViewModel>(bindingContext.ValueProvider.GetValue("especialidade").FirstOrDefault(), serializeOptions);
            especialidadeImagemViewModel.ImagemUpload = bindingContext.ActionContext.HttpContext.Request.Form.Files.FirstOrDefault();

            bindingContext.Result = ModelBindingResult.Success(especialidadeImagemViewModel);
            return Task.CompletedTask;
        }
    }
}
