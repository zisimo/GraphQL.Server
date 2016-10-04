using System.Reflection;
using System.Web.Http;
using GraphQL.Server.Sample.Operations;
using GraphQL.Server.Sample.Repository;
using GraphQL.Server.Security;
using GraphQL.Server.SimpleInjector;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace GraphQL.Server.Sample
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var container = new GraphQLContainer();
            container.Options.AllowOverridingRegistrations = false;
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Register<IContainer>(() => container);
            var data = new Data();
            container.Register<Data>(() => data);
            var authorizationMap = new AuthorizationMap() { AllowMissingAuthorizations = true };
            container.Register<AuthorizationMap>(() => authorizationMap);
            container.Register<AllOperations>();
            
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