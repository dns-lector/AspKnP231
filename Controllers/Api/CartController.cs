using AspKnP231.Data;
using AspKnP231.Data.Entities;
using AspKnP231.Middleware.Auth.Token;
using AspKnP231.Models.Api;
using AspKnP231.Models.Api.Cart;
using AspKnP231.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspKnP231.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    public class CartController(DataContext dataContext, IStorageService storageService) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IStorageService _storageService = storageService;

        private RestResponse restResponse = new();

        private UserAccess? CheckAuth()
        {
            if (!(HttpContext.User.Identity?.IsAuthenticated ?? false))
            {
                restResponse.Data =
                 HttpContext.Items[nameof(AuthTokenMiddleware)]?.ToString() ?? string.Empty;
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return null;
            }
            String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            UserAccess? userAccess = _dataContext.UserAccesses.FirstOrDefault(a => a.Login == userLogin);
            if (userAccess == null)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                restResponse.Data = "'Sub' not found";
                return null;
            }
            return userAccess;
        }

        [HttpGet("{id}")]
        public RestResponse LoadOrderDetails([FromRoute] String id) 
        {
            if(CheckAuth() is UserAccess userAccess)
            {
                var cart = _dataContext
                .Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .AsNoTracking()                
                .FirstOrDefault(c => c.Id.ToString() == id);

                if(cart != null)
                {
                    cart = cart with
                    {
                        CartItems = [..cart.CartItems.Select(ci => ci with
                        {
                            Product = ci.Product with
                            {
                                ImageUrl = _storageService.GetPathPrefix() + 
                                    (ci.Product.ImageUrl ?? "no_image.webp")
                            }
                        })]
                    };
                }
                restResponse.Data = cart;
            }
            return restResponse;
        }
        /* Д.З. Реалізувати перевірку вхідних та вилучених даних методів
         * LoadHistory: до метаданих додати відомості про загальну кількість
         *  наявних позицій
         * LoadOrderDetails: додати перевірку параметра id на формат UUID,
         *  а також встановлювати статус 404 якщо замовлення не буде знайдено
         */

        [HttpGet]
        public RestResponse LoadHistory()
        {
            UserAccess? userAccess = CheckAuth();
            if(userAccess == null) { return restResponse; }
            restResponse.Data = _dataContext
                .Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Where(c => c.UserId == userAccess.UserId)
                .AsNoTracking()
                ;
            return restResponse;
        }


        [HttpPost]
        public RestResponse CreateOrder([FromBody] CartFormModel formModel)
        {
            UserAccess? userAccess = CheckAuth();
            if (userAccess == null) { return restResponse; }

            // У користувача може бути тільки один активний кошик --
            //  інформацію про нього беремо з авторизації
            Cart? cart = _dataContext.Carts.FirstOrDefault(c => 
                c.UserId == userAccess.UserId && c.DeleteDt == null && c.OrderDt == null);
            // якщо кошику немає - створюємо новий
            if (cart == null)
            {
                cart = new Cart() 
                { 
                    Id = Guid.NewGuid(),
                    UserId = userAccess.UserId ,
                    CreateDt = DateTime.Now,
                };
                _dataContext.Carts.Add(cart);
            }
            // Проходимо по переданим деталям та додаємо їх до БД
            foreach(var cartItem in formModel.CartItems)
            {
                _dataContext.CartItems.Add(new()
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = Guid.Parse(cartItem.ProductId),
                    Quantity = cartItem.Cnt,
                    Price = (decimal) cartItem.Price,
                });
            }
            cart.Price = (decimal) formModel.Price;
            cart.OrderDt = DateTime.Now;
            _dataContext.SaveChanges();
            restResponse.Data = "Created";
            return restResponse;
        }
    }
}
/* Д.З. Реалізувати коригування даних: при створенні замовлення
 * зменшувати кількість залишків товарів, що були "продані".
 * А також додати попередню перевірку, що наявних залишків 
 * вистачає для формування замовлення.
 */