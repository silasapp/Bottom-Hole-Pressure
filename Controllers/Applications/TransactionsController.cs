using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using BHP.Helpers;
using BHP.Models.DB;
using BHP.HelpersClass;
using BHP.Models;

namespace LPG_Depot.Controllers.Applications
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly BHP_DBContext _context;

        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();
        RestSharpServices _restService = new RestSharpServices();


        public TransactionsController(BHP_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }

        

        public IActionResult Payments()
        {
            var transactions = (
                from tns in _context.Transactions
                join app in _context.RequestProposal on tns.RequestId equals app.RequestId
                join c in _context.Companies on app.CompanyId equals c.CompanyId into Company
                where app.DeletedStatus == false
                select new TransactionDetails()
                {
                    RefNo = app.RequestRefNo,
                    RRR = tns.Rrr,
                    CompanyName = Company.FirstOrDefault().CompanyName,
                    Amount = tns.AmtPaid,
                    TotalAmount = tns.TotalAmt,
                    ServiceCharge = tns.ServiceCharge,
                    TransDate = tns.TransactionDate,
                    TransStatus = tns.TransactionStatus,
                    TransType = tns.TransactionType,
                    TransRef = tns.TransRef,
                    Description = tns.Description,
                });

            _helpersController.LogMessages("Displaying payment details", _helpersController.getSessionEmail());

            return View(transactions.ToList());
        }


    }

}
