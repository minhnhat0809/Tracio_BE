//using UserService.Api.Services;


using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RouteService.Application.Interfaces.Services;
using RouteService.Application.Mappings;
using RouteService.Infrastructure.DIs;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//builder.Services.AddHttpClient<IRouteService, RouteService.Application.Interfaces.Services.RouteService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// services
builder.Services.AddInfrastructure(builder.Configuration);

// cors
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("CORSPolicy", corsPolicyBuilder => corsPolicyBuilder
        .AllowAnyHeader().WithOrigins()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed((_) => true));
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CORSPolicy");
app.MapControllers();
app.Run();

/*
 *
 * dotnet ef dbcontext scaffold "server=localhost;database=tracio_route;user=root;password=Nhat2003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../RouteService.Domain/Entities --context-dir ../RouteService.Infrastructure/Contexts --context TracioRouteDbContext --force

 */