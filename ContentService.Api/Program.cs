using Amazon.S3;
using ContentService.Application.Interfaces;
using ContentService.Application.Mappings;
using ContentService.Application.Queries.Handlers;
using ContentService.Application.Services;
using ContentService.Infrastructure;
using ContentService.Infrastructure.MessageBroker;
using Microsoft.AspNetCore.Http.Features;
using RabbitMQ.Client;

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

//service
builder.Services.AddScoped<IImageService, ImageService>();

// unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

// rabbitmq
builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
});
builder.Services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>();

// aws
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

// grpc
/*builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:5001");  // Replace with UserService URL
});*/

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
