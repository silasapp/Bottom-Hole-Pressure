using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BHP.Helpers;
using BHP.HelpersClass;
using BHP.Models;
using BHP.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QRCoder;
using Rotativa.AspNetCore;

namespace BHP.Controllers.Permits
{
    public class ExternalPermitViewController : Controller
    {
        private readonly BHP_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();

        public ExternalPermitViewController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



        // GET: ExternalPermitView
        public IActionResult ViewPermit(int id)
        {
            var getPermit = from p in _context.Permits
                            join a in _context.RequestProposal on p.RequestId equals a.RequestId
                            where p.PermitElpsId == id
                            select new
                            {
                                PermitId = p.PermitId
                            };

            if (getPermit.Any())
            {
                return RedirectToAction("ViewBHPPermit", "ExternalPermitView", new { id = generalClass.Encrypt(getPermit.FirstOrDefault().PermitId.ToString()) });
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Permit reference not in correct format. Kindly contact support.") });
            }
        }



        public IActionResult ViewBHPPermit(string id)
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

                    foreach (var w in get.ToList())
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

                    return new ViewAsPdf("ViewBHPPermit", permitViewModels.ToList())
                    {
                        PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    };
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Could not find permit. Kindly contact support.") });
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
