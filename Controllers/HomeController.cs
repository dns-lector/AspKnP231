using System.Diagnostics;
using System.Text.Json;
using AspKnP231.Models;
using AspKnP231.Models.Home;
using AspKnP231.Services.Hash;
using AspKnP231.Services.Scoped;
using Microsoft.AspNetCore.Mvc;

namespace AspKnP231.Controllers
{
    public class HomeController : Controller
    {
        private readonly ScopedService _scopedService;
        private readonly IHashService _hashService;           // Інжекція сервісу "через конструктор" -
                                                              // рекомендований спосіб, передбачає 
        public HomeController(IHashService hashService, ScopedService scopedService)      
        {                                                     // readonly поле - посилання на сервіс та 
            _hashService = hashService;                       // параметр(и) конструктора того ж типу даних
            _scopedService = scopedService;
        }

        public IActionResult Forms()
        {
            HomeFormsViewModel viewModel = new();
            if (HttpContext.Session.Keys.Contains(nameof(HomeFormsFormModel))) 
            {
                // є збережені у сесії дані, тоді відновлюємо, використовуємо та видаляємо
                viewModel.FormModel = JsonSerializer.Deserialize<HomeFormsFormModel>(
                    HttpContext.Session.GetString(nameof(HomeFormsFormModel))!
                );
                HttpContext.Session.Remove(nameof(HomeFormsFormModel));
            }

            return View(viewModel);
        }

        // метод для прийому даних форми, збереження у сесії та передачі редирект
        public IActionResult FormReceiver(HomeFormsFormModel formModel)
        {
            HttpContext.Session.SetString(
                nameof(HomeFormsFormModel),
                JsonSerializer.Serialize(formModel)
            );

            return RedirectToAction(nameof(Forms));   //  /Home/Forms - формує ASP
        }



        public IActionResult Middleware()
        {
            return View();
        }

        public IActionResult IoC()
        {
            // використовуємо сервіс і передаємо дані до представлення
            ViewData["hash"] = _hashService.Digest("123");
            ViewData["hashCode"] = _hashService.GetHashCode();
            ViewData["ControllerScopedHash"] = _scopedService.GetHashCode();
            return View();
        }

        // Зв'язування моделі відбувається коли ми її зазначаємо вхідними параметром Action
        // В старих ASP, якщо модель не є обов'язковою, то необхідно
        // зазначати Nullable (HomeModelsFormModel?)
        public IActionResult Models(HomeModelsFormModel formModel)
        {
            // Особливість нових ASP - модель форми, як об'єкт, створюється
            // у будь-якому випадку, навіть якщо немає даних від форми
            // З метою розрізнення випадків наявності/відсутності даних вводиться
            // елемент форми, що відповідає за кнопку надсилання форми.

            HomeModelsViewModel viewModel = new();
            if(formModel.UserButton != null)
            {
                viewModel.FormModel = formModel;
            }

            return View(viewModel);
        }
        /* Д.З. Зробити сторінку з формою реєстрації нового користувача
         * Описати усі необхідні моделі
         */
        public IActionResult Razor()
        {
            return View();
        }
        
        public IActionResult Intro()
        {
            return View();
        }

        public IActionResult UnLayout()
        {
            return View();
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
/* Д.З. Створити сервіс дати-часу з двома методами
 * GetDate
 * GetTime
 * Реалізувати, інжектувати, вивести результати роботи
 * Створити варіації
 * SqlDateTimeService - 2026-08-19  10:00:01.134
 * NationalDateTimeService - 19.08.2026  10:00:01
 */
/* Д.З. Забезпечити коректну роботу форми на сторінці Моделей
 * (з перенаправленням та збереженням даних у сесії)
 */