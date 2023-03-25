using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var configuration = builder.Configuration;

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetValue<string>("CacheSettings:ConnectionString");
});

// General Configuration
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(configuration["GrpcSettings:DiscountServiceUrl"]);
});
builder.Services.AddScoped<DiscountClientService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(config => 
{
    config.UsingRabbitMq((ctx, cfg) => 
    {
        cfg.Host(new Uri(configuration["EventBusSettings:HostAddress"]));
    });
});

builder.Services.AddOptions<MassTransitHostOptions>()
.Configure(options =>
{
    // if specified, waits until the bus is started before
    // returning from IHostedService.StartAsync
    // default is false
    options.WaitUntilStarted = true;

    // if specified, limits the wait time when starting the bus
    options.StartTimeout = TimeSpan.FromSeconds(60);

    // if specified, limits the wait time when stopping the bus
    options.StopTimeout = TimeSpan.FromSeconds(30);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();