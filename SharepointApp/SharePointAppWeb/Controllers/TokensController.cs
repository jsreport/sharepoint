using System.Diagnostics;
using System.Web.Http;
using SharePointAppWeb.Services;

namespace SharePointAppWeb.Controllers
{
    public class TokensController : ApiController
    {
        // POST api/<controller>
        public string Post()
        {
            var sessionId = Request.Content.ReadAsStringAsync().Result;
            Trace.WriteLine("Session id is " + sessionId);
            return MainService.UserSessions[sessionId];
        }
    }
}