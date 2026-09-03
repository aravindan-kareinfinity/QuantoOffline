using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Cors;
using Owin;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Quanto
{
    public class OwinConfiguration
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Use((context, next) =>
            {
                var req = context.Request;
                context.Response.Headers.Remove("Server");
                return next.Invoke();
            });
            
            // Set stage marker for proper pipeline ordering
            appBuilder.UseStageMarker(PipelineStage.PostAcquireState);

            // Configure WebSocket support
            
            // Configure Web API
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new AllowOptionsHandler());
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            appBuilder.UseWebApi(config);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                var path = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().FullName).Directory.FullName;
                var fileSystem = new VirtualFileSystem(path);
                var random = new Random(int.MaxValue);
                appBuilder.UseStaticFiles(new Microsoft.Owin.StaticFiles.StaticFileOptions()
                {
                    FileSystem = fileSystem,
                    OnPrepareResponse = ctx =>
                    {
                        ctx.OwinContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                        ctx.OwinContext.Response.Headers["Pragma"] = "no-cache";
                        ctx.OwinContext.Response.Headers["Expires"] = "0";
                    }
                });
                appBuilder.UseStageMarker(PipelineStage.MapHandler);
            }
        }
    }

    public class AllowOptionsHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Logger.Current.Error(string.Format("{0}-{1}", request.RequestUri.AbsolutePath, request.Method));
            HttpResponseMessage response = null;
            
            if (request.Method == HttpMethod.Options)
            {
                response = new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
                response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers",
                    "authorization,access-control-allow-origin,x-requested-with,Accept,accept-language,content-language,content-type");
                response.Headers.Add("Accept", "application/json, text/plain, */*");
                
                // Handle Origin header
                if (request.Headers.Contains("Origin"))
                {
                    response.Headers.Add("Access-Control-Allow-Origin", request.Headers.GetValues("Origin").FirstOrDefault());
                }
                else
                {
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            else
            {
                response = await base.SendAsync(request, cancellationToken);
                
                // Add CORS headers to all responses
                if (request.Headers.Contains("Origin"))
                {
                    response.Headers.Add("Access-Control-Allow-Origin", request.Headers.GetValues("Origin").FirstOrDefault());
                }
                else
                {
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            
            return response;
        }
    }

    public class VirtualFileSystem : Microsoft.Owin.FileSystems.IFileSystem
    {
        private readonly Microsoft.Owin.FileSystems.PhysicalFileSystem _physicalFileSystem;
        
        public VirtualFileSystem(string path)
        {
            _physicalFileSystem = new Microsoft.Owin.FileSystems.PhysicalFileSystem(path);
        }

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            return _physicalFileSystem.TryGetDirectoryContents(subpath, out contents);
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            return _physicalFileSystem.TryGetFileInfo(subpath, out fileInfo);
        }
    }


    
}
