using LanguageReader.Api.Features;
using LanguageReader.Api.Middleware;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.DependencyInjection;
using LanguageReader.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

const string ClientCorsPolicy = "ClientCors";

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
    var corsOptions = builder.Configuration
        .GetSection(ClientCorsOptions.SectionName)
        .Get<ClientCorsOptions>() ?? new ClientCorsOptions();

    var allowedOrigins = corsOptions.AllowedOrigins
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .ToArray();

    options.AddPolicy(ClientCorsPolicy, policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true);
            return;
        }

        policy.WithOrigins(allowedOrigins);
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(ClientCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapFeatureEndpoints();

app.Run();
