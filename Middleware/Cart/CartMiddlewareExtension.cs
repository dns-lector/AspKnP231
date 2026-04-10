namespace AspKnP231.Middleware.Cart
{
    public static class CartMiddlewareExtension
    {
        public static IApplicationBuilder UseCart(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CartMiddleware>();
        }
    }
}
