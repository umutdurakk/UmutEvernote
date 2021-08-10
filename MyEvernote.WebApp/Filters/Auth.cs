using MyEvernote.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyEvernote.WebApp.Filters
{
    public class Auth:FilterAttribute,IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext FilterContext)
        {
            if (CurrentSession.User==null)
            {
                FilterContext.Result = new RedirectResult("/Home/Login");
            }
        }
    }
}