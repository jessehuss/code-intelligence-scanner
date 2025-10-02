using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Cataloger.Scanner.Commands;
using Cataloger.Scanner.Analyzers;
using Cataloger.Scanner.Resolvers;
using Cataloger.Scanner.Samplers;
using Cataloger.Scanner.KnowledgeBase;
using Cataloger.Scanner.Services;
using MongoDB.Driver;

namespace Cataloger.Scanner;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var rootCommand = new RootCommand("Code Intelligence Scanner & Knowledge Base Seeder");
        
        // Add subcommands
        rootCommand.AddCommand(ScanCommand.CreateCommand());
        rootCommand.AddCommand(SearchCommand.CreateCommand());
        rootCommand.AddCommand(GetTypeCommand.CreateCommand());
        
        return await rootCommand.InvokeAsync(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register MongoDB client
                services.AddSingleton<IMongoClient>(provider =>
                {
                    var connectionString = context.Configuration.GetConnectionString("KnowledgeBase") 
                        ?? "mongodb://localhost:27017/catalog_kb";
                    return new MongoClient(connectionString);
                });

                services.AddSingleton<IMongoDatabase>(provider =>
                {
                    var client = provider.GetRequiredService<IMongoClient>();
                    var databaseName = context.Configuration["KnowledgeBase:DatabaseName"] ?? "catalog_kb";
                    return client.GetDatabase(databaseName);
                });

                // Register analyzers
                services.AddScoped<POCOExtractor>();
                services.AddScoped<OperationExtractor>();
                services.AddScoped<RelationshipInferencer>();

                // Register resolvers
                services.AddScoped<CollectionResolver>();

                // Register samplers
                services.AddScoped<MongoSampler>();
                services.AddScoped<IPIIDetector, PIIDetector>();

                // Register knowledge base services
                services.AddScoped<KnowledgeBaseWriter>();

                // Register services
                services.AddScoped<IncrementalScanner>();

                // Register commands
                services.AddScoped<ScanCommand>();
                services.AddScoped<SearchCommand>();
                services.AddScoped<GetTypeCommand>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
}
