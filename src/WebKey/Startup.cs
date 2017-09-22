using Android.Widget;
using Autofac;
using WebKey.Adapters;
using WebKey.Support;
using WebKey.ViewModels;

namespace WebKey
{
    public class Startup
    {
        public static IContainer Container { get; set; }
        private static bool Initialized { get; set; }

        public static void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;

                var builder = new ContainerBuilder();

                builder.RegisterType<DataProvider>().As<IDataProvider>();
                builder.RegisterType<HomeListViewModel>().As<IHomeListViewModel>().SingleInstance();
                builder.RegisterType<EncryptDecrypt>().As<IEncryptDecrypt>().SingleInstance();

                Container = builder.Build();
            }
        }
    }
}