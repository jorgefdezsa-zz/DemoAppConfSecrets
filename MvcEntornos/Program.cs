namespace MvcEntornos
{
    using Autofac.Extensions.DependencyInjection;
    using Azure.Identity;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using System;
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
          .ConfigureAppConfiguration((builderContext, config) =>
          {
              IHostEnvironment env = builderContext.HostingEnvironment;

              var configuration = config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                        .AddEnvironmentVariables()
                                        .Build();

              if (!env.IsDevelopment())
              {
                  config.AddAzureAppConfiguration(options =>
                  {
                  options.Connect(new Uri(configuration["AppConfiguration:Url"]), new ManagedIdentityCredential())
                         .ConfigureRefresh(refresh =>
                         {
                             refresh.Register("MvcEntornos:AppConfiguration:Sentinel", refreshAll: true)
                                .SetCacheExpiration(new TimeSpan(0, 1, 0));
                         })
                         .Select(KeyFilter.Any, LabelFilter.Null)
                         .Select(KeyFilter.Any, $"{env.EnvironmentName}")
                             .ConfigureKeyVault(kv =>
                             {
                                 kv.SetCredential(new DefaultAzureCredential());
                             });
                  });
              }
          })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Limits.MaxRequestBodySize = 52428800;
                        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(120);
                        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(120);
                    });
                    webBuilder.ConfigureServices(confirgureService =>
                    {
                        confirgureService.Configure<IISServerOptions>(serverConfig =>
                        {
                            serverConfig.MaxRequestBodySize = 52428800;
                        });
                    });
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseSerilog()
                        .UseStartup<Startup>();
                });
    }
}
