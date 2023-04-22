using BHP.Controllers.Authentication;
using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BHP.Controllers.Configurations
{
    [Authorize]
    public class ZonalOfficesController : Controller
    {
        private readonly BHP_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public ZonalOfficesController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        
        
        // GET: ZonalOffices
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.ZonalOffice.ToListAsync());
        }

        // getting zonal office
        public JsonResult GetZonalOffice()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getFieldOffice = from c in _context.ZonalOffice
                                 where c.DeleteStatus == false
                                 select new
                                 {
                                     ZoneId = c.ZoneId,
                                     ZoneName = c.ZoneName,
                                     UpdatedAt = c.UpdatedAt.ToString(),
                                     CreatedAt = c.CreatedAt.ToString()
                                 };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getFieldOffice = sortColumn == "zoneName" ? getFieldOffice.OrderByDescending(c => c.ZoneName) :
                               sortColumn == "updatedAt" ? getFieldOffice.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getFieldOffice.OrderByDescending(c => c.CreatedAt) :
                               getFieldOffice.OrderByDescending(c => c.ZoneId + " " + sortColumnDir);
                }
                else
                {
                    getFieldOffice = sortColumn == "zoneName" ? getFieldOffice.OrderBy(c => c.ZoneName) :
                               sortColumn == "updatedAt" ? getFieldOffice.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getFieldOffice.OrderBy(c => c.CreatedAt) :
                               getFieldOffice.OrderBy(c => c.ZoneId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getFieldOffice = getFieldOffice.Where(c => c.ZoneName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getFieldOffice.Count();
            var data = getFieldOffice.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying all zonal offices",  helpers.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        
            

        // POST: ZonalOffices/Create
       // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateZonalOffice(string ZoneName)
        {
            string response = "";

            var country = from s in _context.ZonalOffice
                          where s.ZoneName == ZoneName.ToUpper() && s.DeleteStatus == false
                          select s;

            if (country.Any())
            {
                response = "Zonal Office already exits, please enter another Zonal Office.";
            }
            else
            {
                ZonalOffice _zonalOffice = new ZonalOffice()
                {
                    ZoneName = ZoneName.ToUpper(),
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.ZonalOffice.Add(_zonalOffice);

                int ZoneCreated = await _context.SaveChangesAsync();

                if (ZoneCreated > 0)
                {
                    response = "Zone Created";
                }
                else
                {
                    response = "Something went wrong trying to create this Zonal office. Please try again.";
                }
            }

            helpers.LogMessages("Creating new Zonal Office. Status : " + response + " Zonal Office Name : " + ZoneName,  helpers.getSessionEmail());

            return Json(response);
        }

        

        // POST: ZonalOffices/Edit/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditZonalOffice(int ZonalOfficeId, string ZoneName)
        {
            string response = "";

            var getZonalOffice = from c in _context.ZonalOffice where c.ZoneId == ZonalOfficeId select c;

            getZonalOffice.FirstOrDefault().ZoneName = ZoneName.ToUpper();
            getZonalOffice.FirstOrDefault().UpdatedAt = DateTime.Now;
            getZonalOffice.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Zone Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating Zonal Office. Status : " + response + " Zonal Office ID : " + ZonalOfficeId,  helpers.getSessionEmail());

            return Json(response);
        }

       

        // POST: ZonalOffices/Delete/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteZonalOffice(int ZonalOfficeId)
        {
            string response = "";

            var getZones = from c in _context.ZonalOffice where c.ZoneId == ZonalOfficeId select c;

            getZones.FirstOrDefault().DeletedAt = DateTime.Now;
            getZones.FirstOrDefault().UpdatedAt = DateTime.Now;
            getZones.FirstOrDefault().DeleteStatus = true;
            getZones.FirstOrDefault().DeletedBy = helpers.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Zone Deleted";
            }
            else
            {
                response = "Zonal office not deleted. Something went wrong trying to delete this zonal Office.";
            }

            helpers.LogMessages("Deleting Zonal Office. Status : " + response + " Zonal Office ID : " + ZonalOfficeId,  helpers.getSessionEmail());

            return Json(response);
        }


        private bool ZonalOfficeExists(int id)
        {
            return _context.ZonalOffice.Any(e => e.ZoneId == id);
        }
    }
}
