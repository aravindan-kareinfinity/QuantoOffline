using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Quanto
{
    /// <summary>
    /// ASP.NET Core (Kestrel) host replacing Katana/OWIN WebApp.Start for .NET 10.
    /// </summary>
    public static class AspNetCoreHost
    {
        public static IDisposable Start(IEnumerable<string> urls)
        {
            var urlList = urls?.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList()
                ?? new List<string>();
            if (urlList.Count == 0)
                throw new ArgumentException("At least one URL is required.", nameof(urls));

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                ContentRootPath = AppContext.BaseDirectory,
            });

            builder.Logging.ClearProviders();
            builder.WebHost.UseUrls(urlList.ToArray());
            builder.Services.AddControllers()
                .AddNewtonsoftJson();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var allowed = MachineConfig.AllowedOrigins;
                    if (!string.IsNullOrWhiteSpace(allowed))
                    {
                        var origins = allowed.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
                    }
                    else
                    {
                        // Backward compatible default for existing web apps; set AllowedOrigins to tighten.
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                });
            });

            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Remove("Server");
                if (HttpMethods.IsOptions(context.Request.Method))
                {
                    var origin = context.Request.Headers.Origin.FirstOrDefault() ?? "*";
                    var allowed = MachineConfig.AllowedOrigins;
                    if (!string.IsNullOrWhiteSpace(allowed))
                    {
                        var origins = allowed.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (origins.Any(o => string.Equals(o.Trim(), origin, StringComparison.OrdinalIgnoreCase)))
                            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                    }
                    else
                    {
                        context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                    }
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
                    context.Response.Headers["Access-Control-Allow-Headers"] =
                        "authorization,access-control-allow-origin,x-requested-with,Accept,accept-language,content-language,content-type";
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    return;
                }

                await next();
            });
            app.UseCors();
            app.MapControllers();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                        ctx.Context.Response.Headers["Pragma"] = "no-cache";
                        ctx.Context.Response.Headers["Expires"] = "0";
                    }
                });
            }

            var cts = new CancellationTokenSource();
            var started = new ManualResetEventSlim(false);
            app.Lifetime.ApplicationStarted.Register(() => started.Set());
            var runTask = app.RunAsync(cts.Token);

            // Wait until Kestrel has bound so callers can fail clearly on port conflicts.
            if (!started.Wait(TimeSpan.FromSeconds(30)) || runTask.IsFaulted || runTask.IsCanceled)
            {
                try { cts.Cancel(); } catch { /* ignore */ }
                try { app.DisposeAsync().AsTask().GetAwaiter().GetResult(); } catch { /* ignore */ }
                if (runTask.IsFaulted && runTask.Exception != null)
                    throw runTask.Exception.GetBaseException();
                throw new InvalidOperationException(
                    "Kestrel failed to start listening on: " + string.Join(", ", urlList));
            }

            return new HostDisposable(app, cts, runTask);
        }

        private sealed class HostDisposable : IDisposable
        {
            private readonly WebApplication _app;
            private readonly CancellationTokenSource _cts;
            private readonly Task _runTask;
            private bool _disposed;

            public HostDisposable(WebApplication app, CancellationTokenSource cts, Task runTask)
            {
                _app = app;
                _cts = cts;
                _runTask = runTask;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                try
                {
                    _cts.Cancel();
                    _app.StopAsync().GetAwaiter().GetResult();
                    _runTask.GetAwaiter().GetResult();
                    _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
                catch
                {
                    // ignore shutdown races
                }
                finally
                {
                    _cts.Dispose();
                }
            }
        }
    }
}
