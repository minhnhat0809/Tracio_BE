using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ShopService.Api.Extensions;
using ShopService.Application.Services;
using ShopService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load Firebase Admin SDK Credentials
var secret = new GetSecrets();
var firebaseCredentials = await secret.GetFireBaseCredentials();

FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(firebaseCredentials) });

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureAuthorization();
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureGrpcClients();
builder.Services.ConfigureAwsServices(builder.Configuration);
builder.Services.ConfigureMediatr();
builder.Services.ConfigureMapper();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT") ?? "6004";
var restPort = Environment.GetEnvironmentVariable("REST_PORT") ?? "5004";

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopService API V1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CORSPolicy");
app.MapControllers();
app.Run();

// dotnet ef dbcontext scaffold "server=localhost;database=tracio_shop;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../ShopService.Domain/Entities --context-dir ../ShopService.Infrastructure/Contexts --context TracioShopDbContext --force
