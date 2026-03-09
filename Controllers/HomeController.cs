using System.Diagnostics;
using AspKnP231.Models;
using AspKnP231.Models.Home;
using AspKnP231.Services.Hash;
using Microsoft.AspNetCore.Mvc;

namespace AspKnP231.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHashService _hashService;           // Інжекція сервісу "через конструктор" -
                                                              // рекомендований спосіб, передбачає 
        public HomeController(IHashService hashService)       // readonly поле - посилання на сервіс та 
        {                                                     // параметр(и) конструктора того ж типу даних
            _hashService = hashService;                       // 
        }

        public IActionResult IoC()
        {
            // використовуємо сервіс і передаємо дані до представлення
            ViewData["hash"] = _hashService.Digest("123");
            ViewData["hashCode"] = _hashService.GetHashCode();
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