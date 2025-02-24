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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CORSPolicy");
app.MapControllers();
app.Run();

// dotnet ef dbcontext scaffold "server=localhost;database=tracio_shop;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../ShopService.Domain/Entities --context-dir ../ShopService.Infrastructure/Contexts --context TracioShopDbContext --force
