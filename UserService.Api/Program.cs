using Microsoft.EntityFrameworkCore;
using UserService.Api.Helper;
using UserService.Application.Mappings;
using UserService.Domain;
using UserService.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
FirebaseConfig.InitializeFirebase();
builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// mediatR
/*builder.Services.AddMediatR(config =>
{
    //config.RegisterServicesFromAssembly(typeof(GetBlogsQueryHandler).Assembly);
});*/

// database
builder.Services.AddDbContext<TracioUserDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("tracio_content_db"),
        new MySqlServerVersion(new Version(9, 1, 0)) 
    )
);

// DIs
builder.Services.AddServices();

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

// dotnet ef dbcontext scaffold "server=localhost;database=tracio_activity;user=root;password=Nhat2003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../UserService.Domain/Entities --context-dir ../UserService.Infrastructure/Contexts --context TracioUserDbContext --force
