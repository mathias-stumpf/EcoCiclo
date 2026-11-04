using EcoCiclo.Data;
using EcoCiclo.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Render (e outros PaaS) informa a porta pela variável PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// O SQLite não cria a pasta do arquivo; garante que "database/" exista.
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "database"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EcoCicloDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EcoCiclo")));
builder.Services.AddScoped<ServicoPesquisaResiduos>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EcoCicloDbContext>();
    await InicializadorBancoDados.InicializarAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
