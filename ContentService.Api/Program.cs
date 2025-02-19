using ContentService.Api.Extensions;
using ContentService.Application.Services;
using ContentService.Infrastructure;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
var builder = WebApplication.CreateBuilder(args);

// Load Firebase Admin SDK Credentials
var secret = new GetSecrets();
var firebaseCredentials = await secret.GetFireBaseCredentials();

Console.WriteLine(firebaseCredentials);

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

var app = builder.Build();

// Middleware pipeline
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

//dotnet ef dbcontext scaffold "server=localhost;database=tracio_content;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../ContentService.Domain/Entities --context-dir ../ContentService.Infrastructure/Contexts --context TracioContentDbContext --force
