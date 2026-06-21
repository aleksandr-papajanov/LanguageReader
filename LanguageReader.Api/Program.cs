using LanguageReader.Api.Features;
using LanguageReader.Api.Middleware;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.DependencyInjection;
using LanguageReader.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

const string DevelopmentCorsPolicy = "DevelopmentCors";

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthenticationOptions>(
    builder.Configuration.GetSection(AuthenticationOptions.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authenticationOptions = builder.Configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        options.Authority = string.IsNullOrWhiteSpace(authenticationOptions.Authority)
            ? null
            : authenticationOptions.Authority;
        options.Audience = authenticationOptions.Audience;
        options.RequireHttpsMetadata = authenticationOptions.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(authenticationOptions.Issuer),
            ValidIssuer = authenticationOptions.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(authenticationOptions.Audience),
            ValidAudience = authenticationOptions.Audience
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFeatureHandlers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevelopmentCorsPolicy, policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => builder.Environment.IsDevelopment());
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception exception)
    {
        throw new InvalidOperationException(
            $"Failed to apply database migrations on startup. {exception.GetBaseException().Message}",
            exception);
    }

    var vocabularyWordDetailsExists = dbContext.Database
        .SqlQueryRaw<int>("select 1 from information_schema.tables where table_schema = 'public' and table_name = 'vocabulary_word_details'")
        .Any();

    if (!vocabularyWordDetailsExists)
    {
        throw new InvalidOperationException(
            "Database migrations ran, but the table 'vocabulary_word_details' is still missing. " +
            "For local development, run './scripts/reset-local-database.ps1 -Force' " +
            "or verify the '__EFMigrationsHistory' table before starting the API.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevelopmentCorsPolicy);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapFeatureEndpoints();

app.Run();