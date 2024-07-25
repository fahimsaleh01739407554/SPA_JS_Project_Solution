using Microsoft.EntityFrameworkCore;
using SPA_JS_Project.HostedService;
using SPA_JS_Project.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProductDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("db")));
builder.Services.AddHostedService<DbSeederHostedService>();
builder.Services.AddControllersWithViews()
 .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
var app = builder.Build();

app.UseStaticFiles();
app.MapDefaultControllerRoute();


app.Run();
