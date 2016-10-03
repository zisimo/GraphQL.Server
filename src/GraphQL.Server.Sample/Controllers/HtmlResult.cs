using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace GraphQL.Server.Sample.Controllers
{
    public class HtmlResult : IHttpActionResult
    {
        public string HtmlString { get; set; }
        private HttpRequestMessage _request;

        public HtmlResult(string htmlString, HttpRequestMessage request)
        {
            HtmlString = htmlString;
            _request = request;
        }
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                RequestMessage = _request,
                Content = new StringContent(HtmlString, Encoding.ASCII, "text/html")
            };
            return Task.FromResult(response);
        }
    }
}