using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.Http;
using GraphQL.Client;
using GraphQL.Server.Sample.Maps;
using GraphQL.Server.Sample.Objects;
using GraphQL.Server.Sample.Operations;
using GraphQL.Server.Sample.Output;
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

            //Graph Schema
            container.RegisterSingleton<ApiSchema>(() =>
            {
                var apiSchema = new ApiSchema(container);
                apiSchema.AddPropertyFilter<string>((context, propertyInfo, name, value) =>
                {
                    Debug.WriteLine($"PropertyFilter for {name}");
                    return value;
                });
                apiSchema.AddPropertyFilter<Uri>((context, propertyInfo, name, value) =>
                {
                    Debug.WriteLine($"Replacing host for Uri {value}");
                    var builder = new UriBuilder(value)
                    {
                        Host = "www.replacement.com"
                    };
                    return builder.Uri;
                });
                apiSchema.AddPropertyFilter((context, propertyInfo, name, value) =>
                {
                    Debug.WriteLine($"Generic property filter");
                    return value;
                });
                // map a type without GraphObject implementation
                //apiSchema.MapOutput<Robot, Output.RobotOutput>();
                apiSchema.MapOutput<RobotOutput>();

                // map a type with a type mapping
                apiSchema.MapOutput<Lego, LegoMap>();

                // map an operation without IOperation implementation
                apiSchema.Proxy<ILegoServiceOperation>(() => new GraphClient<ILegoServiceOperation>("http://localhost:51436/api", new System.Net.Http.HttpClient()));

                apiSchema.MapAssemblies(Assembly.GetAssembly(typeof(HumanObject)));
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