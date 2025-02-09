using System.Reflection;
using ContentService.Application.Mappings;
using ContentService.Application.Queries.Handlers;
using ContentService.Infrastructure;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// mediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(GetBlogsQueryHandler).Assembly);
});

// mapper
builder.Services.AddAutoMapper(typeof(BlogProfile).Assembly);

// repository
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

// set volume limit
builder.Services.Configure<FormOptions>(builder.Configuration.GetSection("FormOptions"));


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

//dotnet ef dbcontext scaffold "server=localhost;database=tracio_content;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../ContentService.Domain/Entities --context-dir ../ContentService.Infrastructure/Contexts --context TracioContentDbContext --force
