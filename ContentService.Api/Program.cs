using ContentService.Api.Extensions;
using ContentService.Application.Hubs;
using ContentService.Application.Services;
using ContentService.Infrastructure;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load Firebase Admin SDK Credentials
var secret = new GetSecrets();
var firebaseCredentials = await secret.GetFireBaseCredentials();

FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(firebaseCredentials) });

// Add services
builder.Services.AddControllers();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureAuthorization();
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureRabbitMq();
builder.Services.ConfigureGrpcClients();
builder.Services.ConfigureAwsServices(builder.Configuration);
builder.Services.ConfigureMediatr();
builder.Services.ConfigureMapper();
builder.Services.ConfigureAzure(builder.Configuration);
builder.Services.ConfigureHub();
builder.Services.ConfigureLog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseSerilog();

var grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT") ?? "6002";
var restPort = Environment.GetEnvironmentVariable("REST_PORT") ?? "5002";

// config ports
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(grpcPort), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; // gRPC
    });

    options.ListenAnyIP(int.Parse(restPort), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1; // REST API
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ContentService API V1");
        c.RoutePrefix = "swagger"; 
    });
}

var busControl = app.Services.GetRequiredService<IBusControl>();
await busControl.StartAsync();

app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CORSPolicy");
app.MapHub<ContentHub>("content-hub");
app.MapControllers();
app.Run();

//dotnet ef dbcontext scaffold "server=localhost;database=tracio_content;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../ContentService.Domain/Entities --context-dir ../ContentService.Infrastructure/Contexts --context TracioContentDbContext --force
