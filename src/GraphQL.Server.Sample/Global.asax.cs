using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using GraphQL.Client;
using GraphQL.Server.Sample.Interface.Lego;
using GraphQL.Server.Sample.Objects;
using GraphQL.Server.Sample.Output;
using GraphQL.Server.Sample.Repository;
using GraphQL.Server.Security;
using GraphQL.Server.SimpleInjector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;

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

            //Graph Schema
            container.Register<ResolverInfoManager>(Lifestyle.Scoped);
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

                // map a type with a type mapping
                //apiSchema.MapOutput<Lego, LegoMap>();
                apiSchema.MapAssemblies(Assembly.GetAssembly(typeof(HumanObject)));

                // map a type without GraphObject implementation
                //apiSchema.MapOutput<Robot, Output.RobotOutput>();
                apiSchema.MapOutput<RobotOutput>(autoMapChildren: true, overwriteMap: true);

                // map an operation without IOperation implementation
                var proxy = apiSchema.Proxy<ILegoOperation>(() => new GraphClient("http://localhost:51365/api", new HttpClient()));
                //proxy.AddPostOperation(nameof(ILegoOperation.Lego), (context, name, value) =>
                //{
                //    Debug.WriteLine($"PostOperation for {name}");
                //    return value;
                //});
                proxy.AddPostOperation(i => nameof(i.Lego), (context, name, value) =>
                {
                    Debug.WriteLine($"PostOperation for {name}");
                    return value;
                });
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