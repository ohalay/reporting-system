using Amazon.S3;
using Amazon.SQS;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(option => option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<Config>(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddTransient<IAmazonSQS>(_ => new AmazonSQSClient());
builder.Services.AddTransient<IAmazonS3>(_ => new AmazonS3Client());

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();