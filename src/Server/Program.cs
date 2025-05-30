
using GoodFriend.Server.Hubs;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSignalR().AddMessagePackProtocol(opt =>
        opt.SerializerOptions
        .WithCompression(MessagePackCompression.Lz4Block)
        .WithResolver(CompositeResolver.Create(
            StandardResolver.Instance,
            BuiltinResolver.Instance,
            AttributeFormatterResolver.Instance,
            DynamicEnumAsStringResolver.Instance,
            DynamicGenericResolver.Instance,
            DynamicUnionResolver.Instance,
            DynamicObjectResolver.Instance,
            PrimitiveObjectResolver.Instance
        ))
        .WithSecurity(MessagePackSecurity.UntrustedData)
    );
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestQuery
                    | HttpLoggingFields.RequestProtocol | HttpLoggingFields.RequestMethod
                    | HttpLoggingFields.RequestScheme | HttpLoggingFields.ResponseStatusCode;
    o.CombineLogs = true;
});
builder.Services.AddHealthChecks();
builder.Services.AddCors();

var app = builder.Build();
if (app.Environment.IsProduction())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseCors();
app.UseRouting();
app.UseHealthChecks("/healthcheck");
app.MapHub<PlayerEventHub>("/hubs/playerevents");
app.Run();
