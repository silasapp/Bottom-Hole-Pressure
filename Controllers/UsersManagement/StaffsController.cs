using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using BHP.HelpersClass;
using BHP.Helpers;
using BHP.Models.DB;
using BHP.Controllers.Authentication;

namespace BHP.Controllers.UsersManagement
{
    [Authorize]
    public class StaffsController : Controller
    {
        private readonly BHP_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();
        RestSharpServices _restService = new RestSharpServices();

        public static int mydeskCount = 0;


        public StaffsController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }





        /*
         * Changing portal theme for a staff.
         * 
         * id => encrypted type of theme (Light or Dark)
         * 
         */
        public JsonResult UseTheme(string id)
        {
            var theme = generalClass.Decrypt(id);
            var result = "";

            if (theme == "Error")
            {
                result = "Opps!!! Something went wrong trying to change your theme, please try again later.";
            }
            else
            {
                var user = _helpersController.getSessionUserID();

                var getUser = _context.Staff.Where(x => x.StaffId == user && x.DeleteStatus == false && x.ActiveStatus == true);

                if (getUser.Any())
                {
                    getUser.FirstOrDefault().Theme = theme;
                    getUser.FirstOrDefault().UpdatedAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {
                        HttpContext.Session.SetString(AuthController.sessionTheme, generalClass.Encrypt(theme));

                        result = "Theme Changed";
                    }
                    else
                    {
                        result = "Opps!!! Something went wrong trying to update your theme, please try again later.";

                    }
                }
                else
                {
                    result = "Opps!!! Something went wrong trying to find your account. Maybe your accout has been deactivated.";
                }
            }

            _helpersController.LogMessages("Staff changing theme to : " + theme + " theme status result => " + result,  _helpersController.getSessionEmail());

            return Json(result);
        }




        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD, PRINTER, DOC MANAGER, SUPERVISOR, INSPECTOR, REVIEWER, TEAM LEAD")]
        public IActionResult Dashboard()
        {
            var processingApps = _context.RequestProposal.Where(x => x.Status == GeneralClass.Processing && x.DeletedStatus == false).Count();
            var totalApps = _context.RequestProposal.Where(x => x.DeletedStatus == false && x.HasAcknowledge == true).Count();
            var totalPermits = (from p in _context.Permits.AsEnumerable()
                                join a in _context.RequestProposal.AsEnumerable() on p.RequestId equals a.RequestId
                                select new
                                {
                                    p
                                }).Count();

            var nom = _context.NominationRequest.Where(x => x.StaffId == _helpersController.getSessionUserID() && x.HasDone == false);

            ViewData["NominationRequest"] = nom.Count();
            ViewData["ProcessingApps"] = processingApps;
            ViewData["TotalApps"] = totalApps;
            ViewData["TotalPermits"] = totalPermits;

            _helpersController.LogMessages("Displaying Staff Dashboard.",  _helpersController.getSessionEmail());

            return View();
        }




        /*
         * Getting the count of application on my desk
         * 
         */
        public JsonResult MyDeskCount()
        {
            var staff = _helpersController.getSessionUserID();

            var mydesk = from md in _context.MyDesk.AsEnumerable()
                         join r in _context.RequestProposal.AsEnumerable() on md.RequestId equals r.RequestId
                         join d in _context.RequestDuration.AsEnumerable() on r.DurationId equals d.DurationId
                         join c in _context.Companies.AsEnumerable() on r.CompanyId equals c.CompanyId
                         orderby md.DeskId descending
                         where ((md.StaffId == staff && md.HasWork == false) && (r.DeletedStatus == false && r.IsSubmitted == true && r.HasAcknowledge == true) && (c.DeleteStatus == false) && (d.DeleteStatus == false))
                         select new
                         {
                             DeskID = md.DeskId,
                         };

            mydeskCount = mydesk.Count();
            return Json(mydesk.Count());
        }





    }
}
