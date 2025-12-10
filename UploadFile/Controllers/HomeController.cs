using Microsoft.AspNetCore.Mvc;
using UploadFile.Models; // підключаємо моделі проєкту (FileModel та ApplicationContext)

namespace UploadFile.Controllers
{
    public class HomeController : Controller // контролер для головної сторінки та завантаження файлів
    {
        private readonly ApplicationContext _context; // контекст бази даних, інжектується через DI
        private readonly IWebHostEnvironment _appEnvironment; // !!! середовище хостингу, потрібне для доступу до wwwroot

        public HomeController(ApplicationContext context, IWebHostEnvironment appEnvironment) // конструктор з ін'єкцією залежностей
        {
            _context = context; // зберігаємо контекст для подальшої роботи
            _appEnvironment = appEnvironment; // зберігаємо середовище для роботи з файлами
            // без IWebHostEnvironment файли не зберігатимуться на диск в рантаймі
        }

        public IActionResult Index() // дія для відображення списку файлів
        {
            return View(_context.Files.ToList()); // передаємо всі записи з таблиці Files у подання як модель
        }

        [HttpPost] // POST-запит
        public async Task<IActionResult> AddFile(List<IFormFile> uploadedFiles)
        {
            if (uploadedFiles == null || !uploadedFiles.Any() || uploadedFiles.All(f => f.Length == 0))
            {
                TempData["Error"] = "Будь ласка, оберіть хоча б один файл!";
                return RedirectToAction("Index"); // вью індекса зверху іфами перехоплює успішність або неуспішність завантаження файлів
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadsFolder); // створюємо папку, якщо немає

            foreach (var file in uploadedFiles)
            {
                if (file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, Guid.NewGuid() + "_" + fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // зберігаємо в базу
                    var fileModel = new FileModel
                    {
                        Name = fileName,
                        Path = "/uploads/" + Path.GetFileName(filePath),
                        UploadDate = DateTime.Now
                    };

                    _context.Files.Add(fileModel); // додаємо файли в контекст (Files - це властивість в ApplicationContext)
                }
            }

            await _context.SaveChangesAsync(); // це обов’язковий рядок, без якого ніякі дані в базу НЕ запишуться, хоча ми начебто тільки-но додали їх через _context.Files.Add(fileModel).
            // _context.Files.Add(fileModel) — просто каже Entity Framework: oсь новий об'єкт, я хочу його додати в базу пізніше.
            // але він поки що лежить тільки в пам'яті (в так званому Change Tracker).
            // await _context.SaveChangesAsync(); — це команда: а от тепер дійсно виконай всі накопичені зміни і відправ їх у базу даних» (згенеруй і виконай SQL-команди INSERT)

            TempData["Success"] = "Файли успішно завантажено!";
            return RedirectToAction("Index");
        }

        public IActionResult Generate404() // всередині вью є кнопка, яка запускає саме цей метод
        {
            // будь-який маршрут, якого точно немає 
            return Redirect("/this-page-definitely-does-not-exist-404");
        }
    }
}