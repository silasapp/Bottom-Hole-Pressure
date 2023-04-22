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
    public class FieldOfficesController : Controller
    {
        private readonly BHP_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public FieldOfficesController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        // GET: FieldOffices
       // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.FieldOffices.ToListAsync());
        }
        


        public JsonResult GetFieldOffice()
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

            var getFieldOffice = from c in _context.FieldOffices
                                 where c.DeleteStatus == false
                                 select new
                                 {
                                     FieldOfficeID = c.FieldOfficeId,
                                     OfficeName = c.OfficeName,
                                     UpdatedAt = c.UpdatedAt.ToString(),
                                     CreatedAt = c.CreatedAt.ToString()
                                 };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getFieldOffice = sortColumn == "officeName" ? getFieldOffice.OrderByDescending(c => c.OfficeName) :
                               sortColumn == "updatedAt" ? getFieldOffice.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getFieldOffice.OrderByDescending(c => c.CreatedAt) :
                               getFieldOffice.OrderByDescending(c => c.FieldOfficeID + " " + sortColumnDir);
                }
                else
                {
                    getFieldOffice = sortColumn == "officeName" ? getFieldOffice.OrderBy(c => c.OfficeName) :
                               sortColumn == "updatedAt" ? getFieldOffice.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getFieldOffice.OrderBy(c => c.CreatedAt) :
                               getFieldOffice.OrderBy(c => c.FieldOfficeID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getFieldOffice = getFieldOffice.Where(c => c.OfficeName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getFieldOffice.Count();
            var data = getFieldOffice.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying all field officess...",  helpers.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        // POST: FieldOffices/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateFieldOffice(string OfficeName)
        {
            string response = "";

            var country = from s in _context.FieldOffices
                          where s.OfficeName == OfficeName.ToUpper() && s.DeleteStatus == false
                          select s;

            if (country.Any())
            {
                response = "Field Office already exits, please enter another Field Office.";
            }
            else
            {
                FieldOffices _fieldOffice = new FieldOffices()
                {
                    OfficeName = OfficeName.ToUpper(),
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.FieldOffices.Add(_fieldOffice);
                int OfficeCreated = await _context.SaveChangesAsync();

                if (OfficeCreated > 0)
                {
                    response = "Office Created";
                }
                else
                {
                    response = "Something went wrong trying to create this field office. Please try again.";
                }
            }

            helpers.LogMessages("Creating new field office. Status : " + response + " field office name : " + OfficeName,  helpers.getSessionEmail());

            return Json(response);

        }

        // GET: FieldOffices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fieldOffices = await _context.FieldOffices.FindAsync(id);
            if (fieldOffices == null)
            {
                return NotFound();
            }
            return View(fieldOffices);
        }



        /*
         * Edit field Office
         */
       // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditFieldOffice(string OfficeName, int FieldOfficeId)
        {
            string response = "";

            var getFieldOffice = from c in _context.FieldOffices where c.FieldOfficeId == FieldOfficeId select c;

            getFieldOffice.FirstOrDefault().OfficeName = OfficeName.ToUpper();
            getFieldOffice.FirstOrDefault().UpdatedAt = DateTime.Now;
            getFieldOffice.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Office Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating field office. Status : " + response + " field office ID: " + FieldOfficeId,  helpers.getSessionEmail());

            return Json(response);
        }




        // GET: FieldOffices/Delete/
       // [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteOffice(int FieldOfficeID)
        {
            string response = "";

            var getState = from c in _context.FieldOffices where c.FieldOfficeId == FieldOfficeID select c;

            getState.FirstOrDefault().DeletedAt = DateTime.Now;
            getState.FirstOrDefault().UpdatedAt = DateTime.Now;
            getState.FirstOrDefault().DeleteStatus = true;
            getState.FirstOrDefault().DeletedBy = helpers.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Office Deleted";
            }
            else
            {
                response = "Office not deleted. Something went wrong trying to delete this Field Office.";
            }

            helpers.LogMessages("Deleting field office. Status : " + response + " field office ID : " + FieldOfficeID,  helpers.getSessionEmail());

            return Json(response);
        }
    

        // POST: FieldOffices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fieldOffices = await _context.FieldOffices.FindAsync(id);
            _context.FieldOffices.Remove(fieldOffices);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FieldOfficesExists(int id)
        {
            return _context.FieldOffices.Any(e => e.FieldOfficeId == id);
        }
    }
}
