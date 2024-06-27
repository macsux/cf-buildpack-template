using System.Net;
using MyBuildpackHostingStartup;

[assembly: HostingStartup(typeof(MyBuildpackStartupInjector))]

namespace MyBuildpackHostingStartup;

public class MyBuildpackStartupInjector: IHostingStartup, IStartupFilter
{
    public void Configure(IWebHostBuilder builder)
    {
        Console.WriteLine("Hello from injector1");
        builder.ConfigureServices(c => c.AddTransient<IStartupFilter, MyBuildpackStartupInjector>());

    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> nextMiddleware) => app =>
        {

            Console.WriteLine("Injector middleware is called");
            
            app.Use(async (context, next2) =>
            {
                Console.WriteLine($"Someone called me on {context.Request.Path}");
                if (context.Request.Path == "/hello")
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.WriteAsync("Hello world");
                }
                await next2();
            });
            nextMiddleware(app);
        };
}