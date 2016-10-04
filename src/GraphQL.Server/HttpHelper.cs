using System.Web;

namespace GraphQL.Server
{
    public interface IHttpAdapter
    {
        string UserHostAddress { get; }

        string GetHeader(string name);
    }
    

    public class HttpAdapter : IHttpAdapter
    {
        public string UserHostAddress => HttpContext.Current.Request.UserHostAddress;

        public string GetHeader(string name)
        {
            return HttpContext.Current.Request.Headers[name];
        }
    }
}