using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using BHP.Models.DB;
using BHP.Helpers;
using BHP.HelpersClass;

namespace BHP.Controllers.Configurations
{
    [Authorize]
    public class LocationsController : Controller
    {
        private readonly BHP_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public LocationsController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        // GET: Locations
       // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Location.ToListAsync());
        }


        // Get all locations
        public JsonResult GetLocations()
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

            var getCountries = from c in _context.Location
                               join s in _context.Staff on c.CreatedBy equals s.StaffId into staff
                               join ss in _context.Staff on c.UpdatedBy equals ss.StaffId into staffs
                               where c.DeleteStatus == false
                               select new
                               {
                                   LocationId = c.LocationId,
                                   LocationName = c.LocationName,
                                   UpdatedAt = c.UpdatedAt.ToString(),
                                   CreatedAt = c.CreatedAt.ToString(),
                                   CreatedBy = staff.FirstOrDefault().FirstName + " " + staff.FirstOrDefault().LastName,
                                   UpdatedBy = staffs.FirstOrDefault().FirstName + " " + staffs.FirstOrDefault().LastName,
                               };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getCountries = sortColumn == "locationName" ? getCountries.OrderByDescending(c => c.LocationName) :
                               sortColumn == "updatedAt" ? getCountries.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getCountries.OrderByDescending(c => c.CreatedAt) :
                               getCountries.OrderByDescending(c => c.LocationId + " " + sortColumnDir);
                }
                else
                {
                    getCountries = sortColumn == "locationName" ? getCountries.OrderBy(c => c.LocationName) :
                               sortColumn == "updatedAt" ? getCountries.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getCountries.OrderBy(c => c.CreatedAt) :
                               getCountries.OrderBy(c => c.LocationId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getCountries = getCountries.Where(c => c.LocationName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getCountries.Count();
            var data = getCountries.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying all locations",  helpers.getSessionEmail());
            
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        //create Locatioin
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateLocation(string Location)
        {
            string response = "";

            var country = from c in _context.Location
                          where c.LocationName == Location.ToUpper() && c.DeleteStatus == false
                          select c;

            if (country.Any())
            {
                response = "Location already exits, please enter another location.";
            }
            else
            {
                Location loc = new Location()
                {
                    LocationName = Location.ToUpper(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = helpers.getSessionUserID(),
                    DeleteStatus = false
                };

                _context.Location.Add(loc);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "Location Created";
                }
                else
                {
                    response = "Something went wrong trying to create location. Please try again.";
                }
            }

            helpers.LogMessages("Creating new location. Status : " + response + " Location name : " + Location,  helpers.getSessionEmail());

            return Json(response);
        }



       // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditLocation(int LocationId, string Location)
        {
            string response = "";
            var get = from c in _context.Location where c.LocationId == LocationId select c;

            get.FirstOrDefault().LocationName = Location.ToUpper();
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedBy = helpers.getSessionUserID();
            get.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Location Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating Location. Status : " + response + " Location ID : " + LocationId,  helpers.getSessionEmail());

            return Json(response);
        }




        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> RemoveLocation(int LocationID)
        {
            string response = "";

            var get = from c in _context.Location where c.LocationId == LocationID select c;

            get.FirstOrDefault().DeletedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedBy = helpers.getSessionUserID();
            get.FirstOrDefault().DeleteStatus = true;
            get.FirstOrDefault().DeletedBy = helpers.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Location Removed";
            }
            else
            {
                response = "Location not deleted. Something went wrong trying to delete this Location.";
            }

            helpers.LogMessages("Deleting Location. Status : " + response + " Location ID : " + LocationID,  helpers.getSessionEmail());

            return Json(response);

        }


        private bool LocationExists(int id)
        {
            return _context.Location.Any(e => e.LocationId == id);
        }
    }
}
