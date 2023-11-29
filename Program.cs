using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShortenUrl.Models;
using ShortenUrl.Models.DTO;
using ShortenUrl.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Text.Json.Serialization;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var allowFrontEndOrigin = "FrontEnd";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<UrlService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<JsonOptions>(options =>
{
	options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCors(options =>
{
	options.AddPolicy(name: allowFrontEndOrigin,
		policy =>
		{
			policy.WithOrigins("http://localhost:4200")
			.AllowCredentials()
			.AllowAnyMethod()
			.AllowAnyHeader();
		});
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

app.UseCors(allowFrontEndOrigin);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/users/register", (HttpContext httpContext, UserService userService, UserAuthDto userDto) =>
{
	try
	{
		var user = userService.CreateUser(userDto);
		var refreshToken = user.RefreshToken;
		setTokenCookie(httpContext, refreshToken);
		var userResponse = new UserDto
		{
			Id = user.UserId,
			Token = user.Token,
			UserName = user.UserName
		};
		return Results.Ok(userResponse);
	}
	catch (InvalidDataException e)
	{
		return Results.BadRequest(e.Message);
	}
}).AllowAnonymous();

app.MapPost("/users/authenticate", (HttpContext httpContext, UserService userService, UserAuthDto userDto) =>
{
	try
	{
		var user = userService.LoginUser(userDto);
		var refreshToken = user.RefreshToken;
		setTokenCookie(httpContext, refreshToken);
		var userResponse = new UserDto
		{
			Id = user.UserId,
			Token = user.Token,
			UserName = user.UserName
		};
		return Results.Ok(userResponse);
	}
	catch (Exception e)
	{
		return Results.BadRequest(e.Message);
	}
}).AllowAnonymous();

app.MapPost("/users/refresh-token", (HttpContext httpContext, UserService userService) =>
{
	try
	{
		var refreshToken = httpContext.Request.Cookies["refreshToken"];
		var user = userService.RefreshToken(refreshToken);
		setTokenCookie(httpContext, user.RefreshToken);
		var userResponse = new UserDto
		{
			Id = user.UserId,
			Token = user.Token,
			UserName = user.UserName
		};
		return Results.Ok(userResponse);
	}
	catch (Exception e)
	{
		return Results.BadRequest(e.Message);
	}
}).AllowAnonymous();

app.MapPost("/url/shorten", (HttpContext httpContext, UrlService urlService, UrlDto urlDto) =>
{
	try
	{
		var url = urlService.SaveUrl(urlDto, httpContext.Request.Host.ToString());
		return Results.Ok(url);
	}
	catch (Exception e)
	{
		return Results.BadRequest(e.Message);
	}
}).RequireAuthorization();

app.MapGet("/url/{userId}/all-urls", (UrlService urlService, string userId) =>
{
	var urls = urlService.GetUrls(userId);
	return urls;
}).RequireAuthorization();

app.MapGet("/url/{shortUrl}/redirect", (UrlService urlService, string shortUrl) =>
{
	var url = urlService.GetUrl(shortUrl);
	if (url is not null)
	{
		return Results.Ok(url.OriginalUrl);
	}
	return Results.NotFound();
}).RequireAuthorization();

app.MapDelete("/url/{shortUrl}/archive", (UrlService urlService, string shortUrl) =>
{
	try
	{
		urlService.ArchiveUrl(shortUrl);
		return Results.Ok();
	}
	catch (Exception e)
	{
		return Results.BadRequest(e.Message);
	}
}).RequireAuthorization();

static void setTokenCookie(HttpContext httpContext, string refreshToken)
{
	var cookieOptions = new CookieOptions
	{
		HttpOnly = true,
		Secure = true,
		Expires = DateTime.Now.AddDays(7)
	};
	httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
}

app.Run();