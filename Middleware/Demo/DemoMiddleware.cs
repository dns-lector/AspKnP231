using AspKnP231.Services.Hash;

namespace AspKnP231.Middleware.Demo
{
    public class DemoMiddleware
    {
        private readonly RequestDelegate _next;          // Всі класи Middleware
                                                         // мають оголосити конструктор,
        public DemoMiddleware(RequestDelegate next)      // який приймає посилання на 
        {                                                // наступний Middleware - next
            _next = next;                                // та зберігає його для роботи.
        }                                                // Побудова ланцюга Middleware - 
        // одноразовий процес при старті застосунку


        // InvokeAsync - логіка обробника, яка виконується для кожного запиту - багаторазово
        // За такої схеми інжекція сервісів здійснюється у метод, утворюючи вільний
        // перелік та порядок параметрів 
        public async Task InvokeAsync(HttpContext context, IHashService hashService)
        {
            // Логіка "прямого ходу"
            // context - той самий HttpContext, що буде спільним як для 
            // контролерів, так і для Razor, відповідно може вживатись для 
            // передачі даних від Middleware


            // Call the next delegate/middleware in the pipeline.
            await _next(context);

            // Логіка "зворотнього ходу"
        }
    }
}
