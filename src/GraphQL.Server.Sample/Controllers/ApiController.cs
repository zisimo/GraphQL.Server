using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GraphQL.Execution;
using GraphQL.Types;
using GraphQL.Validation.Complexity;

namespace GraphQL.Server.Sample.Controllers
{
    [RoutePrefix("api")]
    public class ApiController : System.Web.Http.ApiController
    {
        private bool DisplayStackTrace = true;

        public IContainer Container { get; set; }
        public DocumentExecuter Executer { get; set; }
        public ISchema Schema { get; set; }

        public ApiController(IContainer container)
        {
            Container = container;
            Executer = new DocumentExecuter(new GraphQLDocumentBuilder(), new ApiValidator(container), new ComplexityAnalyzer());
            Schema = Container.GetInstance<ApiSchema>();
        }

        [Route("")]
        [HttpPost]
        public async Task<GraphQLOutput> Query(GraphQLQuery query)
        {
            GraphQLOutput output = null;
            try
            {
                var executionResult = await Executer.ExecuteAsync(Schema, null, query.Query, null, query.GetInputs());
                output = new GraphQLOutput(executionResult.Data, executionResult.Errors?.ToArray());
            }
            catch (Exception ex)
            {
                output = new GraphQLOutput(null, new[] { new ExecutionError("Controller exception", ex), });
            }
            if (!DisplayStackTrace && output.Errors != null)
            {
                foreach (var error in output.Errors)
                {
                    error.StackTrace = null;
                }
            }
            return output;
        }

        [Route("")]
        [HttpGet]
        public IHttpActionResult Default()
        {
            var content = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\public\\index.html"));
            return new HtmlResult(content, Request);
        }
    }
}
