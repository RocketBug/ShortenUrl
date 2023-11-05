using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShortenUrl.Models;
using ShortenUrl.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<JsonOptions>(options => 
{
	options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddSwaggerGen(options => 
{
	options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		BearerFormat = "JWT",
		Scheme = "Bearer"
	});
	options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value,
        ValidAudience = builder.Configuration.GetSection("JwtSettings:Audience").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtSettings:Token").Value!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiDbContext>(options =>
{
	options.UseSqlite(connectionString);
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/register", (HttpContext httpContext, ApiDbContext apiDbContext, IConfiguration configuration, UserDto userDto) => 
{
	try
	{
		var userService = new UserService(apiDbContext, configuration);
		var user = userService.CreateUser(userDto);
		return Results.Ok(user);
	}
	catch (InvalidDataException e)
	{
		return Results.BadRequest(e.Message);
	}
}).AllowAnonymous();

app.MapPost("/auth/login", (HttpContext httpContext, ApiDbContext apiDbContext, IConfiguration configuration, UserDto userDto) =>
{
    try
    {
        var userService = new UserService(apiDbContext, configuration);
        var isAuthentic = userService.LoginUser(userDto);
        return Results.Ok(isAuthentic);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}).AllowAnonymous();

app.MapPost("/shorten", (HttpContext httpContext, ApiDbContext apiDbContext, UrlDto urlDto) =>
{
	decimal hours = urlDto.ValidMinutes / 60;
	hours = decimal.Round(hours, 2);
	if (hours > 1)
	{
		return Results.BadRequest("Can't be valid for more than an hour.");
	}
	var urlService = new UrlService(apiDbContext);
	var url = urlService.SaveUrl(urlDto, httpContext.Request.Host.ToString());
	return Results.Ok(url);
}).RequireAuthorization();

app.MapGet("/{userId}/all-urls", (ApiDbContext apiDbContext, int userId) =>
{
	var urlService = new UrlService(apiDbContext);
	var urls = urlService.GetUrls(userId);
	return urls;
}).RequireAuthorization();

app.MapGet("/{shortUrl}", (ApiDbContext apiDbContext, string shortUrl) =>
{
	var urlService = new UrlService(apiDbContext);
	var url = urlService.GetUrl(shortUrl);
	if (url is not null)
	{
		return Results.Ok(url.OriginalUrl);
	}
	return Results.NotFound();
}).RequireAuthorization();

app.Run();