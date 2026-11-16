var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("PlayerApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5244/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("TournamentApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5080/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
