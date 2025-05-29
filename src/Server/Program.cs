
using GoodFriend.Server.Hubs;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            PrimitiveObjectResolver.Instance,
            StandardResolver.Instance
        ))
        .WithSecurity(MessagePackSecurity.UntrustedData)
    );
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
builder.Services.AddHttpLogging();
builder.Services.AddHealthChecks();
builder.Services.AddCors();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseRouting();
app.UseCors();
app.UseHealthChecks("/healthcheck");
app.MapHub<PlayerEventHub>("/hubs/playerevents");
app.Run();
