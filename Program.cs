using Microsoft.EntityFrameworkCore;
using ShortenUrl.Models;
using ShortenUrl.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
}).WithOpenApi();

app.MapGet("/all-urls", (ApiDbContext apiDbContext) =>
{
	var urlService = new UrlService(apiDbContext);
	var urls = urlService.GetUrls();
	return urls;
}).WithOpenApi();

app.MapGet("/{shortUrl}", (ApiDbContext apiDbContext, string shortUrl) =>
{
	var urlService = new UrlService(apiDbContext);
	var url = urlService.GetUrl(shortUrl);
	if (url is not null)
	{
		return Results.Redirect(url.OriginalUrl);
	}
	return Results.NotFound();
}).WithOpenApi();

app.Run();

class ApiDbContext : DbContext
{
	public virtual DbSet<Url> Urls { get; set; }
	public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
	{

	}
}