using Microsoft.EntityFrameworkCore;
using UploadFile.Models;

namespace UploadFile
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // отримуємо рядок підключення з конфігураційного файлу, там нова база Files!
            string? connection = builder.Configuration.GetConnectionString("DefaultConnection");

            // реєструємо контекст бази даних для роботи з SQL Server (новий контекст)
            builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(connection));
            builder.Services.AddControllersWithViews();
            var app = builder.Build();

            // увімкнення обслуговування статичних файлів з папки wwwroot
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/Error404");

            // стандартний маршрут за замовчуванням
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}