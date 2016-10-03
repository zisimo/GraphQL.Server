using System.IO;
using System.Web;
using System.Web.Http;

namespace GraphQL.Server.Sample.Controllers
{
    [RoutePrefix("")]
    public class HomeController : System.Web.Http.ApiController
    {
        [Route("")]
        [HttpGet]
        public IHttpActionResult Default()
        {
            var content = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\public\\index.html"));
            return new HtmlResult(content, Request);
        }
    }
}