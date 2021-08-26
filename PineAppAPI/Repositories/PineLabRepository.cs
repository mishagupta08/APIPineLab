using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using PineAppAPI.Models;
using PineAppAPI.Properties;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace PineAppAPI.Repositories
{
    public class PineLabRepository
    {
        PineLabsEntities entity = new PineLabsEntities();
        private const int ERRORCODE = 444;
        int CompanyIdPineAPI = Convert.ToInt32(ConfigurationManager.AppSettings["CompanyIdPineAPI"]);
        string authCodeRequestUrl = ConfigurationManager.AppSettings["AuthCodeUrl"];//request code url
        string accessTokenRequestUrl = ConfigurationManager.AppSettings["AuthTokenUrl"];//access token url
        //string categoryUrl = "https://sandbox.woohoo.in/rest/v3/catalog/categories";
        //string productListUrl = "https://sandbox.woohoo.in/rest/v3/catalog/categories/categoryId/products";
        //string productDetailUrl = "https://sandbox.woohoo.in/rest/v3/catalog/products/sku";
        //string orderStatusUrl = "https://sandbox.woohoo.in/rest/v3/order/refno/status";
        //string createOrderUrl = "https://sandbox.woohoo.in/rest/v3/orders";
        //string ActiveCardApi = "https://sandbox.woohoo.in/rest/v3/order/id/cards";
        string categoryUrl = "https://extapi12.woohoo.in/rest/v3/catalog/categories";
        string productListUrl = "https://extapi12.woohoo.in/rest/v3/catalog/categories/categoryId/products";
        string productDetailUrl = "https://extapi12.woohoo.in/rest/v3/catalog/products/sku";
        string orderStatusUrl = "https://extapi12.woohoo.in/rest/v3/order/refno/status";
        string createOrderUrl = "https://extapi12.woohoo.in/rest/v3/orders";
        string ActiveCardApi = "https://extapi12.woohoo.in/rest/v3/order/id/cards";
        Regex reg = new Regex(@"%[a-f0-9]{2}");

        //string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjb25zdW1lcklkIjoiMjE0IiwiZXhwIjoxNjA1MjY4NTEyLCJ0b2tlbiI6ImIwZjJhY2U1MjMxY2QwNGI1NGVjMWNmM2ZlYTVhNDk2In0.RWmrgIsoiICqj93pfOEey9qyKO0JVB_2nO_OrJi6le0";
        string dateAtClient = "yyyy-mm-ddThh:min:ss.000Z";
        public async Task<ResponceDetail> GenerateAuthenticationToken(int companyId)
        {
            var responceObject = new ResponceDetail();
            try
            {
                var company = await GetConsumerKeyAndSecret(companyId);

                //var detail = new AuthCodeRequestParameter
                //{
                //    clientId = company.ConsumerKey,
                //    username = company.Username,
                //    password = company.Password
                //};

                var detail = new AuthCodeRequestParameter
                {
                    clientId = company.LiveConsumerKey,
                    username = company.Username,
                    password = company.Password
                };

                var jsonData = JsonConvert.SerializeObject(detail);
                IRestResponse response = await CallRestClient(authCodeRequestUrl, Method.POST, jsonData, null);
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    var authCodeResponce = JsonConvert.DeserializeObject<TokenResponce>(response.Content);
                    if (authCodeResponce != null)
                    {
                        var authDetail = new AuthTokenRequestParameter()
                        {
                            clientId = company.LiveConsumerKey,
                            clientSecret = company.LiveConsumerSecret,
                            authorizationCode = authCodeResponce.authorizationCode
                        };

                        jsonData = JsonConvert.SerializeObject(authDetail);

                        response = await CallRestClient(accessTokenRequestUrl, Method.POST, jsonData, null);
                        if (response != null && response.StatusCode == HttpStatusCode.OK)
                        {
                            responceObject.AuthenticationToken = JsonConvert.DeserializeObject<TokenResponce>(response.Content);
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                            responceObject.Status = true;
                        }
                        else
                        {
                            responceObject.Message = response.Content;
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                        }
                    }
                }
                else
                {
                    responceObject.Message = response.Content;
                    responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                }
            }
            catch (Exception e)
            {
                responceObject.Message = e.Message;
                if (string.IsNullOrEmpty(e.Message))
                {
                    responceObject.Message = Convert.ToString(e.InnerException);
                }
            }
            return responceObject;
        }

        public async Task<ResponceDetail> GetCategoryList(RequestModel categoryDetail)
        {
            var responceObject = new ResponceDetail();
            categoryDetail.CompanyId = CompanyIdPineAPI;
            try
            {
                if (categoryDetail.CompanyId == 0)
                {
                    responceObject.Message = "Please send company Id";
                    responceObject.StatusCode = 404;
                }
                else
                {
                    var company = await GetConsumerKeyAndSecret(categoryDetail.CompanyId);
                    company.AuthToken = await CheckAuthToken(categoryDetail.CompanyId);

                    var urlToCall = categoryUrl;
                    if (categoryDetail.CategoryId != 0)
                    {
                        urlToCall = urlToCall + "/" + categoryDetail.CategoryId;
                    }

                    var encodedDetail = HttpUtility.UrlEncode(urlToCall);
                    Regex reg = new Regex(@"%[a-f0-9]{2}");
                    encodedDetail = reg.Replace(encodedDetail, m => m.Value.ToUpperInvariant());

                    var REQUEST_SIGNATURE = CreateREQUEST_SIGNATURE("GET&" + encodedDetail, company.LiveConsumerSecret);

                    if (string.IsNullOrEmpty(REQUEST_SIGNATURE))
                    {
                        responceObject.Message = Resources.ErrorMessage;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else if (REQUEST_SIGNATURE.Contains("Error : "))
                    {
                        responceObject.Message = REQUEST_SIGNATURE;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else
                    {
                        var headerList = new Dictionary<string, string>();
                        var today = DateTime.UtcNow;
                        var dateToSend = dateAtClient.Replace("yyyy", today.Year.ToString()).Replace("mm", today.Month.ToString()).Replace("dd", today.Day.ToString()).Replace("hh", today.Hour.ToString()).Replace("min", today.Minute.ToString()).Replace("ss", today.Second.ToString());
                        var datet = DateTime.UtcNow.ToString();
                        headerList.Add("dateAtClient", "2011-10-05T14:48:00.000Z");
                        //headerList.Add("dateAtClient", dateToSend);
                        headerList.Add("signature", REQUEST_SIGNATURE);
                        headerList.Add("Authorization", "Bearer " + company.AuthToken);

                        IRestResponse response = await CallRestClient(urlToCall, Method.GET, null, headerList);

                        if (response != null && response.StatusCode == HttpStatusCode.OK)
                        {
                            responceObject.CategoryList = JsonConvert.DeserializeObject<CategoryResponce>(response.Content);
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                            responceObject.Status = true;
                        }
                        else
                        {
                            responceObject.Message = response.Content;
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                responceObject.Message = e.Message;
                if (string.IsNullOrEmpty(e.Message))
                {
                    responceObject.Message = Convert.ToString(e.InnerException);
                }
            }
            return responceObject;
        }

        public async Task<ResponceDetail> GetProductListByCategoryId(RequestModel requestDetail)
        {
            requestDetail.CompanyId = CompanyIdPineAPI;
            var responceObject = new ResponceDetail();
            try
            {
                if (requestDetail.CompanyId == 0)
                {
                    responceObject.Message = "Please send company Id";
                    responceObject.StatusCode = 404;
                }
                else if (requestDetail.CategoryId == 0)
                {
                    responceObject.Message = "Please send category Id";
                    responceObject.StatusCode = 404;
                }
                else
                {

                    var company = await GetConsumerKeyAndSecret(requestDetail.CompanyId);
                    company.AuthToken = await CheckAuthToken(requestDetail.CompanyId);

                    var urlToCall = productListUrl;
                    if (requestDetail.CategoryId != 0)
                    {
                        urlToCall = urlToCall.Replace("categoryId", Convert.ToString(requestDetail.CategoryId));
                        urlToCall = urlToCall + "?limit=" + requestDetail.Limit + "&offset=" + requestDetail.Offset;
                    }

                    var encodedDetail = HttpUtility.UrlEncode(urlToCall);
                    Regex reg = new Regex(@"%[a-f0-9]{2}");
                    encodedDetail = reg.Replace(encodedDetail, m => m.Value.ToUpperInvariant());

                    var REQUEST_SIGNATURE = CreateREQUEST_SIGNATURE("GET&" + encodedDetail, company.LiveConsumerSecret);

                    if (string.IsNullOrEmpty(REQUEST_SIGNATURE))
                    {
                        responceObject.Message = Resources.ErrorMessage;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else if (REQUEST_SIGNATURE.Contains("Error : "))
                    {
                        responceObject.Message = REQUEST_SIGNATURE;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else
                    {
                        var headerList = new Dictionary<string, string>();
                        headerList.Add("dateAtClient", "2011-10-05T14:48:00.000Z");
                        headerList.Add("signature", REQUEST_SIGNATURE);
                        headerList.Add("Authorization", "Bearer " + company.AuthToken);

                        IRestResponse response = await CallRestClient(urlToCall, Method.GET, null, headerList);

                        if (response != null && response.StatusCode == HttpStatusCode.OK)
                        {
                            responceObject.ProductList = JsonConvert.DeserializeObject<ProductListContainer>(response.Content);
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                            responceObject.Status = true;
                        }
                        else
                        {
                            responceObject.Message = response.Content;
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                responceObject.Message = e.Message;
                if (string.IsNullOrEmpty(e.Message))
                {
                    responceObject.Message = Convert.ToString(e.InnerException);
                }
            }

            return responceObject;
        }

         public async Task<ResponceDetail>ActivetedCardApi(RequestModel requestDetail)
        {
            requestDetail.CompanyId = CompanyIdPineAPI;
            var responceObject = new ResponceDetail();
            try
            {
                if (requestDetail.CompanyId == 0)
                {
                    responceObject.Message = "Please send company Id";
                    responceObject.StatusCode = 404;
                }
                else if (requestDetail.orderId == "")
                {
                    responceObject.Message = "Please send category Id";
                    responceObject.StatusCode = 404;
                }
                else
                {
                    var company = await GetConsumerKeyAndSecret(requestDetail.CompanyId);
                    company.AuthToken = await CheckAuthToken(requestDetail.CompanyId);
                    var urlToCall = ActiveCardApi;
                    if (requestDetail.orderId != "")
                    {
                        urlToCall = urlToCall.Replace("id", Convert.ToString(requestDetail.orderId));
                        urlToCall = urlToCall + "?limit=" + requestDetail.Limit + "&offset=" + requestDetail.Offset;
                    }
                       // urlToCall = urlToCall + "?limit=" + requestDetail.Limit + "&offset=" + requestDetail.Offset;

                    var encodedDetail = HttpUtility.UrlEncode(urlToCall);
                    Regex reg = new Regex(@"%[a-f0-9]{2}");
                    encodedDetail = reg.Replace(encodedDetail, m => m.Value.ToUpperInvariant());

                    var REQUEST_SIGNATURE = CreateREQUEST_SIGNATURE("GET&" + encodedDetail, company.LiveConsumerSecret);

                    if (string.IsNullOrEmpty(REQUEST_SIGNATURE))
                    {
                        responceObject.Message = Resources.ErrorMessage;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else if (REQUEST_SIGNATURE.Contains("Error : "))
                    {
                        responceObject.Message = REQUEST_SIGNATURE;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else
                    {
                        var headerList = new Dictionary<string, string>();
                        headerList.Add("dateAtClient", "2011-10-05T14:48:00.000Z");
                        headerList.Add("signature", REQUEST_SIGNATURE);
                        headerList.Add("Authorization", "Bearer " + company.AuthToken);

                        IRestResponse response = await CallRestClient(urlToCall, Method.GET, null, headerList);
                        if (response != null && response.StatusCode == HttpStatusCode.OK)
                        {
                           // responceObject.ProductList = JsonConvert.DeserializeObject<ProductListContainer>(response.Content);
                            responceObject.CardActivated = JsonConvert.DeserializeObject<M_ResCardActivated>(response.Content);
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                            responceObject.Status = true;
                        }
                        else
                        {
                            responceObject.Message = response.Content;
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                        }
                    }
                    
                }
            }
             catch(Exception ex)
            {
                responceObject.Message = ex.Message;
                if (string.IsNullOrEmpty(ex.Message))
                {
                    responceObject.Message = Convert.ToString(ex.InnerException);
                }
            }
            return responceObject;
        }

        public async Task<ResponceDetail> GetProductDetailBySku(RequestModel requestDetail)
        {
            requestDetail.CompanyId = CompanyIdPineAPI;
            var responceObject = new ResponceDetail();
            try
            {
                if (requestDetail.CompanyId == 0)
                {
                    responceObject.Message = "Please send company Id";
                    responceObject.StatusCode = 404;
                }
                else if (string.IsNullOrEmpty(requestDetail.Sku))
                {
                    responceObject.Message = "Please send product Sku.";
                    responceObject.StatusCode = 404;
                }
                else
                {
                    var company = await GetConsumerKeyAndSecret(requestDetail.CompanyId);
                    company.AuthToken = await CheckAuthToken(requestDetail.CompanyId);

                    var urlToCall = productDetailUrl;

                    urlToCall = urlToCall.Replace("sku", requestDetail.Sku);

                    var encodedDetail = HttpUtility.UrlEncode(urlToCall);
                    Regex reg = new Regex(@"%[a-f0-9]{2}");
                    encodedDetail = reg.Replace(encodedDetail, m => m.Value.ToUpperInvariant());

                    var REQUEST_SIGNATURE = CreateREQUEST_SIGNATURE("GET&" + encodedDetail, company.LiveConsumerSecret);

                    if (string.IsNullOrEmpty(REQUEST_SIGNATURE))
                    {
                        responceObject.Message = Resources.ErrorMessage;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else if (REQUEST_SIGNATURE.Contains("Error : "))
                    {
                        responceObject.Message = REQUEST_SIGNATURE;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else
                    {
                        var headerList = new Dictionary<string, string>();
                        headerList.Add("dateAtClient", "2011-10-05T14:48:00.000Z");
                        headerList.Add("signature", REQUEST_SIGNATURE);
                        headerList.Add("Authorization", "Bearer " + company.AuthToken);

                        IRestResponse response = await CallRestClient(urlToCall, Method.GET, null, headerList);

                        if (response != null && response.StatusCode == HttpStatusCode.OK)
                        {
                            responceObject.ProductDetail = JsonConvert.DeserializeObject<ProductDetail>(response.Content);
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                            responceObject.Status = true;
                        }
                        else
                        {
                            responceObject.Message = response.Content;
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                responceObject.Message = e.Message;
                if (string.IsNullOrEmpty(e.Message))
                {
                    responceObject.Message = Convert.ToString(e.InnerException);
                }
            }

            return responceObject;
        }

        public async Task<ResponceDetail> CreateOrder(RequestModel requestDetail)
        {
            requestDetail.CompanyId = CompanyIdPineAPI;
            var responceObject = new ResponceDetail();
            try
            {
                if (requestDetail.CompanyId == 0)
                {
                    responceObject.Message = "Please send company Id";
                    responceObject.StatusCode = 404;
                }
                else if (requestDetail.orderRequestDetail == null)
                {
                    responceObject.Message = "Please send Order detail";
                    responceObject.StatusCode = 404;
                }
                else
                {

                    var company = await GetConsumerKeyAndSecret(requestDetail.CompanyId);
                    company.AuthToken = await CheckAuthToken(requestDetail.CompanyId);

                    var urlToCall = createOrderUrl;

                    var orderJson = JsonConvert.SerializeObject(requestDetail.orderRequestDetail);
                    var jObj = JObject.FromObject(requestDetail.orderRequestDetail);
                    var encodedNewJson = Uri.EscapeDataString(orderJson);
                    //encodedNewJson = reg.Replace(encodedNewJson, m => m.Value.ToUpperInvariant());
                    //encodedNewJson = encodedNewJson.Replace("+", "");
                    var encodedDetail = Uri.EscapeDataString(urlToCall);

                    encodedDetail = reg.Replace(encodedDetail, m => m.Value.ToUpperInvariant());

                    var REQUEST_SIGNATURE = CreateREQUEST_SIGNATURE("POST&" + encodedDetail + "&" + encodedNewJson, company.LiveConsumerSecret);

                    if (string.IsNullOrEmpty(REQUEST_SIGNATURE))
                    {
                        responceObject.Message = Resources.ErrorMessage;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else if (REQUEST_SIGNATURE.Contains("Error : "))
                    {
                        responceObject.Message = REQUEST_SIGNATURE;
                        responceObject.StatusCode = ERRORCODE;
                    }
                    else
                    {
                        var headerList = new Dictionary<string, string>();
                        headerList.Add("dateAtClient", "2020-09-01T10:25:01.876Z");
                        headerList.Add("signature", REQUEST_SIGNATURE);
                        headerList.Add("Authorization", "Bearer " + company.AuthToken);

                            IRestResponse response = await CallRestClient(urlToCall, Method.POST, orderJson, headerList);
                        responceObject.orderContent = response.Content;
                        if (response != null && (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted))
                        {
                            responceObject.CreateOrderResponceDetail = JsonConvert.DeserializeObject<OrderRoot>(response.Content);
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                            responceObject.Status = true;
                        }
                        else
                        {
                            responceObject.Message = response.Content;
                            responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                responceObject.Message = e.Message;
                if (string.IsNullOrEmpty(e.Message))
                {
                    responceObject.Message = Convert.ToString(e.InnerException);
                }
            }

            return responceObject;
        }

        public async Task<ResponceDetail> GetOrderStatus(RequestModel requestDetail)
        {
            requestDetail.CompanyId = CompanyIdPineAPI;
            var responceObject = new ResponceDetail();
            try
            {
                if (requestDetail.CompanyId == 0)
                {
                    responceObject.Message = "Please send company Id";
                    responceObject.StatusCode = 404;
                }
                else if (string.IsNullOrEmpty(requestDetail.OrderRefno))
                {
                    responceObject.Message = "Please send Order reference no.";
                    responceObject.StatusCode = 404;
                }
                else
                {
                    var company = await GetConsumerKeyAndSecret(requestDetail.CompanyId);
                    company.AuthToken = await CheckAuthToken(requestDetail.CompanyId);
                    if (company == null)
                    {
                        responceObject.Message = "Company detail not found.";
                    }
                    else
                    {
                        var urlToCall = orderStatusUrl;

                        urlToCall = urlToCall.Replace("refno", requestDetail.OrderRefno);

                        var encodedDetail = HttpUtility.UrlEncode(urlToCall);
                        Regex reg = new Regex(@"%[a-f0-9]{2}");
                        encodedDetail = reg.Replace(encodedDetail, m => m.Value.ToUpperInvariant());

                        var REQUEST_SIGNATURE = CreateREQUEST_SIGNATURE("GET&" + encodedDetail, company.LiveConsumerSecret);

                        if (string.IsNullOrEmpty(REQUEST_SIGNATURE))
                        {
                            responceObject.Message = Resources.ErrorMessage;
                            responceObject.StatusCode = ERRORCODE;
                        }
                        else if (REQUEST_SIGNATURE.Contains("Error : "))
                        {
                            responceObject.Message = REQUEST_SIGNATURE;
                            responceObject.StatusCode = ERRORCODE;
                        }
                        else
                        {
                            var headerList = new Dictionary<string, string>();
                            headerList.Add("dateAtClient", "2011-10-05T14:48:00.000Z");
                            headerList.Add("signature", REQUEST_SIGNATURE);
                            headerList.Add("Authorization", "Bearer " + company.AuthToken);

                            IRestResponse response = await CallRestClient(urlToCall, Method.GET, null, headerList);

                            if (response != null && response.StatusCode == HttpStatusCode.OK)
                            {
                                responceObject.CreateOrderResponceDetail = JsonConvert.DeserializeObject<OrderRoot>(response.Content);
                                responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                                responceObject.Status = true;
                            }
                            else
                            {
                                responceObject.Message = response.Content;
                                responceObject.StatusCode = Convert.ToInt16(response.StatusCode);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                responceObject.Message = e.Message;
                if (string.IsNullOrEmpty(e.Message))
                {
                    responceObject.Message = Convert.ToString(e.InnerException);
                }
            }

            return responceObject;
        }

        private string CreateREQUEST_SIGNATURE(string message, string secret)
        {
            try
            {
                return GetHmacSha256(message, secret);
            }
            catch (Exception e)
            {
                var error = "Error : " + e.Message;
                if (e.InnerException != null)
                {
                    var err = e.InnerException.ToString();
                    if (!string.IsNullOrEmpty(err))
                    {
                        error = error + " " + err;
                    }
                }

                return error;
            }
        }

        private string GetHmacSha256(string stringToSign, string secretKey)
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var data = Encoding.UTF8.GetBytes(stringToSign);
            var hash = new Sha512Digest();

            HMac hmac = new HMac(hash);

            hmac.Init(new KeyParameter(key, 0, key.Length));

            hmac.BlockUpdate(data, 0, data.Length);

            byte[] abyDigest = new byte[hmac.GetMacSize()];

            int nRet = hmac.DoFinal(abyDigest, 0);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < abyDigest.Length; i++)
            {
                builder.Append(abyDigest[i].ToString("x2"));
            }
            return builder.ToString();
        }

        private static async Task<IRestResponse> CallRestClient(string url, Method methodCode, string jsonData, Dictionary<string, string> headerList)
        {
            var client = new RestClient(url);
            client.Timeout = -1;

            var request = new RestRequest(methodCode);

            //Headers
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "*/*");
            if (headerList != null && headerList.Count > 0)
            {
                foreach (var header in headerList)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (methodCode == Method.POST)
            {
                //Request Body
                request.AddParameter("application/json", jsonData, ParameterType.RequestBody);
            }
            //Execute
            IRestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        private async Task<string> CheckAuthToken(int companyId)
        {
            var company = await Task.Run(() => entity.Companies.FirstOrDefault(c => c.Id == companyId));
            var responce = new ResponceDetail();
            var generateFlag = false;
            if (company == null)
            {
                return string.Empty;
            }
            else
            {
                if (company.AuthTokenGeneationDate == null)
                {
                    generateFlag = true;
                }
                else
                {
                    var days = (DateTime.Now - company.AuthTokenGeneationDate).Value.TotalDays;
                    if (days >= 7)
                    {
                        generateFlag = true;
                    }
                }

                if (generateFlag)
                {
                    responce = await this.GenerateAuthenticationToken(companyId);
                    if (responce.Status)
                    {
                        company.AuthTokenGeneationDate = DateTime.Now;
                        company.AuthToken = responce.AuthenticationToken.token;
                        await entity.SaveChangesAsync();
                    }
                }


            }

            return company.AuthToken;
        }

        private async Task<Company> GetConsumerKeyAndSecret(int companyId)
        {
            var company = await Task.Run(() => entity.Companies.FirstOrDefault(c => c.Id == companyId));

            // var company = new CompanyDetail();
            //company.ConsumerKey = "3dd591fddf3c64e23c79006698394c50";
            //company.LiveConsumerSecret = "5538bbcf3a67e227b2a7728db7202235";
            //company.Username = "discounttadkaapisandboxb2b@woohoo.in";
            //company.Password = "discounttadkaapisandboxb2b@1234";

            return company;
        }

        void Sort(JObject jObj)
        {
            var props = jObj.Properties().ToList();
            foreach (var prop in props)
            {
                prop.Remove();
            }

            foreach (var prop in props.OrderBy(p => p.Name))
            {
                jObj.Add(prop);
                if (prop.Value is JObject)
                    Sort((JObject)prop.Value);
            }
        }

    }
}