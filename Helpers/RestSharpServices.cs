using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BHP.Helpers
{
    public class RestSharpServices
    {

        public static RestClient _client;
        public RestRequest _request;
        public Method _method;
        public string _url;
        public IConfiguration configuration;
          
        public RestSharpServices()
        {
            ElpsServices elpsServices = new ElpsServices();
            RestClient restClient = new RestClient(ElpsServices._elpsBaseUrl);
            restClient.Timeout = (60 * 1000);
            _client = restClient;
        }



        private Method restMethod(string methodType = null)
        {
            var method = methodType == "PUT" ? Method.PUT : methodType == "POST" ? Method.POST : methodType == "DELETE" ? Method.DELETE : Method.GET;
            return method;
        }

      
        private RestRequest ServiceRequest(string apiURL, string method = null)
        {
            _method  = restMethod(method);
            var request = new RestRequest(apiURL, _method);
            return request;
        }


        private RestRequest AddParameters(string apiURL, string method = null, List<ParameterData> paramData = null)
        {
            var _request = ServiceRequest(apiURL, method);
            _request.AddUrlSegment("email", ElpsServices._elpsAppEmail);
            _request.AddUrlSegment("apiHash", ElpsServices.appHash);

            if(paramData != null)
            {
                foreach(var _paramData in paramData)
                {
                    _request.AddUrlSegment(_paramData.ParamKey, _paramData.ParamValue);
                }
            }
            
            return _request;
        }


        // For all request PUT, POST, GET 
        public IRestResponse Response(string apiURL, List<ParameterData> paramData = null, string method = null, object json = null)
        {
            _url = apiURL;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _request = AddParameters(ElpsServices._elpsBaseUrl + apiURL, method, paramData);
            _request.RequestFormat = DataFormat.Json;

            if (json != null)
            {
               /* _request.AddBody(json);*/ _request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(json), ParameterType.RequestBody);
            }
            IRestResponse restResponse = _client.Execute(_request);

            return restResponse;
        }

      
        
        public string ErrorResponse(IRestResponse restResponse)
        {
            return "A network related error has occured. Message : " + restResponse.ErrorException.Source.ToString() + " - "+ restResponse.ErrorException.InnerException.Message.ToString() + " --- Error Code : " + restResponse.ErrorException.HResult;
        }


        
        public List<ParameterData> parameterData(string key, string value)
        {
            var paramData = new List<ParameterData>();

            paramData.Add(new ParameterData
            {
                ParamKey = key,
                ParamValue = value
            });

            return paramData;
        }


    }
}
