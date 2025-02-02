var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();

// dotnet ef dbcontext scaffold "server=localhost;database=tracio_activity;user=root;password=N@hat892003." "Pomelo.EntityFrameworkCore.MySql" --output-dir ../UserService.Domain/Entities --context-dir ../UserService.Infrastructure/Contexts --context TracioUserDbContext --force
