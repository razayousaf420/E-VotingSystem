WebApplicationBuilder l_WebApplicationBuilder = WebApplication.CreateBuilder(args);
l_WebApplicationBuilder.Services.AddControllersWithViews();
l_WebApplicationBuilder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


WebApplication l_WebApplication = l_WebApplicationBuilder.Build();
if (l_WebApplication.Environment.IsDevelopment() == false)
{
    l_WebApplication.UseExceptionHandler("/Home/Error"); //To be develop.
    l_WebApplication.UseHsts();
}

l_WebApplication.UseHttpsRedirection();
l_WebApplication.UseStaticFiles();
l_WebApplication.UseRouting();
l_WebApplication.UseAuthorization();
l_WebApplication.UseSession();

l_WebApplication.MapControllerRoute(
    name: "default",
 pattern: "{controller=Account}/{action=Index}");

l_WebApplication.Run();