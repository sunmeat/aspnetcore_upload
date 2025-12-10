using Microsoft.AspNetCore.Mvc;

namespace UploadFile.Controllers
{
    // красивий спосіб зробити загальну сторінку для помилок - створити окремий контролер
    public class ErrorController : Controller
    {
        [Route("Error404")]
        public IActionResult Error404()
        {
            return View("~/Views/Shared/Error404.cshtml");
        }
    }
}