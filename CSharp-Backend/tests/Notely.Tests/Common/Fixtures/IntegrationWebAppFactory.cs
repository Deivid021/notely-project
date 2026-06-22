using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Notely.Api.Data;
using Notely.Api.Services;

namespace Notely.Tests.Common.Fixtures;

public class IntegrationWebAppFactory : WebApplicationFactory<Program>
{
    public IAuthService AuthService { get; private set; } = Substitute.For<IAuthService>();
    public INoteService NoteService { get; private set; } = Substitute.For<INoteService>();
    public INoteGroupService NoteGroupService { get; private set; } = Substitute.For<INoteGroupService>();

    public void ResetMocks()
    {
        AuthService = Substitute.For<IAuthService>();
        NoteService = Substitute.For<INoteService>();
        NoteGroupService = Substitute.For<INoteGroupService>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json"),
                optional: true);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IAuthService>();
            services.RemoveAll<INoteService>();
            services.RemoveAll<INoteGroupService>();

            services.AddScoped(_ => AuthService);
            services.AddScoped(_ => NoteService);
            services.AddScoped(_ => NoteGroupService);
        });
    }
}
