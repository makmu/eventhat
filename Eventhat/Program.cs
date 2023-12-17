using System.Text;
using Eventhat;
using Eventhat.Aggregators;
using Eventhat.Components;
using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Mail;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextFactory<MessageContext>(
    options => options.UseSqlServer("Server=127.0.0.1;Database=MessageDb;User Id=sa;Password=!SAPassword!1;")
    //options => options.UseInMemoryDatabase("MessageDb")
);
builder.Services.AddDbContextFactory<ViewDataContext>(
    options => options.UseSqlServer("Server=127.0.0.1;Database=ViewDataDb;User Id=sa;Password=!SAPassword!1;")
    //options => options.UseInMemoryDatabase("ViewDataDb")
);

builder.Services.AddSingleton<MessageStore>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "me",
        ValidAudience = "my_audience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication")),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Add security requirements
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer token"
    });

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// get services

var messageContextFactory = app.Services.GetService<IDbContextFactory<MessageContext>>();
if (messageContextFactory == null) throw new Exception("Could not initialize message database");
using (var context = messageContextFactory.CreateDbContext())
{
    context.Database.EnsureCreated();
}

var viewDataContextFactory = app.Services.GetService<IDbContextFactory<ViewDataContext>>();
if (viewDataContextFactory == null) throw new Exception("Could not initialize view data database");

using (var context = viewDataContextFactory.CreateDbContext())
{
    context.Database.EnsureCreated();
}

var messageStore = app.Services.GetService<MessageStore>();
if (messageStore == null) throw new Exception("Could not initialize message store");

// build aggregators
IEnumerable<IAgent> aggregators = new IAgent[]
{
    new HomePageAggregator(viewDataContextFactory, messageStore),
    new UserCredentialsAggregator(viewDataContextFactory, messageStore),
    new VideoOperationsAggregator(viewDataContextFactory, messageStore),
    new CreatorsVideosAggregator(viewDataContextFactory, messageStore),
    new AdminUsersAggregator(viewDataContextFactory, messageStore),
    new AdminStreamsAggregator(viewDataContextFactory, messageStore)
};

// build components
IEnumerable<IAgent> components = new IAgent[]
{
    new IdentityComponent(messageStore),
    new SendEmailComponent(messageStore, new Mailer(), "no-reply@test.com"),
    new VideoPublishingComponent(messageStore)
};

// start aggregators
foreach (var aggregator in aggregators) aggregator.Start();

// start components
foreach (var component in components) component.Start();

app.Run();

// stop aggregators
foreach (var aggregator in aggregators) aggregator.Stop();

// stop components
foreach (var component in components) component.Stop();