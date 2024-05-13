
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.SemanticKernel;

namespace DatabaseChatBot.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        /*** Configure Open AI ***/
        // Create a kernel with a logger and Azure OpenAI chat completion service
        //////////////////////////////////////////////////////////////////////////////////
        var AzureOpenAIDeploymentName = "gpt-35-turbo";//Env.Var("AzureOpenAI:ChatCompletionDeploymentName")!;
        var AzureOpenAIEndpoint = "https://oai-test-wus.openai.azure.com/";//Env.Var("AzureOpenAI:Endpoint")!;
        var AzureOpenAIApiKey = "0a7417a990ff423ba75c0b92fbb20559";//Env.Var("AzureOpenAI:ApiKey")!;

        var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins");

        var kernelBuilder = Kernel.CreateBuilder()
                    .AddAzureOpenAIChatCompletion(AzureOpenAIDeploymentName, AzureOpenAIEndpoint, AzureOpenAIApiKey);
        kernelBuilder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
        kernelBuilder.Plugins.AddFromPromptDirectory(Path.Combine(pluginsDirectory, "OrchestratorPlugin"));
        Kernel kernel = kernelBuilder.Build();
        builder.Services.AddSingleton<Kernel>(kernel);

        builder.Services.AddSingleton<Services.SQLDataProdiverOptions>(x =>
        {
            return new Services.SQLDataProdiverOptions()
            {
                ConnectionString = builder.Configuration.GetConnectionString("SqlConnectionString"),
                TableDescriptionFileUrl = builder.Configuration.GetValue<string>("TableDescriptionsUrl"),
            };
        });
        builder.Services.AddScoped<Services.IDataProvider, Services.SQLDataProvider>();
        builder.Services.AddScoped<Plugins.OrchestratorPlugin.Orchestrator>();

        var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance("AzureAd");
        tokenAcquirerFactory.Build();
        builder.Services.AddSingleton<ITokenAcquirer>(tokenAcquirerFactory.GetTokenAcquirer());

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


#if DEBUG
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
#else
//TODO: add prod cors policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
#endif

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.UseCors();

        app.MapControllers();

        app.Run();
    }
}

