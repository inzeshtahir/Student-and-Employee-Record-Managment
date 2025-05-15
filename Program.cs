using LabAssignment6.DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string? dbConnStr = builder.Configuration.GetConnectionString("StudentRecord");
if (dbConnStr != null )
{
    builder.Services.AddDbContext<StudentrecordContext>(
        options => options.UseMySQL(dbConnStr));
}
else
{
    throw new Exception("no connection string obtained");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
