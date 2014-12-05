using System;
using System.Diagnostics;
using System.Web.Mvc;
using Microsoft.SharePoint.Client;
using SharePointAppWeb.Filters;
using SharePointAppWeb.Services;

namespace SharePointAppWeb.Controllers
{
    public class HomeController : Controller
    {
        [SharePointContextFilter]
        public ActionResult Index()
        {
            Trace.WriteLine("Trying to obtain a token");

            SharePointContext spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            if (Request.QueryString.Get("SessionId") == null)
            {
                ViewBag.SPHostUrl = spContext.SPHostUrl;
                return View();
            }

            using (ClientContext clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext == null)
                {
                    Trace.WriteLine("Not able to obtain client context.");
                    throw new Exception("Unable to create Client Context");
                }

                string sessionId = Guid.NewGuid().ToString();

                ViewBag.SessionId = sessionId;

                MainService.UserSessions.Add(sessionId, spContext.UserAccessTokenForSPHost);
                Trace.WriteLine("Creating session id " + sessionId);
                return View("SessionId");
            }
        }
    }
}