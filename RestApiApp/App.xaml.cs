using System;
using Prism.Ioc;
using Prism.Unity;
using RestApiApp.Services.Json;
using RestApiApp.Services.Rest;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RestApiApp
{
    public partial class App : PrismApplication
    {
        public static T Resolve<T>()
        {
            return (Current as App).Container.Resolve<T>();
        }

        public App()
            : this(null)
        {
        }

        public App(Prism.IPlatformInitializer initializer = null)
            : base(initializer)
        {
        }

        #region -- Overrides --

        protected override void OnInitialized()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IJsonService>(Container.Resolve<JsonService>());
            containerRegistry.RegisterInstance<IRestService>(Container.Resolve<RestService>());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        #endregion
    }
}
