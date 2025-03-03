using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using NotificationService.Api.Extensions;
using NotificationService.Application.Services;
using NotificationService.Application.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Load Firebase Admin SDK Credentials
var secret = new GetSecrets();
var firebaseCredentials = await secret.GetFireBaseCredentials();

FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(firebaseCredentials) });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureCors();
builder.Services.ConfigureMediatr();
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.ConfigureSignalR();
builder.Services.ConfigureGrpcClients();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureAuthorization();
builder.Services.ConfigureSignalR();
builder.Services.ConfigureMapper();
builder.Services.ConfigureRabbitMq();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService API V1");
        c.RoutePrefix = "swagger"; 
    });
}

app.MapControllers();
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CORSPolicy");
app.MapHub<NotificationHub>("notification-hub");
app.Run();

