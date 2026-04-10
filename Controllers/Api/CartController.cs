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
    public class CartController(DataContext dataContext, DataAccessor dataAccessor, IStorageService storageService) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly DataAccessor _dataAccessor = dataAccessor;
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

        [HttpDelete("{id}")]
        public RestResponse UpdateCartItem([FromRoute] String id)
        {
            if (CheckAuth() is UserAccess userAccess)
            {
                try
                {
                    Guid cartItemId;
                    try { cartItemId = Guid.Parse(id); }
                    catch { throw new Exception("cartItemId must be valid UUID"); }
                    // перевіряємо, що даний cartItemId належить авторизованому користувачу
                    Cart cart = _dataAccessor.GetActiveCart(userAccess.UserId)
                        ?? throw new Exception("User has no active cart");
                    CartItem cartItem = cart.CartItems.FirstOrDefault(c => c.Id == cartItemId)
                        ?? throw new Exception("cartItemId belongs no to authorized user");
                    cart.CartItems.Remove(cartItem);
                    CalcCartPrice(cart);
                    restResponse.Data = cart;
                }
                catch (Exception ex)
                {
                    restResponse.Data = ex.Message;
                }
            }
            return restResponse;
        }


        [HttpPut("{id}")]
        public RestResponse UpdateCartItem([FromRoute] String id, int inc)
        {
            if (CheckAuth() is UserAccess userAccess)
            {
                try
                {
                    if (inc == 0)
                    {
                        throw new Exception("Parameter 'inc' could not be empty");
                    }
                    Guid cartItemId;
                    try { cartItemId = Guid.Parse(id); }
                    catch { throw new Exception("cartItemId must be valid UUID"); }
                    // перевіряємо, що даний cartItemId належить авторизованому користувачу
                    Cart cart = _dataAccessor.GetActiveCart(userAccess.UserId)
                        ?? throw new Exception("User has no active cart");
                    CartItem cartItem = cart.CartItems.FirstOrDefault(c => c.Id == cartItemId)
                        ?? throw new Exception("cartItemId belongs no to authorized user");
                    // перевіряємо застосовність інкременту: підсумкова кількість
                    // замовлення не повинна бути 0 чи менша (видалення - окрема точка)
                    // а також не перевищувати наявну кількість товару (Stock)
                    int newQuantity = cartItem.Quantity + inc;
                    if(newQuantity < 0)
                    {
                        throw new Exception("Update fails: negative result obtains");
                    }
                    if (newQuantity == 0)
                    {
                        throw new Exception("Update fails: zero result obtains - Delete is separate endpoint");
                    }
                    if (newQuantity > cartItem.Product.Stock)
                    {
                        throw new Exception($"Update fails: stock limit is {cartItem.Product.Stock}");
                    }
                    cartItem.Quantity = newQuantity;
                    CalcCartPrice(cart);
                    restResponse.Data = cart;
                }
                catch (Exception ex)
                {
                    restResponse.Data = ex.Message;
                }
            }
            return restResponse;
        }

        [HttpPost("{id}")]
        public RestResponse AddProductToCart([FromRoute] String id)
        {
            if (CheckAuth() is UserAccess userAccess)
            {
                Guid productId;
                try { productId = Guid.Parse(id); }
                catch { restResponse.Data = "productId must be valid UUID"; return restResponse; }
                Cart cart = _dataAccessor.GetOrCreateActiveCart(userAccess.UserId);
                // перевіряємо чи є вже такий товар у кошику
                // якщо є, то збільшуємо кількість
                // якщо немає, то створюємо новий елемент (Item)
                CartItem? cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if(cartItem == null)
                {
                    cartItem = new()
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = 1,
                    };
                    // cart.CartItems.Add(cartItem);
                    _dataContext.CartItems.Add(cartItem);
                    _dataContext.SaveChanges();
                }
                else
                {
                    cartItem.Quantity += 1;
                }
                // перераховуємо вартості
                CalcCartPrice(cart);
                restResponse.Data = cart;
            }
            return restResponse;
        }

        private void CalcCartPrice(Cart cart)
        {
            decimal price = 0;
            foreach (var item in cart.CartItems)
            {
                if(item.DiscountId == null)
                {
                    ShopProduct product = item.Product ??
                        _dataAccessor.GetShopProductBySlug(item.ProductId.ToString())!;
                    item.Price = product.Price * item.Quantity;
                }
                else
                {
                    throw new NotImplementedException("CalcCartPrice: product discounts");
                }
                price += item.Price;
            }
            if(cart.DiscountId == null)
            {
                cart.Price = price;
            }
            else
            {
                throw new NotImplementedException("CalcCartPrice: cart discounts");
            }
            _dataContext.SaveChanges();
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
            Cart cart = _dataAccessor.GetOrCreateActiveCart(userAccess.UserId);
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