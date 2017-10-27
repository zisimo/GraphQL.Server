using System.Web.Http;
using GraphQL.Server.Sample.Interface.Lego;
using GraphQL.Server.Sample.Service.Operations;
using GraphQL.Server.Sample.Service.Repository;
using GraphQL.Server.Security;
using GraphQL.Server.SimpleInjector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;
using Formatting = Newtonsoft.Json.Formatting;

namespace GraphQL.Server.Sample
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var container = new GraphQLContainer();
            container.Options.AllowOverridingRegistrations = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Register<IContainer>(() => container);
            var data = new Data();
            container.Register<Data>(() => data);
            var authorizationMap = new AuthorizationMap() { AllowMissingAuthorizations = true };
            container.Register<AuthorizationMap>(() => authorizationMap);
            container.Register<ILegoOperation, LegoOperation>();

            //Graph Schema
            container.RegisterSingleton<ApiSchema>(() =>
            {
                var apiSchema = new ApiSchema(container);

                apiSchema.MapOperation<LegoOperation>();

                apiSchema.Lock();
                return apiSchema;
            });
            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.MapHttpAttributeRoutes();
            GlobalConfiguration.Configuration.EnsureInitialized();

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
        }
    }
}