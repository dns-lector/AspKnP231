using System.Diagnostics;
using AspKnP231.Models;
using AspKnP231.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace AspKnP231.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult IoC()
        {
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
