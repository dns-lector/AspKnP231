using AspKnP231.Data;
using AspKnP231.Models.Shop;
using AspKnP231.Models.Shop.Admin;
using AspKnP231.Models.User;
using AspKnP231.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace AspKnP231.Controllers
{
    public class ShopController(DataContext dataContext, IStorageService storageService, DataAccessor dataAccessor) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IStorageService _storageService = storageService;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        public IActionResult Index()
        {
            ShopIndexViewModel viewModel = new()
            {
                ShopSections = [.. _dataAccessor.AllShopSections()],
            };
            return View(viewModel);
        }

        public IActionResult Section([FromRoute] String id)
        {
            ShopSectionViewModel viewModel = new()
            {
                ShopSection = _dataAccessor.GetShopSectionBySlug(id),
                ShopSections = [.. _dataAccessor.AllShopSections()],
            };
            return View(viewModel);
        }

        public IActionResult Product([FromRoute] String id)
        {
            ShopProductViewModel viewModel = new()
            {
                ShopProduct = _dataAccessor.GetShopProductBySlug(id),
                ShopSections = [.. _dataAccessor.AllShopSections()],
            };
            return View(viewModel);
        }
        /* Д.З. Завершити роботу з сторінкою товару:
         * Включити до моделі перелік промо-товарів, забезпечити
         * їх показ на сторінці
         */

        public IActionResult Admin()
        {
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String role = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? String.Empty;

                if (role == "Admin")
                {
                    ShopAdminViewModel viewModel = new()
                    {
                        ShopSections = [.. _dataContext.ShopSections.AsNoTracking()],
                    };

                    if (HttpContext.Session.Keys.Contains(nameof(ShopSectionFormModel)))
                    {
                        // є збережені у сесії дані, тоді відновлюємо, використовуємо та видаляємо
                        viewModel.ShopSectionFormModel = JsonSerializer.Deserialize<ShopSectionFormModel>(
                            HttpContext.Session.GetString(nameof(ShopSectionFormModel))!
                        );
                        ModelStateDictionary modelState = new();

                        JsonElement savedState = JsonSerializer.Deserialize<JsonElement>(
                            HttpContext.Session.GetString("ShopSectionModelState")!
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
                        viewModel.ShopSectionModelState = modelState;

                        if (modelState.IsValid)
                        {
                            _dataContext.ShopSections.Add(new()
                            {
                                Id = Guid.NewGuid(),
                                Title = viewModel.ShopSectionFormModel!.Title,
                                Description = viewModel.ShopSectionFormModel.Description,
                                Slug = viewModel.ShopSectionFormModel.Slug,
                                ImageUrl = viewModel.ShopSectionFormModel.ImageUrl!,
                            });
                            _dataContext.SaveChanges();
                        }

                        HttpContext.Session.Remove(nameof(ShopSectionFormModel));
                        HttpContext.Session.Remove("ShopSectionModelState");
                    }

                    if (HttpContext.Session.Keys.Contains(nameof(ShopProductFormModel)))
                    {
                        viewModel.ShopProductFormModel = JsonSerializer.Deserialize<ShopProductFormModel>(
                            HttpContext.Session.GetString(nameof(ShopProductFormModel))!
                        );
                        ModelStateDictionary modelState = new();

                        JsonElement savedState = JsonSerializer.Deserialize<JsonElement>(
                            HttpContext.Session.GetString("ShopProductModelState")!
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
                        viewModel.ShopProductModelState = modelState;

                        if (modelState.IsValid)
                        {
                            _dataContext.ShopProducts.Add(new()
                            {
                                Id = Guid.NewGuid(),
                                Title = viewModel.ShopProductFormModel!.Title,
                                Description = viewModel.ShopProductFormModel.Description,
                                Slug = viewModel.ShopProductFormModel.Slug,
                                ImageUrl = viewModel.ShopProductFormModel.ImageUrl,
                                ShopSectionId = viewModel.ShopProductFormModel.SectionId,
                                Price = (decimal)viewModel.ShopProductFormModel.Price,
                                Stock = viewModel.ShopProductFormModel.Stock,
                            });
                            _dataContext.SaveChanges();
                        }

                        HttpContext.Session.Remove(nameof(ShopProductFormModel));
                        HttpContext.Session.Remove("ShopProductModelState");
                    }

                    return View("Admin", viewModel);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Discount()
        {
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String role = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? String.Empty;

                if (role == "Admin")
                {
                    return View(new AdminDiscountViewModel
                    {
                        Discounts = [.. _dataContext.Discounts],
                        Products = [.. _dataContext.ShopProducts.Where(p => p.DeletedAt == null)],
                        DiscountDetails = [.. _dataContext.DiscountDetails.Include(d => d.Product).Include(d => d.Discount)]
                    });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public JsonResult DiscountDetailFormReceiver(AdminDiscountDetailFormModel formModel)
        {
            if (ModelState.IsValid)
            {
                _dataContext.DiscountDetails.Add(new()
                {
                    Id = Guid.NewGuid(),
                    DiscountId = Guid.Parse(formModel.DiscountId),
                    ProductId = Guid.Parse(formModel.ProductId),
                    Price = (decimal?)formModel.Price
                });
                _dataContext.SaveChanges();
                return Json(new
                {
                    status = "OK"
                });
            }
            else
            {
                return Json(ModelState);
            }
        }

        [HttpPost]
        public JsonResult DiscountFormReceiver(AdminDiscountFormModel formModel)
        {
            if (ModelState.IsValid)
            {
                _dataContext.Discounts.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Title = formModel.Title,
                    Description = formModel.Description,
                    Percent = formModel.Percent ?? 0,
                    Price = (decimal?)formModel.Price,
                    StartMoment = formModel.Start,
                    FinishMoment = formModel.Finish
                });
                _dataContext.SaveChanges();
                return Json(new
                {
                    status = "OK"
                });
            }
            else
            {
                return Json(ModelState);
            }
        }

        public IActionResult ProductFormReceiver(ShopProductFormModel formModel)
        {
            if (formModel.Slug != null)
            {
                if (_dataContext.ShopProducts.Any(p => p.Slug == formModel.Slug))
                {
                    ModelState.AddModelError("Slug", "Даний Slug вже у вжитку");
                }
            }

            if (ModelState.IsValid && formModel.ImageFile != null && formModel.ImageFile.Length > 0)
            {
                formModel.ImageUrl = _storageService.Save(formModel.ImageFile);
            }

            HttpContext.Session.SetString(
                "ShopProductModelState",
                JsonSerializer.Serialize(ModelState)
            );

            HttpContext.Session.SetString(
                nameof(ShopProductFormModel),
                JsonSerializer.Serialize(formModel)
            );
            return RedirectToAction(nameof(Index));
        }

        public IActionResult SectionFormReceiver(ShopSectionFormModel formModel)
        {
            if (formModel.Slug != null)
            {
                if (_dataContext.ShopSections.Any(s => s.Slug == formModel.Slug))
                {
                    ModelState.AddModelError("Slug", "Даний Slug вже у вжитку");
                }
            }

            if (ModelState.IsValid && formModel.ImageFile != null && formModel.ImageFile.Length > 0)
            {
                formModel.ImageUrl = _storageService.Save(formModel.ImageFile);
            }

            HttpContext.Session.SetString(
                "ShopSectionModelState",
                JsonSerializer.Serialize(ModelState)
            );

            HttpContext.Session.SetString(
                nameof(ShopSectionFormModel),
                JsonSerializer.Serialize(formModel)
            );
            return RedirectToAction(nameof(Index));
        }
    }
}
