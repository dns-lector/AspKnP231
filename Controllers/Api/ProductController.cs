using AspKnP231.Data;
using AspKnP231.Data.Entities;
using AspKnP231.Middleware.Auth.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspKnP231.Controllers.Api
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(DataContext dataContext) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpGet("{id}")]
        public Object GetProduct(String id)
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

            return new
            {
                meta = new {
                    id,
                    authMessage
                },
                data = _dataContext.ShopProducts.FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id),
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
        id: "123",
        dataType: "object"   // "array"   // null
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