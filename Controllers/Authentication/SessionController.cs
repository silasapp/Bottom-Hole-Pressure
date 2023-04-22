using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BHP.Controllers.Authentication
{
    public class SessionController : Controller
    {

        private readonly BHP_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();
        RestSharpServices _restService = new RestSharpServices();


        public SessionController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        /*
      * To check if user is still logged in
      */
        public JsonResult CheckSession()
        {
            try
            {
                var session = _helpersController.getSessionEmail();

                string result = "";

                if (session == null || session == "Error" || session == "")
                {
                    result = "true";
                }
                return Json(result);
            }
            catch (Exception)
            {
                return Json("true");
            }
        }

    }
}
