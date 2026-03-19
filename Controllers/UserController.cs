using AspKnP231.Data;
using AspKnP231.Data.Entities;
using AspKnP231.Models.Home;
using AspKnP231.Models.User;
using AspKnP231.Services.Kdf;
using AspKnP231.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;

namespace AspKnP231.Controllers
{
    public class UserController(DataContext dataContext, IStorageService storageService, IKdfService kdfService) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IStorageService _storageService = storageService;
        private readonly IKdfService _kdfService = kdfService;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            UserSignupViewModel viewModel = new();
            if (HttpContext.Session.Keys.Contains(nameof(UserSignupFormModel)))
            {
                // є збережені у сесії дані, тоді відновлюємо, використовуємо та видаляємо
                viewModel.FormModel = JsonSerializer.Deserialize<UserSignupFormModel>(
                    HttpContext.Session.GetString(nameof(UserSignupFormModel))!
                );
                ModelStateDictionary modelState = new();

                JsonElement savedState = JsonSerializer.Deserialize<JsonElement>(
                    HttpContext.Session.GetString(nameof(ModelState))!
                )!;
                foreach (var item in savedState.EnumerateObject())
                {
                    var errors = item.Value.GetProperty("Errors");
                    if (errors.GetArrayLength() > 0)
                    {
                        foreach (var err in errors.EnumerateArray())
                        {
                            modelState.AddModelError(item.Name, err.GetProperty("ErrorMessage").GetString()!);
                        }
                    }
                }
                viewModel.FormModelState = modelState;
                viewModel.IsSignupSuccessfull = modelState.IsValid;
                if (viewModel.IsSignupSuccessfull)
                {
                    // Реєструємо у БД
                    Guid userId = Guid.NewGuid();
                    _dataContext.UsersData.Add(new()
                    {
                        Id = userId,
                        Name = viewModel.FormModel!.UserName,
                        Email = viewModel.FormModel!.UserEmail,
                        Birthdate = viewModel.FormModel!.UserBirthdate!.Value,
                    });
                    String salt = Guid.NewGuid().ToString();
                    _dataContext.UserAccesses.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        UserRoleId = _dataContext.UserRoles.First(r => r.Name == "Self Registered").Id,
                        Salt = salt,
                        Dk = _kdfService.Dk(salt, viewModel.FormModel!.UserPassword),
                        Login = viewModel.FormModel!.UserLogin,
                        AvatarFilename = viewModel.FormModel!.SavedFilename,
                        CreatedAt = DateTime.Now,
                    });
                    _dataContext.SaveChanges();
                }

                HttpContext.Session.Remove(nameof(UserSignupFormModel));
                HttpContext.Session.Remove(nameof(ModelState));
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SignUpForm(UserSignupFormModel formModel)
        {
            // Перевірка мінімального віку - 3650 = 10 років
            if (formModel.UserBirthdate != null && (DateTime.Now - formModel.UserBirthdate!.Value).Days < 3650)
            {
                ModelState.AddModelError("user-birthdate", "Вік замалий для реєстрації");
            }
            // Валідація паролю - ДЗ
            if (formModel.UserPassword != formModel.UserRepeat)
            {
                ModelState.AddModelError("user-repeat", "Повтор не збігається з паролем");
            }

            if (formModel.UserLogin != null)
            {
                if (_dataContext.UserAccesses.Any(ua => ua.Login == formModel.UserLogin))
                {
                    ModelState.AddModelError("user-login", "Даний логін вже у вжитку");
                }
            }

            if (ModelState.IsValid && formModel.UserAvatar != null && formModel.UserAvatar.Length > 0)
            {
                /* Д.З. Забезпечити валідацію файлу-аватарки
                 * на предмет того, що його розширення відповідає
                 * графічним файлам. Перелік узгодити з вибором MIME 
                 * типів у контролері Storage.
                 * Якщо файл має неприпустимий тип, то додавати 
                 * помилку валідації даного поля та виводити її на формі.
                 */
                formModel.SavedFilename = _storageService.Save(formModel.UserAvatar);
            }

            HttpContext.Session.SetString(
                nameof(ModelState),
                JsonSerializer.Serialize(ModelState)
            );

            HttpContext.Session.SetString(
                nameof(UserSignupFormModel),
                JsonSerializer.Serialize(formModel)
            );
            return RedirectToAction(nameof(SignUp));
        }

        [HttpGet]
        public JsonResult SignIn()
        {
            // Basic authentication
            String authHeader = Request.Headers.Authorization.ToString();
            if (String.IsNullOrEmpty(authHeader))
            {
                return Json(new
                {
                    status = 401,
                    data = "Missing 'Authorization' header"
                });
            }
            String scheme = "Basic ";
            if (!authHeader.StartsWith(scheme))
            {
                return Json(new
                {
                    status = 401,
                    data = "Invalid 'Authorization' scheme. Must be " + scheme
                });
            }
            String basicCredentials = authHeader[scheme.Length..];
            String userPass;
            try
            {
                userPass = Encoding.UTF8.GetString(
                Convert.FromBase64String(basicCredentials));
            }
            catch(Exception ex)
            {
                return Json(new
                {
                    status = 401,
                    data = "Credentials decode error " + ex.Message
                });
            }
            String[] parts = userPass.Split(':', 2);
            if(parts.Length != 2)
            {
                return Json(new
                {
                    status = 401,
                    data = "user-pass invalid format. Missing ':'? "
                });
            }

            UserAccess? userAccess = _dataContext
                .UserAccesses
                .Include(u => u.UserData)
                .FirstOrDefault(u => u.Login == parts[0]);

            if(userAccess == null)
            {
                return Json(new
                {
                    status = 401,
                    data = "Authentication rejected"
                });
            }

            if(_kdfService.Dk(userAccess.Salt, parts[1]) != userAccess.Dk)
            {
                return Json(new
                {
                    status = 401,
                    data = "Authentication rejected."
                });
            }
            return Json(new
            {
                status = 200,
                // data = userAccess.UserData   // Object Cycle
                data = userAccess.UserData.Name
            });
        }
    }
}
