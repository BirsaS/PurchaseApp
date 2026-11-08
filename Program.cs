using Microsoft.EntityFrameworkCore;
using PurchaseApp.Data;
using PurchaseApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PurchaseRequestWorkflowService>();
builder.Services.AddSingleton<PurchaseRequestApiQueue>();
builder.Services.AddHostedService<PurchaseRequestApiWorker>();
builder.Services.AddHttpClient<PurchaseRequestApiService>(client =>
    client.BaseAddress = new Uri("https://purchasemockapi.onrender.com/"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Set ConnectionStrings:DefaultConnection in user secrets or environment variables.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(NormalizePostgresConnectionString(connectionString)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/PurchaseRequests/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PurchaseRequests}/{action=Index}/{id?}");

app.Run();

static string NormalizePostgresConnectionString(string value)
{
    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
        || uri.Scheme is not ("postgres" or "postgresql"))
    {
        return value;
    }

    var credentials = uri.UserInfo.Split(':', 2);
    var connectionString = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty
    };

    foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
    {
        var parts = parameter.Split('=', 2);
        var key = Uri.UnescapeDataString(parts[0].Replace('+', ' '));
        var parameterValue = parts.Length > 1
            ? Uri.UnescapeDataString(parts[1].Replace('+', ' '))
            : string.Empty;
        connectionString[key] = parameterValue;
    }

    return connectionString.ConnectionString;
}
