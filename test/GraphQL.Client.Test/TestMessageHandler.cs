using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Test
{
    public class TestMessageHandler : HttpMessageHandler
    {
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string> Content { get; set; }
        public string LastRequestContent { get; set; }

        public TestMessageHandler()
        {
            Content = new Dictionary<string, string>();
            StatusCode = HttpStatusCode.OK;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestContent = request.Content.ReadAsStringAsync().Result;
            string content = null;
            foreach (var pair in Content)
            {
                if (LastRequestContent.Contains(pair.Key))
                {
                    content = pair.Value;
                    break;
                }
            }
            return Task.FromResult(new HttpResponseMessage(StatusCode) {Content = new StringContent(content)});
        }

        public void SetContent(string key, string content)
        {
            Content[key] = content;
        }
    }
}
