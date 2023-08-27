using Eventhat;
using Eventhat.Aggregators;
using Eventhat.Components;
using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Testing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IMessageStreamDatabase, InMemoryMessageStreamDatabase>();
builder.Services.AddSingleton<MessageStore>();

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

// get services
var db = app.Services.GetService<IMessageStreamDatabase>();
if (db == null)
    throw new Exception("Could not inject database at start");
var messageStore = app.Services.GetService<MessageStore>();
if (messageStore == null)
    throw new Exception("Could not inject message store at start");

// build aggregators
IEnumerable<IAgent> agggregators = new IAgent[]
{
    new HomePageAggregator(db, messageStore),
    new UserCredentialsAggregator(db, messageStore)
};

// build components
IEnumerable<IAgent> components = new IAgent[]
{
    new IdentityComponent(messageStore)
};

// start aggregators
foreach (var aggregator in agggregators) Task.Run(() => aggregator.StartAsync());

// start components
foreach (var component in components) Task.Run(() => component.StartAsync());

app.Run();

// stop aggregators
foreach (var aggregator in agggregators) aggregator.Stop();

// stop components
foreach (var component in components) component.Stop();