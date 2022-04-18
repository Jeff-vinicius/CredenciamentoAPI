using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using CredenciamentoAPI.Extensions;
using CredenciamentoAPI.Business.Intefaces;
using CredenciamentoAPI.Business.Notificacoes;
using CredenciamentoAPI.Business.Services;
using CredenciamentoAPI.Data.Context;
using CredenciamentoAPI.Data.Repository;

namespace CredenciamentoAPI.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            services.AddScoped<CredenciamentoDbContext>();
            services.AddScoped<IEspecialidadeRepository, EspecialidadeRepository>();
            services.AddScoped<IConvenioRepository, ConvenioRepository>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();

            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IConvenioService, ConvenioService>();
            services.AddScoped<IEspecialidadeService, EspecialidadeService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUser, AspNetUser>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;
        }
    }
}
