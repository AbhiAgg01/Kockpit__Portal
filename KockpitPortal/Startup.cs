using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KockpitPortal.Service;
using KockpitPortal.Utility;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace KockpitPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddMvc();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(120);
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"D:\IIS\Kockpit"))
                .SetApplicationName("SharedCookieApp");

            services.AddAuthentication("Identity.Application")
                .AddCookie("Identity.Application", options =>
                {
                    options.Cookie.Name = ".AspNet.SharedCookie";
                });

            services.AddControllers();

            //to run the background service
            services.AddScoped<IMyScopedService, ScopedService>();
            services.AddCronJob<Agent>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"*/1 * * * *";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();
            var wsOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(wsOptions);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var session = context.Request.Query["session"].ToString();
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var webSocketClient = WebSocketClientCollection.Add(session, webSocket, new Dictionary<string, string>());
                        await Echo(context, webSocketClient);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Index}/{id?}");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private async Task Echo(HttpContext context, WebSocketClient webSocketClient)
        {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult wsresult = await webSocketClient.SocketConnection.ReceiveAsync(buffer, CancellationToken.None);

            while (!wsresult.CloseStatus.HasValue)
            {
                buffer = new ArraySegment<byte>(new Byte[8192]);

                WebSocketReceiveResult result = null;

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await webSocketClient.SocketConnection.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            var ReceivedMessage = reader.ReadToEnd().Trim();
                            //code to decrypt the receiving message
                            ReceivedMessage = Secure.Decryptdata(ReceivedMessage);

                            var ReceivingResult = JsonConvert.DeserializeObject<RecievingResult>(ReceivedMessage.Trim());
                            webSocketClient.ResultQueue[ReceivingResult.RequestId] = (ReceivingResult.ErrorCode == 0 ? "Error: " : "") + ReceivingResult.Result;

                        }
                    }
                }
            }
            await webSocketClient.SocketConnection.CloseAsync(wsresult.CloseStatus.Value, wsresult.CloseStatusDescription,
                CancellationToken.None);
        }
    }
}
