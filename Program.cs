using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("找不到 DefaultConnection 連線字串。");

builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(connectionString));
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<ClassSessionService>();
builder.Services.AddScoped<EnrollmentApplicationService>();

var app = builder.Build();
var seedOnly = args.Contains("--seed-only", StringComparer.OrdinalIgnoreCase);
if (app.Environment.IsDevelopment() &&
    (seedOnly || builder.Configuration.GetValue<bool>("Seed:SyncAutumn2026Schedule")))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var result = await DbSeeder.SeedAsync(db);
    Console.WriteLine(
        $"2026 秋季課表同步完成：Mia 建立={result.MiaCreated}，Celia 建立={result.CeliaCreated}，" +
        $"課程新增={result.CoursesCreated}，課程更新={result.CoursesUpdated}，" +
        $"場次新增={result.SessionsCreated}，" +
        $"場次更新={result.SessionsUpdated}，場次移除={result.SessionsRemoved}。");
}
if (seedOnly)
{
    return;
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
