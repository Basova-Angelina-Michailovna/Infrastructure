using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StretchRoom.Infrastructure.Interfaces;

internal interface IStartupBase
{
    void ConfigureServices(IServiceCollection services);
    void Configure(IApplicationBuilder app, IHostEnvironment env);
}