using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Library.Interfaces;
using Autodesk.Revit.UI;
using Core.Services;
using Library.Models;
using RoomStudies.Models;
using Autodesk.Revit.DB.Architecture;

namespace Core
{
    /// <summary>
    ///     Provides a host for the application's services and manages their lifetimes
    /// </summary>
    public static class Host
    {
        private static IHost _host;

        /// <summary>
        ///     Starts the host and configures the application's services
        /// </summary>
        public static void Start()
        {
            var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings
            {
                ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location),
                DisableDefaults = true
            });


            builder.Services.AddTransient<Func<Room, RoomStudyModel>>(provider => (room) =>
            {
                return new RoomStudyModel(room);
            });


            builder.Services.AddTransient<IMainModel<Room, RoomStudyModel>, MainModel>();
            builder.Services.AddTransient<MainModel>();

            _host = builder.Build();
            _host.Start();
        }

        /// <summary>
        ///     Stops the host and handle <see cref="IHostedService"/> services
        /// </summary>
        public static void Stop()
        {
            _host.StopAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Get service of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of service object to get</typeparam>
        /// <exception cref="System.InvalidOperationException">There is no service of type <typeparamref name="T"/></exception>
        public static T GetService<T>() where T : class
        {
            return _host.Services.GetRequiredService<T>();
        }
    }
}
