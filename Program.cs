using ApiSample.Services;
using ApiSample;
using ApiSample.ShedulerServices;
using ApiSample.Authentication;

var builder = WebApplication.CreateBuilder(args);




builder.Services.Configure<Settings>(builder.Configuration.GetSection("EntitySettings"));
builder.Services.AddSingleton<IDataServices, DataProcessingService>();
builder.Services.AddHostedService<CleanupService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IApiKeyValidation, ApiKeyValidation>();
builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen(config =>
{
    //this causes Swagger to add an input so you can to add the value to header when you are executing an api method.
    config.OperationFilter<ICustomFilter>();
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
