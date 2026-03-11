using AspKnP231.Services.Hash;
using AspKnP231.Middleware.Demo;
using AspKnP231.Services.Scoped;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Реєструємо співставлення інтерфейсу та класу-сервісу у контейнері
// "якщо буде запит на інжекцію IHashService, то слід видати об'єкт Md5HashService"
// builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddHash();   // замінено на розширення (див. HashExtension)

builder.Services.AddScoped<ScopedService>();    // без інтерфейсу - тільки один параметр типу

builder.Services.AddDistributedMemoryCache();          // Налаштування сесій
builder.Services.AddSession(options =>                 // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state
{                                                      // 
    options.IdleTimeout = TimeSpan.FromSeconds(10);    // 
    options.Cookie.HttpOnly = true;                    // 
    options.Cookie.IsEssential = true;                 // 
});                                                    // 

var app = builder.Build();

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
app.UseSession();       // Включення сесій https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state

// Місце для обробників користувача (Custom Middlewares)
// порядок оголошення відповідає за порядок зв'язування (послідовності next())
// тому порядок важливо дотримуватись, якщо один обробник залежить від інших
// (на відміну від сервісів, порядок додавання яких не грає ролі)
app.UseDemo();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
