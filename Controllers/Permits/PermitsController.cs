using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BHP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using BHP.HelpersClass;
using BHP.Helpers;
using BHP.Models;
using System.IO;
using System.DrawingCore;
using QRCoder;
using Microsoft.AspNetCore.Authorization;
using Rotativa.AspNetCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace BHP.Controllers.Permits
{
    [Authorize]
    public class PermitsController : Controller
    {
        private readonly BHP_DBContext _context;
       
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();
        RestSharpServices _restService = new RestSharpServices();


        public PermitsController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



        // GET: Permits
        public IActionResult Index(string id, string option)
        {
            int ids = 0;

            var type = id;
            var general_id = generalClass.Decrypt(option);

            var myPermits = from p in _context.Permits
                            join a in _context.RequestProposal on p.RequestId equals a.RequestId
                            join c in _context.Companies on a.CompanyId equals c.CompanyId
                            select new MyPermit
                            {
                                PermitID = p.PermitId,
                                PermitNo = p.PermitNo,
                                RefNo = a.RequestRefNo,
                                IssuedDate = p.IssuedDate,
                                ExpireDate = p.ExpireDate,
                                isPrinted = p.Printed,
                                CompanyName = c.CompanyName,
                                CompanyID = c.CompanyId,
                                CompanyEmail = c.CompanyEmail
                            };

            ViewData["ClassifyPermits"] = "All Permits";

            if (!string.IsNullOrWhiteSpace(id) || !string.IsNullOrWhiteSpace(option))
            {
                if (id == "_all")
                {
                    myPermits = myPermits.Where(x => x.PermitID > 0 /*&& x.isLegacy == ""*/);
                }
                else if (id == "_printed")
                {
                    myPermits = myPermits.Where(x => x.isPrinted == true /*&& x.isLegacy == ""*/);

                    ViewData["ClassifyPermits"] = "All Printed Permits";
                }
                else if (id == "_notprinted")
                {
                    myPermits = myPermits.Where(x => x.isPrinted == false /*&& x.isLegacy == ""*/);

                    ViewData["ClassifyPermits"] = "All Not Printed Permits";
                }
                else if (id == "_company")
                {
                    ids = Convert.ToInt32(general_id);
                    myPermits = myPermits.Where(x => x.CompanyID == ids);

                    ViewData["ClassifyPermits"] = "All Permits for " + myPermits.FirstOrDefault()?.CompanyName + " Company";
                }
            }

            _helpersController.LogMessages("Displaying " + ViewData["ClassifyPermits"],  _helpersController.getSessionEmail());

            return View(myPermits.ToList());
        }




        public IActionResult ViewPermit(string id, string option)
        {
            int permitID = generalClass.DecryptIDs(id);

            List<PermitViewModel> permitViewModels = new List<PermitViewModel>();

            if (permitID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Permit reference not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                var Host = Request.Host;
                var absolutUrl = Host + "/Permits/VerifyPermitQrCode/" + id;
                var QrCode = GenerateQR(absolutUrl);

                var getPermits = from p in _context.Permits.AsEnumerable()
                                 join a in _context.RequestProposal.AsEnumerable() on p.RequestId equals a.RequestId
                                 join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                                 join tr in _context.Transactions.AsEnumerable() on a.RequestId equals tr.RequestId into trans
                                 from t in trans.DefaultIfEmpty()
                                 join sf in _context.Staff on p.ApprovedBy equals sf.StaffId into staff
                                 from s in staff.DefaultIfEmpty()
                                 where p.PermitId == permitID
                                 select new PermitModel
                                 {
                                     PermitID = p.PermitId,
                                     RequestID = a.RequestId,
                                     RefNo = a.GeneratedNumber,
                                     PermitNO = p.PermitNo,
                                     DateApplied = a.DateApplied,
                                     IssuedDate = p.IssuedDate.ToString("dd MMMM, yyyy"),
                                     ExpiaryDate = p.ExpireDate.ToString("dddd, dd MMMM yyyy"),
                                     CompanyID = a.CompanyId,
                                     CompanyName = c.CompanyName,
                                     CompanyAddress = c.Address,
                                     CompanyCity = c.City,
                                     CompanyState = c.StateName,
                                     StaffName = s.LastName + " " + s.FirstName,
                                     Signature = staff.FirstOrDefault()?.SignatureName == null ? "" : staff.FirstOrDefault()?.SignatureName,
                                     QrCode = QrCode,
                                     TotalAmount = t == null ? 0 : (int)t.TotalAmt,
                                    
                                 };

                if (getPermits.Any())
                {
                    var get = (from a in _context.Application.AsEnumerable()
                               where a.RequestId == getPermits.FirstOrDefault().RequestID
                               select new
                               {
                                   a
                               }).GroupBy(x => new
                               {
                                   FieldName = x.a.WellName
                               }).Select(c => new wells
                               {
                                   FieldName = c.Key.FieldName.ToString(),
                                   Well = c.FirstOrDefault().a.WellName,
                                   Count = c.Count()
                               });

                    var wells_info = "";

                    List<string> wellInfos = new List<string>();

                    int totalwell = 0;

                    foreach(var w in get.ToList())
                    {
                        wells_info = " " + w.Count + " wells in your " + w.FieldName + " field ";
                        wellInfos.Add(wells_info);
                        totalwell += w.Count;
                    }

                    var nominate = from n in _context.NominatedStaff
                                   join s in _context.Staff on n.StaffId equals s.StaffId
                                   join l in _context.Location on s.LocationId equals l.LocationId
                                   where n.RequestId == getPermits.FirstOrDefault().RequestID
                                   select new Nomination
                                   {
                                       StaffEmail = s.StaffEmail,
                                       StaffName = s.LastName + " " + s.FirstName,
                                       Designation = n.Designation,
                                       PhoneNumber = n.PhoneNumber,
                                       Location = l.LocationName
                                   };

                    permitViewModels.Add(new PermitViewModel
                    {
                        permitModels = getPermits.ToList(),
                        wells = get.ToList(),
                        infos = wellInfos,
                        totalWell = totalwell,
                        nominations = nominate.ToList()
                    }); 

                    ViewData["PermitDocumentName"] = "Permit for " + getPermits.FirstOrDefault().CompanyName + " (" + getPermits.FirstOrDefault().PermitNO + ").pdf";

                    if (option == "_view")
                    {
                        _helpersController.SavePermitHistory(permitID, "Preview");

                        _helpersController.LogMessages("Displaying " + ViewData["PermitDocumentName"],  _helpersController.getSessionEmail());

                        return new ViewAsPdf("ViewPermit", permitViewModels.ToList())
                        {
                            PageSize = Rotativa.AspNetCore.Options.Size.A4,
                        };
                    }
                    else if (option == "_download")
                    {
                        var fetchPermits = _context.Permits.Where(x => x.PermitId == permitID);

                        fetchPermits.FirstOrDefault().Printed = true;
                        _context.SaveChanges();

                        _helpersController.SavePermitHistory(permitID, "Download");

                        _helpersController.LogMessages("Downloading " + ViewData["PermitDocumentName"],  _helpersController.getSessionEmail());

                        return new ViewAsPdf("ViewPermit", permitViewModels.ToList())
                        {
                            PageSize = Rotativa.AspNetCore.Options.Size.A4,
                            FileName = ViewData["PermitDocumentName"].ToString()
                        };
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps! What did you just do now??? Please click the back button.") });
                    }
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Could not find permit. Kindly contact support.") });
                }
            }
        }






        public IActionResult ViewHistory(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Suitability reference is empty or not in correct format. Kindly contact support.") });
            }

            int permitID = 0;

            var permit = generalClass.Decrypt(id);

            if (permit == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Suitability reference error or not in correct format. Kindly contact support.") });
            }
            else
            {
                permitID = Convert.ToInt32(permit);

                var getPermitHistory = from ph in _context.PermitHistory
                                       join p in _context.Permits on ph.PermitId equals p.PermitId
                                       where ph.PermitId == permitID
                                       select new PermitView
                                       {
                                           PermitNO = p.PermitNo,
                                           ViewType = ph.ViewType,
                                           PreviewedAt = ph.PreviewedAt,
                                           DownloadedAt = ph.DownloadedAt,
                                           UserDetails = ph.UserDetails
                                       };

                ViewData["PermitHistoryTitle"] = "Permit History Details";

                if (getPermitHistory.Any())
                {
                    ViewData["PermitHistoryTitle"] = "Permit History Details for : " + getPermitHistory.FirstOrDefault()?.PermitNO;
                }

                return View(getPermitHistory.ToList());
            }
        }




        [AllowAnonymous]
        public IActionResult VerifyPermitQrCode(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Permit not found or not in correct format. Kindly contact support.") });
            }

            int permitID = 0;

            var permit = generalClass.Decrypt(id);

            if (permit == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Permit not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                permitID = Convert.ToInt32(permit);

                var prm = from p in _context.Permits.AsEnumerable()
                          join a in _context.RequestProposal.AsEnumerable() on p.RequestId equals a.RequestId
                          join d in _context.RequestDuration on a.DurationId equals d.DurationId
                          join c in _context.Companies.AsEnumerable() on a.CompanyId equals c.CompanyId
                          join t in _context.Transactions.AsEnumerable() on a.RequestId equals t.RequestId
                          where p.PermitId == permitID
                          select new PermitModel
                          {
                              RefNo = a.RequestRefNo,
                              GeneratedRef = a.GeneratedNumber,
                              PermitNO = p.PermitNo,
                              Year = d.ProposalYear,
                              IssuedDate = p.IssuedDate.ToString("dd MMMM, yyyy"),
                              ExpiaryDate = p.ExpireDate.ToString("dddd, dd MMMM yyyy"),
                              MyCompanyDetails = c.CompanyName + " (" + c.Address + ", " + c.City + ", " + c.StateName + ")",
                              TotalWell = _context.Application.Where(x => x.RequestId == a.RequestId).GroupBy(x => x.WellName).Count(),
                              TotalReservior = _context.Application.Where(x => x.RequestId == a.RequestId).GroupBy(x => x.Reservoir).Count(),
                              TotalField = _context.Application.Where(x => x.RequestId == a.RequestId).GroupBy(x => x.Fields).Count(),
                              TotalAmount = (int)t.TotalAmt,
                              PayDescriiption = t.Description,
                              Status = a.Status,
                              DateApplied = a.DateApplied,
                              RRR = t.Rrr,
                              CompanyCode = c.IdentificationCode
                          };

                if (prm.Any())
                {
                    return View(prm.ToList());
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps!!! Something went wrong tyring to verify this Permit or Permit was not found, Kindly contact support.") });
                }
            }
        }




        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.DrawingCore.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }



        private static Byte[] GenerateQR(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var imageResult = BitmapToBytes(qrCodeImage);
            return imageResult;
        }


    }

   
}
