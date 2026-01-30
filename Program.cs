using PortfolyoDbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();





builder.Services.AddDbContext<portfolyodbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Her program açýldýðýnda DbContext sýnýfýný kullanrak veri tabanýna baðlanýr


var app = builder.Build();









// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

//Proram cs --> projemizin ilk açýldýðýnda çalýþacak kodlar
//Projem ilk açýldýðýnda veri tabanýna baðlanmalý
//Bunun için veri tabanýmý temsil eden sýnýf(DbContext) tanýmlamam lazým   