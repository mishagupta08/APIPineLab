using Newtonsoft.Json;
using PineAppAPI.Models;
using PineAppAPI.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PineAppAPI.Controllers
{
    public class HomeController : ApiController
    {
        public PineLabRepository pineRepository;
        public BasicmlmRepository basicmlmRepository;
        public Repository repository;

        [HttpPost, Route("api/Home/LoginSuperAdmin")]
        public async Task<IHttpActionResult> LoginSuperAdmin()
        {
            var detail = await Request.Content.ReadAsStringAsync();
            var filters = JsonConvert.DeserializeObject<User>(detail);
            this.repository = new Repository();
            var result = await this.repository.LoginSuperAdmin(filters.Username, filters.Password);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        /*****PINE APP API START*******/

        [HttpPost, Route("api/Home/GenerateAuthenticationToken/{companyId}")]
        public async Task<IHttpActionResult> GenerateAuthenticationToken(int companyId)
        {
            this.pineRepository = new PineLabRepository();
            var result = await this.pineRepository.GenerateAuthenticationToken(companyId);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/GetCategoryList")]
        public async Task<IHttpActionResult> GetCategoryList()
        {
            this.pineRepository = new PineLabRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<RequestModel>(postDataJson);
            var result = await this.pineRepository.GetCategoryList(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/GetProductListByCategoryId")]
        public async Task<IHttpActionResult> GetProductListByCategoryId()
        {
            this.pineRepository = new PineLabRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<RequestModel>(postDataJson);
            var result = await this.pineRepository.GetProductListByCategoryId(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/GetProductDetailBySku")]
        public async Task<IHttpActionResult> GetProductDetailBySku()
        {
            this.pineRepository = new PineLabRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<RequestModel>(postDataJson);
            var result = await this.pineRepository.GetProductDetailBySku(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/CreateOrder")]
        public async Task<IHttpActionResult> CreateOrder()
        {
            this.pineRepository = new PineLabRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<RequestModel>(postDataJson);
            var result = await this.pineRepository.CreateOrder(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/GetOrderStatus")]
        public async Task<IHttpActionResult> GetOrderStatus()
        {
            this.pineRepository = new PineLabRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<RequestModel>(postDataJson);
            var result = await this.pineRepository.GetOrderStatus(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/ActivetedCardApi")]
        public async Task<IHttpActionResult> ActivetedCardApi()
        {
            this.pineRepository = new PineLabRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<RequestModel>(postDataJson);
            var result = await this.pineRepository.ActivetedCardApi(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);

        }

        /*****PINE APP API END*******/

        [HttpPost, Route("api/Home/ManageCategory")]
        public async Task<IHttpActionResult> ManageCategory()
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<Filters>(postDataJson);
            var result = await this.repository.ManageCategory(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/OrderReport")]
        public async Task<IHttpActionResult> OrderReport()
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<User>(postDataJson);
            var result = await this.repository.OrderReport(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/ManageProducts")]
        public async Task<IHttpActionResult> ManageProducts()

        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<Filters>(postDataJson);
            var result = await this.repository.ManageProducts(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/RefreshFullSystemFromWohoo")]
        public async Task<IHttpActionResult> RefreshFullSystemFromWohoo()
        {
            this.repository = new Repository();
            var result = await this.repository.RefreshFullSystemFromWohoo();
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/ManageCart/{operation}")]
        public async Task<IHttpActionResult> ManageCart(string operation)
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<CarteDetail>(postDataJson);
            var result = await this.repository.ManageCart(requestObject, operation);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/ManageUser/{operation}")]
        public async Task<IHttpActionResult> ManageUser(string operation)
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<List<User>>(postDataJson);
            var result = await this.repository.ManageUser(requestObject, operation);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/ManageOrder/{operation}")]
        public async Task<IHttpActionResult> ManageOrder(string operation)
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<OrderContainer>(postDataJson);
            var result = await this.repository.ManageOrder(requestObject, operation);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/OrderReportDetail/{refNo}")]
        public async Task<IHttpActionResult> OrderReportDetail(string refNo)
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<User>(postDataJson);
            var result = await this.repository.OrderReportDetail(requestObject, refNo);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/ValidateTransactionLogin")]
        public async Task<IHttpActionResult> ValidateTransactionLogin()
        {
            this.basicmlmRepository = new BasicmlmRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<User>(postDataJson);
            var result = await this.basicmlmRepository.ValidateTransactionLogin(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/GetWalletDetails")]
        public async Task<IHttpActionResult> GetWalletDetails()
        {
            this.basicmlmRepository = new BasicmlmRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<User>(postDataJson);
            var result = await this.basicmlmRepository.GetWalletBalance(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/DeductWallet")]
        public async Task<IHttpActionResult> DeductWallet()
        {
            this.basicmlmRepository = new BasicmlmRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<User>(postDataJson);
            var result = await this.basicmlmRepository.DeductWalletBalance(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/MangeOtpFunctions/{operation}")]
        public async Task<IHttpActionResult> MangeOtpFunctions(string operation)
        {
            var detail = await Request.Content.ReadAsStringAsync();
            var objUser = JsonConvert.DeserializeObject<User>(detail);
            this.repository = new Repository();
            var result = await this.repository.MangeOtpFunctions(objUser, operation);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost, Route("api/Home/VerifyTrasaction")]
        public async Task<IHttpActionResult> VerifyTrasaction()
        {
            this.basicmlmRepository = new BasicmlmRepository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<User>(postDataJson);
            var result = await this.basicmlmRepository.Confirmvoucher(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }
        [HttpPost, Route("api/Home/ManageFundRequest")]
        public async Task<IHttpActionResult> ManageFundRequest()
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<User>(postDataJson);
            var result = await this.repository.ManageFundRequest(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }
        [HttpPost, Route("api/Home/FundRequest")]
        public async Task<IHttpActionResult> FundRequest()
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<FundRequest>(postDataJson);
            var result = await this.repository.SaveFundRequest(requestObject);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }
        [HttpPost, Route("api/Home/UpdateFundRequest")]
        public async Task<IHttpActionResult> UpdateFundRequest(FundRequest objFundRequest)
        {
            this.repository = new Repository();
            var postDataJson = await Request.Content.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<FundRequest>(postDataJson);
            var result = await this.repository.UpdateFundRequest(objFundRequest.ID, objFundRequest.Remark, objFundRequest.CreatedBy);
            return Content(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

    }
}
