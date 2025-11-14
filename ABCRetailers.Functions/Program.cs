using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;

var host = new HostBuilder()
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        cfg.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        cfg.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((ctx, services) =>
    {
        // 🔑 Get connection string from environment (local.settings.json or Azure App Settings)
        var storageConn = Environment.GetEnvironmentVariable("STORAGE_CONNECTION");

        // ✅ Register Azure service clients for DI
        services.AddSingleton(new TableServiceClient(storageConn));
        services.AddSingleton(new BlobServiceClient(storageConn));
        services.AddSingleton(new QueueServiceClient(storageConn));
        services.AddSingleton(new ShareServiceClient(storageConn));
    })
    .Build();

host.Run();
