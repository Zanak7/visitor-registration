using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()

    // Startar upp mina Azure Functions
    .ConfigureFunctionsWorkerDefaults()

    // Tar in inställningar (lokalt används local.settings.json)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        }
    })

     // Lägger till loggar och inställningar
    .ConfigureServices(services =>
    {
        services.AddOptions();
        services.AddLogging();
    })
    .Build();

await host.RunAsync();
