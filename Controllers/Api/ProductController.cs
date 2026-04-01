using AspKnP231.Data;
using AspKnP231.Data.Entities;
using AspKnP231.Middleware.Auth.Token;
using AspKnP231.Models.Api;
using AspKnP231.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspKnP231.Controllers.Api
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(DataContext dataContext, IStorageService storageService) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IStorageService _storageService = storageService;

        [HttpGet("{id}")]
        public RestResponse GetProduct(String id)
        {
            String authMessage;
            if(HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                authMessage = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            }
            else
            {
                authMessage = HttpContext.Items[nameof(AuthTokenMiddleware)]?.ToString() ?? string.Empty;
            }
            var product = _dataContext.ShopProducts.FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id);
            return new()
            {
                Meta = new() {
                    ServerTime = DateTime.Now.Ticks,
                    Cache = 3600,
                    ResourceId = id,
                    AuthStatus = authMessage,
                    DataType = product == null ? "null" : "object"
                },
                Data = product == null ? null : new ShopProductPage
                {
                    Product = new()
                    {
                        Id = product.Id.ToString(),
                        Title = product.Title,
                        Price = product.Price,
                        Stock = product.Stock,
                        Slug = product.Slug,
                        ImageUrl = _storageService.GetPathPrefix() + (product.ImageUrl ?? "no_image.webp"),
                        Rating = null,
                        Discount = 0,
                    },
                    Recommended = []
                },
            };
        }
    }
}
/*
Uniform interface (уніфікований формат відповідей АРІ)
{
    meta: {
        time: 1260123562,
        cache: 3600,
        dataType: "object",   // "array"   // null
        id: "123",
    },
    data: {
    }
}

Д.З. Реалізувати вихід з авторизованого стану фронтенда
за умови одержання відповіді з помилкою по токену.
Для спрощення перевірки можна встановити короткий 
термін придатності токена та натискати кнопку поза
цим часом.
 */