using DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace ZippedImageServer.Extensions;

public static class StartupDb
{
    public static void InitializeDb(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;

        var context = services.GetService<ServerContext>();
        context!.Database.Migrate();
    }
}