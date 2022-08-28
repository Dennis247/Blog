using Blog.Domain.Models.Response;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Blog.Web.Services
{
    public interface IHttpServices
    {
        WebHttpResponse Get(string url, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null);
        WebHttpResponse Post(string url, string payload, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null);
    }

    public class HttpServices : IHttpServices
    {
        private readonly IHttpContextAccessor _context;

        public HttpServices(IHttpContextAccessor context)
        {
            _context = context;
        }


        public WebHttpResponse Post(string url, string payload, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            WebHttpResponse responseM = new WebHttpResponse { Data = "", StatusCode = HttpStatusCode.InternalServerError };
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                addToken(request);
                if (headers != null)
                {
                    request.AddHeaders(headers);
                }
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        request.AddParameter(item.Key, item.Value);
                    }
                }
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", payload, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                responseM.Data = response.Content;
                responseM.StatusCode = response.StatusCode;

            }
            catch (Exception ex)
            {
                responseM.Data = ex.Message;
                responseM.StatusCode = HttpStatusCode.InternalServerError;
            }
            return responseM;
        }


        public WebHttpResponse Get(string url, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            WebHttpResponse responseM = new WebHttpResponse { Data = "", StatusCode = HttpStatusCode.InternalServerError };
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                addToken(request);

                if (headers != null)
                {
                    request.AddHeaders(headers);
                }

                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        request.AddParameter(item.Key, item.Value);
                    }
                }

                request.AddHeader("Content-Type", "application/json");
                client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                IRestResponse response = client.Execute(request);
                responseM.Data = response.Content;
                responseM.StatusCode = response.StatusCode;

            }
            catch (Exception ex)
            {
                responseM.Data = ex.Message;
                responseM.StatusCode = HttpStatusCode.InternalServerError;

            }
            return responseM;
        }

        void addToken(RestRequest request)
        {
            var JWToken = _context.HttpContext.Session.GetString("JWToken");
            if (JWToken != null)
            {
                request.AddHeader("Authorization", "Bearer " + JWToken);
            }
        }
    }

}
