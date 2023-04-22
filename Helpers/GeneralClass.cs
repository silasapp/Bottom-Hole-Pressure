using BHP.HelpersClass;
using BHP.Models.DB;
using LpgLicense.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace BHP.Helpers
{
    public class GeneralClass : Controller
    {
        RestSharpServices restSharpServices = new RestSharpServices();
       
        public static string Approved = "Approved";
        public static string Rejected = "Rejected";
        public static string PaymentPending = "Payment Pending";
        public static string PaymentCompleted = "Payment Completed";
        public static string Processing = "Processing";
        public static string ProposalSubmitted = "Proposal Submitted";
        public static string ResultSubmitted = "Result Submitted";
        public static int ProposalAmount = 250000;
        public static string ApplicaionRequired = "Application Required";
        public static string Resubmitted = "Application Required";
        public static string DocumentsRequired = "Documents Required";
        public static string DocumentsUploaded = "Documents Uploaded";
        public static string BHPCode = "345";

        public static int elpsStateID = 0;

        public static int _Year = DateTime.Now.Year + 1;

        public static string _WAITING = "WAITING";
        public static string _STARTED = "STARTED";
        public static string _FINISHED = "FINISHED";

        public static string START = "START";
        public static string NEXT = "NEXT";
        public static string END = "END";
        public static string PASS = "PASS";
        public static string DONE = "DONE";
        public static string BEGIN = "BEGIN";

        public static string CUSTOMER = "CUSTOMER";
        public static string AD_UMR = "AD RM";
        public static string HEAD_UMR = "HEAD UMR";
        public static string SECTION_HEAD = "MANAGER RS";
        public static string TEAM = "TEAM";
        public static string HOOD = "HOOD";
        public static string COMPANY = "COMPANY";
        public static string SUPER_ADMIN = "SUPER ADMIN";
        public static string ADMIN = "ADMIN";
        public static string ICT_ADMIN = "ICT ADMIN";
        public static string DIRECTOR = "DIRECTOR";
        public static string SUPPORT = "SUPPORT";
        public static string ZOPSCON = "ZOPSCON";
        public static string OPSCON = "OPSCON";


        // Sending eamil parameters
        public static string PROPOSAL_REQUEST = "PROPOSAL REQUEST";
        public static string STAFF_NOTIFY = "STAFF NOTIFY";
        public static string COMPANY_NOTIFY = "COMPANY NOTIFY";


        public static string submit_exercise_company_doc = "STAFF REPORT FOR PARTICIPATING IN EXERCISE (BHP)"; // company doc
        public static string staff_application_report_company_doc = "ATTACHED APLICATION REPORT FOR STAFF TO UPLOAD"; // company doc
        public static string company_acknowledgement_doc = "COMPANY'S BHP ACKNOWLEDGEMENT LETTER IN RESPONSE TO PROPOSAL REQUEST"; // company doc


        private Object lockThis = new object();
        

        public string Encrypt(string clearText)
        {
            try
            {
                byte[] b = ASCIIEncoding.ASCII.GetBytes(clearText);
                string crypt = Convert.ToBase64String(b);
                byte[] c = ASCIIEncoding.ASCII.GetBytes(crypt);
                string encrypt = Convert.ToBase64String(c);

                return encrypt;
            }
            catch (Exception ex)
            {
                return "Error";
                throw ex;
            }
        }



        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] b;
                byte[] c;
                string decrypt;
                b = Convert.FromBase64String(cipherText);
                string crypt = ASCIIEncoding.ASCII.GetString(b);
                c = Convert.FromBase64String(crypt);
                decrypt = ASCIIEncoding.ASCII.GetString(c);

                return decrypt;
            }
            catch (Exception ex)
            {
                return "Error";
                throw ex;
            }
        }


         /* Decrypting all ID
         *
         * ids => encrypted ids
         */ 
        public int DecryptIDs(string ids)
        {
            int id = 0;
            var ID = this.Decrypt(ids);

            if (ID == "Error")
            {
                id = 0;
            }
            else
            {
                id = Convert.ToInt32(ID);
            }

            return id;
        }




        public JsonResult RestResult(string url, string method, List<ParameterData> parameterData = null, object app_object = null, string output = null, string endUrl = null)
        {
            AppModels appModel = new AppModels();

            var response = restSharpServices.Response("/api/" + url + "/{email}/{apiHash}" + endUrl, parameterData, method, app_object);

            if (response.ErrorException != null)
            {
                return Json("Network Error");
            }
            else
            {
                if(method == "POST" || method == "PUT" || method == "DELETE")
                {
                    if (!string.IsNullOrWhiteSpace(response.Content))
                    {
                        return Json(output);
                    }
                    else
                    {
                        return Json("Opps... an error occured, please try again. " + response.ErrorMessage);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(response.Content))
                    {
                        return Json(JsonConvert.DeserializeObject(response.Content));
                    }
                    else
                    {
                        return Json("Opps... an error occured, please try again. " + response.ErrorMessage);
                    }
                }
            }
        }



        public List<Document> getCompanyDocuments(string companyID)
        {
            List<Document> documents = new List<Document>();

            var paramData = restSharpServices.parameterData("id", companyID);
            var response = restSharpServices.Response("/api/CompanyDocuments/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful == false)
            {
                documents = null;
            }
            else
            {
                documents = JsonConvert.DeserializeObject<List<Document>>(response.Content);
            }
            return documents;
        }




        public int GetStatesFromCountry(string State)
        {
            var paramData2 = restSharpServices.parameterData("Id", "156");
            var response2 = restSharpServices.Response("/api/Address/states/{Id}/{email}/{apiHash}", paramData2); // GET

            var res2 = JsonConvert.DeserializeObject<List<LpgLicense.Models.State>>(response2.Content);

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string state = textInfo.ToTitleCase(State.ToLower());

            foreach (var s in res2)
            {
                if (s.Name.Contains(state))
                {
                    elpsStateID = s.Id;
                    break;
                }
            }

            return elpsStateID;
        }



        //Generating Application number
        public string Generate_Application_Number()
        {
            lock (lockThis)
            {
                Thread.Sleep(1000);
                return BHPCode + DateTime.Now.ToString("MMddyyHHmmss");
            }
        }



        public string Generate_Receipt_Number()
        {
            lock (lockThis)
            {
                Thread.Sleep(1000);
                return DateTime.Now.ToString("MMddyyHHmmss");
            }
        }


       

        public string GetStateShortName(string state, string code)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>()
            {
                {"Abia", "AB" }, 
                {"Adamawa", "AD" }, 
                {"Akwa Ibom", "AK" }, 
                {"Anambra", "AN" }, 
                {"Bauchi", "BA" }, 
                {"Bayelsa", "BY" }, 
                {"Benue", "BE" }, 
                {"Borno", "BO" }, 
                {"Cross River", "CR" }, 
                {"Delta", "DE" }, 
                {"Ebonyi", "EB" }, 
                {"Edo", "ED" }, 
                {"Enugu", "EN" }, 
                {"Federal Capital Territory", "FC" }, 
                {"Abuja", "FC" }, 
                {"Gombe", "GO" }, 
                {"Imo", "IM" }, 
                {"Jigawa", "JI" }, 
                {"Kaduna", "KD" }, 
                {"Kano", "KN" }, 
                {"Katsina", "KT" }, 
                {"Kebbi", "KE" }, 
                {"Kogi", "KO" }, 
                {"Kwara", "KW" }, 
                {"Lagos", "LA" }, 
                {"Nasarawa", "NA" }, 
                {"Niger", "NI" }, 
                {"Ogun", "OG" }, 
                {"Ondo", "ON" }, 
                {"Osun", "OS" }, 
                {"Oyo", "OY" }, 
                {"Plateau", "PL" }, 
                {"Rivers", "RI" }, 
                {"Sokoto", "SO" }, 
                {"Taraba", "TA" }, 
                {"Yobe", "YO" }, 
                {"Zamfara", "ZA" }, 
            };
            var shortState = pairs.Where(x => x.Key.ToUpper() == state.ToUpper().Trim()).FirstOrDefault();
            var CompanyCode = "BHP-" + code.Trim() + "-" + shortState.Value;
            return CompanyCode;
        }




       
    }
}

    

