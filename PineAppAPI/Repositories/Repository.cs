using PineAppAPI.Models;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Configuration;

namespace PineAppAPI.Repositories
{

    public class Repository
    {
        PineLabsEntities entity = new PineLabsEntities();
        string LiveCONNECTION_STRING = ConfigurationManager.ConnectionStrings["liveconstr"].ConnectionString;
        private string InsertProductDetailSP = "InsertProductDetail @pProdDetail";
        private string InsertWohooProductImageDetaiSP = "InsertWohooProductImageDetail @pImages";
        private string InsertProductContentSP = "InsertProductContent @pProdContent";
        private string InsertProductDenominationSP = "InsertProductDenomination @pProdDenomination";
        private string InsertProductDiscountsSP = "InsertProductDiscounts @pProdDiscount";
        private string InsertProductThemeSP = "InsertProductTheme @pProdTheme";
        private string InsertRelatedProductListSP = "InsertRelatedProductList @pProrelated";
        private string InsertProductCategoryRelationListSP = "InsertProductCategoryRelationList @pProdCat";
        private static byte[] KeyByte = Encoding.ASCII.GetBytes("6b04d38748f94490a636cf1be3d82841");
        private static byte[] IVByte = Encoding.ASCII.GetBytes("f8adbf3c94b7463d");
       

        public async Task<ResponceDetail> LoginSuperAdmin(string username, string password)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    responseDetail.Message = "Please send complete details.";
                }

                var role = await Task.Run(() => entity.Roles.FirstOrDefault(g => g.Name.Contains("Admin")));

                var user = await Task.Run(() => entity.Users.FirstOrDefault(g => g.Username == username && g.Password == password && g.Status == true && g.RoleId == role.Id));

                if (user == null)
                {
                    responseDetail.Message = "Invalid username or password.";
                }
                else
                {
                    user.LastLogin = DateTime.Now;
                    await entity.SaveChangesAsync();
                    responseDetail.Status = true;
                    responseDetail.UserDetail = user;
                    //  responseDetail.Message = new JavaScriptSerializer().Serialize(user);
                }
            }
            catch (Exception e)
            {
                responseDetail.Message = e.Message;
            }

            return responseDetail;
        }

        public async Task<ResponceDetail> ManageUser(List<User> bulkList, string operation)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                if (string.IsNullOrEmpty(operation))
                {
                    responseDetail.Message = "Please send complete details.";
                }

                if (operation == "Add")
                {
                    if (bulkList == null || bulkList.Count == 0)
                    {
                        responseDetail.Message = "Please send complete details.";
                    }
                    var flag = false;

                    foreach (var userDetail in bulkList)
                    {
                        var user = await Task.Run(() => entity.Users.FirstOrDefault(g => g.Username == userDetail.Username && g.CompanyId == userDetail.CompanyId));
                        if (user == null)
                        {
                            var role = await Task.Run(() => entity.Roles.FirstOrDefault(g => g.Name.Contains("User")));

                            userDetail.LastLogin = DateTime.Now;
                            userDetail.Status = true;

                            entity.Users.Add(userDetail);
                        }
                        else
                        {
                            flag = true;
                            responseDetail.Message = "Username already exist, No record added.";
                            break;
                        }
                    }

                    if (!flag)
                    {
                        await entity.SaveChangesAsync();
                        responseDetail.Status = true;
                        responseDetail.Message = "Record saved successfully.";
                    }
                }
                else if (operation == "ById")
                {
                    var data = bulkList.FirstOrDefault();
                    var brnd = entity.Users.FirstOrDefault(g => g.Id == data.Id);
                    if (brnd == null)
                    {
                        responseDetail.Message = "User detail not found.";
                    }
                    else
                    {
                        responseDetail.Status = true;
                        responseDetail.Message = new JavaScriptSerializer().Serialize(brnd);
                    }
                }
                else if (operation == "Login")
                {
                    var usr = bulkList.FirstOrDefault();
                    var role = await Task.Run(() => entity.Roles.FirstOrDefault(g => g.Name.Contains("User")));
                    var user = await Task.Run(() => entity.Users.FirstOrDefault(g => g.Username == usr.Username && g.Password == usr.Password && g.Status == true && g.RoleId == role.Id && g.CompanyId == usr.CompanyId));
                    if (user == null)
                    {
                        responseDetail.Message = "Invalid username or password.";
                    }
                    else
                    {
                        responseDetail.Status = true;
                        responseDetail.UserDetail = user;
                    }
                }
                else if (operation == "Edit" || operation == "Delete" || operation == "UpdateStatus")
                {
                    var users = bulkList.FirstOrDefault();
                    var usr = entity.Users.FirstOrDefault(g => g.Id == users.Id);
                    if (usr == null)
                    {
                        responseDetail.Message = "User not found.";
                    }
                    else
                    {
                        if (operation == "Edit")
                        {
                            usr.Firstname = users.Firstname;
                            usr.Lastname = users.Lastname;
                            usr.Username = users.Username;
                            usr.Password = users.Password;
                            usr.Email = users.Email;
                            usr.Mobile = users.Mobile;
                            usr.CompanyId = users.CompanyId;

                            usr.Status = users.Status;
                            responseDetail.Message = "User detail updated successfully.";
                        }
                        else if (operation == "UpdateStatus")
                        {
                            responseDetail.Status = true;
                            if (usr.Status == true)
                            {
                                usr.Status = false;
                                responseDetail.Message = "Activate";
                            }
                            else
                            {
                                usr.Status = true;
                                responseDetail.Message = "DeAtivate";
                            }
                        }
                        else
                        {
                            entity.Users.Remove(usr);
                            responseDetail.Status = true;
                            responseDetail.Message = "User deleted successfully.";
                        }

                        await entity.SaveChangesAsync();
                    }
                }
                else if (operation == "List")
                {
                    var list = await Task.Run(() => entity.Users.ToList());

                    if (list == null || list.Count() == 0)
                    {
                        responseDetail.Message = "No Records found.";
                    }
                    else
                    {
                        responseDetail.Status = true;
                        responseDetail.Message = new JavaScriptSerializer().Serialize(list);
                    }
                }

                else if (operation == "ListByCompanyId")
                {
                    var data = bulkList.FirstOrDefault();
                    var list = await Task.Run(() => entity.Users.Where(p => p.CompanyId == data.CompanyId));

                    if (list == null || list.Count() == 0)
                    {
                        responseDetail.Message = "No Records found.";
                    }
                    else
                    {
                        responseDetail.Status = true;
                        responseDetail.Message = new JavaScriptSerializer().Serialize(list);
                    }
                }
            }
            catch (Exception e)
            {
                responseDetail.Message = e.Message;
            }

            return responseDetail;
        }

        public async Task<ResponceDetail> ManageOrder(OrderContainer order, string operation)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                if (operation == "SaveBillingAddress")
                {
                    if (order.billingAddress == null)
                    {
                        responseDetail.Message = "Please send complete detail.";
                    }
                    else
                    {
                        var exist = await Task.Run(() => entity.BillingAddresses.FirstOrDefault(p => p.UserId == order.UserId && (string.IsNullOrEmpty(p.OrderId) || p.OrderId.Equals("0.0"))));
                        if (exist == null)
                        {
                            order.billingAddress.UserId = order.UserId;
                            order.billingAddress.CreatedDate = System.DateTime.Now;
                            entity.BillingAddresses.Add(order.billingAddress);
                        }
                        else
                        {
                            exist.AddLineOne = order.billingAddress.AddLineOne;
                            exist.AddLineTwo = order.billingAddress.AddLineTwo;
                            exist.City = order.billingAddress.City;
                            exist.Country = order.billingAddress.Country;
                            exist.Email = order.billingAddress.Email;
                            exist.Firstname = order.billingAddress.Firstname;
                            exist.lastname = order.billingAddress.lastname;
                            exist.Mobile = order.billingAddress.Mobile;
                            exist.PostCode = order.billingAddress.PostCode;
                            exist.State = order.billingAddress.State;
                            //exist.CreatedDate = System.DateTime.Now;

                        }
                        await entity.SaveChangesAsync();

                        responseDetail.Message = "Address saved successfully.";
                        responseDetail.Status = true;
                    }
                }
                else if (operation == "CreateOrder")
                {
                    var exist = await Task.Run(() => entity.BillingAddresses.FirstOrDefault(p => p.UserId == order.UserId && (string.IsNullOrEmpty(p.OrderId) || p.OrderId.Equals("0.0"))));
                    if (exist == null)
                    {
                        responseDetail.Message = "Address detail not found.";
                    }
                    else
                    {
                        exist.OrderId = "DT" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        var Userid = Convert.ToString(order.UserId);
                        var OrderRefNo = await Task.Run(() => entity.TblOrderRefNoes.FirstOrDefault(p => p.Userid == Userid && p.RefNo == exist.OrderId));

                        if (OrderRefNo == null)
                        {
                            TblOrderRefNo data = new TblOrderRefNo()
                            {
                                Userid = Userid,
                                RefNo = exist.OrderId,
                                Created = System.DateTime.Now
                            };

                            entity.TblOrderRefNoes.Add(data);
                            await entity.SaveChangesAsync();
                        }
                        responseDetail = await CreateOrderReqModel(order, responseDetail, exist);
                        if (responseDetail == null)
                        {
                            responseDetail.Message = "Something went wrong. Please try again later.";
                        }
                        else
                        {
                            //save order responce in DB
                            if (responseDetail.Status)
                            {
                                //in case of successfull create order.
                                if (responseDetail.CreateOrderResponceDetail != null)
                                {
                                    var orderDetail = new Order();
                                    if (responseDetail.CreateOrderResponceDetail.cancel != null)
                                    {
                                        orderDetail.CancelAllowed = responseDetail.CreateOrderResponceDetail.cancel.allowed;
                                        orderDetail.CancelallowedWithIn = responseDetail.CreateOrderResponceDetail.cancel.allowedWithIn;
                                    }

                                    orderDetail.orderId = responseDetail.CreateOrderResponceDetail.orderId;
                                    orderDetail.refno = responseDetail.CreateOrderResponceDetail.refno;
                                    orderDetail.status = responseDetail.CreateOrderResponceDetail.status;
                                    orderDetail.ResponceContent = responseDetail.orderContent;
                                    orderDetail.UserId = Convert.ToString(order.UserId);
                                    orderDetail.Created = System.DateTime.Now;
                                    entity.Orders.Add(orderDetail);
                                    await entity.SaveChangesAsync();
                                    if (responseDetail.CreateOrderResponceDetail.payments != null)
                                    {
                                        var pay = new OrderPayment();
                                        foreach (var paymt in responseDetail.CreateOrderResponceDetail.payments)
                                        {
                                            pay = new OrderPayment();
                                            pay.balance = string.IsNullOrEmpty(paymt.balance) ? 0 : Convert.ToDecimal(paymt.balance);
                                            pay.code = paymt.code;
                                            pay.OrderRefNo = exist.OrderId;
                                            pay.Created = System.DateTime.Now;
                                            entity.OrderPayments.Add(pay);
                                        }
                                        await entity.SaveChangesAsync();

                                    }

                                    if (responseDetail.CreateOrderResponceDetail.cards != null)
                                    {

                                        var cartRes1 = await this.ManageCart(new CarteDetail { UserId = order.UserId }, "UpdteCartAftrOdrComplete");

                                        var cards = new order_card();

                                        foreach (var crd in responseDetail.CreateOrderResponceDetail.cards)
                                        {
                                            cards = new order_card();
                                            if (crd.activationCode == null)
                                            {
                                                cards.ActivationCode = "";
                                            }
                                            else
                                            {
                                                cards.ActivationCode = crd.activationCode.ToString();
                                            }
                                            if (crd.activationUrl == null)
                                            {
                                                cards.ActivationUrl = "";
                                            }
                                            else
                                            {
                                                cards.ActivationUrl = crd.activationUrl.ToString();
                                            }


                                            cards.Amount = crd.amount;
                                            if (crd.barcode == null)
                                            {
                                                cards.barcode = "";
                                            }
                                            else
                                            {
                                                cards.barcode = crd.barcode.ToString();
                                            }

                                            cards.CardId = crd.cardId;
                                            cards.CardNumber = crd.cardNumber;
                                            cards.EnrptCardNo = Encrypt(cards.CardNumber, KeyByte, IVByte);
                                            var decr = Decrypt(cards.EnrptCardNo, KeyByte, IVByte);
                                            cards.CardPin = crd.cardPin;
                                            cards.EnrptCardPin = Encrypt(cards.CardPin, KeyByte, IVByte);
                                            var d = Decrypt(cards.EnrptCardPin, KeyByte, IVByte);
                                            cards.LabelProductName = crd.productName;
                                            if (crd.labels != null)
                                            {
                                                cards.LableCardNumber = crd.cardNumber;
                                                cards.LableCardPin = crd.cardPin;
                                                if (crd.validity == null)
                                                {
                                                    cards.lableValidity = "";
                                                }
                                                else
                                                {
                                                    cards.lableValidity = crd.validity.ToString();
                                                }
                                                
                                            }

                                            cards.OrderRefNo = exist.OrderId;
                                            cards.ProductSku = crd.sku;
                                            cards.ProductTheme = crd.theme;
                                            if (crd.recipientDetails != null)
                                            {
                                                cards.RecepientEmail = crd.recipientDetails.email;
                                            }
                                            if (crd.validity == null)
                                            {
                                                cards.Validity = "";
                                            }
                                            else
                                            {
                                                cards.Validity = crd.validity.ToString();
                                            }
                                                
                                            cards.Created = System.DateTime.Now;
                                            entity.order_card.Add(cards);

                                            //sms = "Dear" + addressdetail.Firstname + " Your Pin No is" + crd.cardPin + "and Card No" + crd.cardNumber + "and validity" + crd.validity.ToString() + "";
                                            //  var  statusId = SaveJobSMSQue(addressdetail.UserId), sms, Convert.ToString(txtMobileNo.Value), "Login OTP", Convert.ToString(Session["SponsorFormNo"]));

                                        }
                                        await entity.SaveChangesAsync();
                                        string sms = string.Empty;
                                        string productsku = string.Empty;
                                        string cardNo = string.Empty;
                                        string pinNo = string.Empty;
                                        string Amount = string.Empty;
                                        string validity = string.Empty;
                                        var addressdetail = await Task.Run(() => entity.BillingAddresses.FirstOrDefault(p => p.UserId == order.UserId && p.OrderId == exist.OrderId));

                                        sms = " you have  just received your gift voucher.your code:";
                                        foreach (var item in responseDetail.CreateOrderResponceDetail.cards)
                                        {
                                            productsku = item.sku;
                                            cardNo = item.cardNumber;
                                            pinNo = item.cardPin; ;
                                            Amount = item.amount;
                                            validity = Convert.ToDateTime(item.validity).ToString("dd/MM/yyyy");
                                            sms += "voucher code" + cardNo + "Activition Pin: voucher pin" + cardNo + " of Rs." + Amount + "will Expire on " + validity + "Use the given detail for redeeming at BIgbazar uBRe";
                                        }
                                        string sms1 = sms;

                                        var statusId = SaveJobSMSQue(Convert.ToInt32(addressdetail.UserId), sms1, Convert.ToString(addressdetail.Mobile.Replace("91", "")), "giftVoucher", "1");

                                        string msgbody = string.Empty;
                                        var user = await Task.Run(() => entity.Users.FirstOrDefault(p => p.Id == order.UserId ));
                                        string URL = System.Web.HttpContext.Current.Request.Url.Host.ToUpper().Replace("HTTP://", "").Replace("HTTPS://", "").Replace("WWW.", "").Replace("/", "").Replace("Utility.", "").Replace("Care.", "");// System.Web.HttpContext.Current.Request.UserHostName;
                                        msgbody = "<div class='container'><h3> Hello " + user.Firstname +""+user.Lastname+"("+user.Username+")!!</h3>";
                                        msgbody += "<p>You have just received your gift voucher worth Rs." + responseDetail.CreateOrderResponceDetail.cards.Sum(s => Convert.ToDecimal(s.amount)) + ".Please find the details of E-gift Voucher and use the given details for redemption at respective shop.</p></div>";
                                        msgbody += "<div class='col-md-12'> <table cellpadding='5' cellspacing='1' border='2' style='font-family:Arial'><thead style='background-color:Aqua;text-transform:uppercase;font-weight:bold;font-size:13px'><tr align='center'><th>Amount</th><th>ForUse</th><th>VoucherCode</th><th>Pin</th><th>Expiry</th></tr></thead>";
                                        msgbody += "<tbody style='font-size:12px'>";
                                        foreach (var data in responseDetail.CreateOrderResponceDetail.cards)
                                        {
                                            msgbody += "<tr align='center'><td>" + Convert.ToString(data.amount) + "</td>";
                                            msgbody += "<td>" + data.sku + "</td>";
                                            msgbody += "<td>" + data.cardNumber + "</td>";
                                            msgbody += "<td>" + data.cardPin + "</td>";
                                            msgbody += "<td>" + Convert.ToDateTime(data.validity).ToString("dd/MM/yyyy") + "</td>";
                                            msgbody += "</tr>";
                                        }
                                        msgbody += "</tbody></table></div></div>";

                                        var status = SaveJobEmailQue(Convert.ToInt32(addressdetail.UserId), msgbody, Convert.ToString(addressdetail.Email), "Gift Voucher", Convert.ToInt32(1), "PineShop more E-Gift Voucher Order no" + exist.OrderId, "");
                                    }
                                        //var cartRes1 = await this.ManageCart(new CarteDetail { UserId = order.UserId }, "UpdteCartAftrOdrComplete");
                                        //await entity.SaveChangesAsync();
                                        if (responseDetail.CreateOrderResponceDetail.products != null)
                                        {

                                            var op = new order_product();
                                            var orderProd = new Product();
                                            //                                        await entity.SaveChangesAsync();

                                        }
                                        if (responseDetail.CreateOrderResponceDetail.status == "COMPLETE")
                                        {
                                            responseDetail.Message = "Order created successfully RefNo. " + exist.OrderId;

                                            //

                                        }
                                        else if (responseDetail.CreateOrderResponceDetail.status == "PROCESSING")
                                        {
                                            responseDetail.Message = " Please wait 5 to 10 min Still we could not Process Your order RefNo. " + exist.OrderId;
                                        }


                                    //}
                                }
                            }


                        }
                    }
                }
            }
            catch (Exception e)
            {
                responseDetail.Message = e.Message;
            }

            return responseDetail;
        }

       

        public int SaveJobEmailQue(int formNo, string emailBody, string emailId, string emailType, int companyId, string subject, string fromEmailID)
        {
            int SMSSTATUS = 0;
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@formNo", formNo),
                new SqlParameter("@EMailBody", emailBody),
                new SqlParameter("@EmailId", emailId),
                new SqlParameter("@EmailType", emailType),
                new SqlParameter("@CompanyId", companyId),
                new SqlParameter("@Subject", subject),
                new SqlParameter("@FromEmailID", fromEmailID)
          };
            //Lets get the list of all employees in a datataable
           
               string CONNECTION_STRING = LiveCONNECTION_STRING;
            
            using (DataSet ds = SqlHelper.ExecuteDataset(CONNECTION_STRING, "sp_JobEmailQue", parameters))
            {
                //check if any record exist or not
                if (ds.Tables[0].Rows.Count > 0)
                {
                    SMSSTATUS = Convert.ToInt32(ds.Tables[0].Rows[0]["EMAILSTATUS"]);
                }
            }
            return SMSSTATUS;
        }


        public int SaveJobSMSQue(int formNo, string smsBody, string mobileNo, string smsType, string companyId)
        {
            int SMSSTATUS = 0;
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@formNo", formNo),
                new SqlParameter("@smsBody", smsBody),
                new SqlParameter("@mobileNo", mobileNo),
                new SqlParameter("@smsType", smsType),
                new SqlParameter("@companyId", companyId)
          };
            //Lets get the list of all employees in a datataable
           
              string  CONNECTION_STRING = LiveCONNECTION_STRING;
            
            using (DataSet ds = SqlHelper.ExecuteDataset(CONNECTION_STRING, "sp_JobSMSQue", parameters))
            {
                //check if any record exist or not
                if (ds.Tables[0].Rows.Count > 0)
                {
                    SMSSTATUS = Convert.ToInt32(ds.Tables[0].Rows[0]["SMSSTATUS"]);
                }
            }
            return SMSSTATUS;
        }


        private async Task<ResponceDetail> CreateOrderReqModel(OrderContainer order, ResponceDetail responseDetail, BillingAddress exist)
        {
            var orderReq = new OrderRequestModel();
            orderReq.address = new Address();
            exist.Country = "IN";
            exist.Mobile = "+91" + exist.Mobile;
            orderReq.address.line1 = exist.AddLineOne;
            orderReq.address.line2 = exist.AddLineTwo;
            orderReq.address.city = exist.City;
            orderReq.address.country = exist.Country;
            orderReq.address.email = exist.Email;
            orderReq.address.firstname = exist.Firstname;
            orderReq.address.lastname = exist.lastname;
            orderReq.address.telephone = exist.Mobile;
            orderReq.address.postcode = exist.PostCode;
            orderReq.address.region = exist.State;
            orderReq.address.billToThis = true;
            orderReq.billing = new Billing();
            orderReq.billing.line1 = exist.AddLineOne;
            orderReq.billing.line2 = exist.AddLineTwo;
            orderReq.billing.city = exist.City;
            orderReq.billing.country = exist.Country;
            orderReq.billing.email = exist.Email;
            orderReq.billing.firstname = exist.Firstname;
            orderReq.billing.lastname = exist.lastname;
            orderReq.billing.telephone = exist.Mobile;
            orderReq.billing.postcode = exist.PostCode;
            orderReq.billing.region = exist.State;

            var cartRes = await this.ManageCart(new CarteDetail { UserId = order.UserId }, "ListCartByUserId");
            if (cartRes != null && cartRes.CartList != null && cartRes.CartList.Count > 0)
            {
                orderReq.payments = new List<Payment1>();
                var total = cartRes.CartList.Sum(p => p.TotalPrice);
                 var totalQty= cartRes.CartList.Sum(p => p.Quantity);
                orderReq.payments.Add(new Payment1 { code = "svc", amount = total.ToString() });
                orderReq.products = new List<Product1>();
                var prod = new Product1();
                foreach (var cProd in cartRes.CartList)
                {
                    prod = new Product1();
                    prod.giftMessage = "";
                    prod.price = Convert.ToInt32(cProd.ProdPrice);
                    prod.qty = cProd.Quantity ?? 1;
                    prod.sku = cProd.ProductSku;
                    prod.theme = "";

                    //this code can be removed when we have currency in cart detail table.
                    var wohooProd = await Task.Run(() => entity.WohooProductLists.FirstOrDefault(p => p.Sku == cProd.ProductSku));
                    if (wohooProd != null)
                    {
                        prod.currency = Convert.ToInt32(wohooProd.NumericCode);
                    }

                    orderReq.products.Add(prod);
                }

                orderReq.refno = exist.OrderId;
                // orderReq.refno = "DT20210331181838274";
                orderReq.syncOnly = totalQty >= 4 ? false : true;//false;//true;
                orderReq.deliveryMode = "API";

                var pinerepo = new PineLabRepository();
                var reqModel = new RequestModel();
                reqModel.orderRequestDetail = orderReq;
                responseDetail = await pinerepo.CreateOrder(reqModel);

            }

            return responseDetail;
        }

        public async Task<ResponceDetail> ManageCategory(Filters filterDetail)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                if (filterDetail.Operation == "Refresh")
                {
                    PineLabRepository repo = new PineLabRepository();
                    var requestModel = new RequestModel();
                    requestModel.CompanyId = filterDetail.CompanyId;
                    var res = await repo.GetCategoryList(requestModel);
                    if (res.CategoryList == null || !res.Status)
                    {
                        if (string.IsNullOrEmpty(res.Message))
                        {
                            res.Message = "Somehing went wrong. Please try agian later.";
                        }
                        responseDetail.Message = res.Message;
                    }
                    else
                    {
                        var imageList = new List<Image>();
                        var image = new Image();

                        var cat = new Category();
                        var dbCatList = new List<Category>();
                        cat.Name = res.CategoryList.name;
                        cat.ParentId = res.CategoryList.id;
                        cat.ApiCatId = res.CategoryList.id;
                        cat.Description = Convert.ToString(res.CategoryList.description);
                        cat.SubcategoriesCount = res.CategoryList.subcategoriesCount;
                        cat.Url = res.CategoryList.url;
                        cat.Status = true;
                        if (res.CategoryList.images != null && !(string.IsNullOrEmpty(res.CategoryList.images.image) && string.IsNullOrEmpty(res.CategoryList.images.thumbnail)))
                        {
                            image = new Image();
                            //Using base field to save image of category to avoing adding a new column in table.
                            image.@base = res.CategoryList.images.image;
                            image.thumbnail = res.CategoryList.images.thumbnail;
                            image.CategoryId = res.CategoryList.id;
                            imageList.Add(image);
                        }

                        dbCatList.Add(cat);

                        foreach (var c in res.CategoryList.subcategories)
                        {
                            cat = new Category();
                            cat.ApiCatId = c.id;
                            cat.Description = Convert.ToString(c.description);
                            cat.Name = c.name;
                            cat.ParentId = res.CategoryList.id;
                            cat.SubcategoriesCount = c.subcategoriesCount;
                            cat.Url = c.url;
                            cat.Status = true;


                            dbCatList.Add(cat);

                            if (c.images != null && !(string.IsNullOrEmpty(c.images.image) && string.IsNullOrEmpty(c.images.thumbnail)))
                            {
                                image = new Image();
                                //Using base field to save image of category to avoing adding a new column in table.
                                image.@base = c.images.image;
                                image.thumbnail = c.images.thumbnail;
                                image.CategoryId = c.id;
                                imageList.Add(image);
                            }
                        }

                        //insert category first.
                        var mainXml = string.Empty;
                        mainXml = Serialize(dbCatList);
                        await Task.Run(() => entity.InsertCategory(mainXml));

                        //Insert category images.
                        if (imageList != null && imageList.Count > 0)
                        {
                            //Insert images
                            mainXml = string.Empty;
                            mainXml = Serialize(imageList);
                            await Task.Run(() => entity.InsertCategoryImageList(mainXml));
                        }

                        responseDetail.Message = "Categories Refresh successfully.";
                        responseDetail.Status = true;
                    }

                    //var cateList = new List<Category>();
                    //foreach(var cat in catList)
                }
                else if (filterDetail.Operation == "Category List")
                {
                    responseDetail.TotalRecordCount = await Task.Run(() => entity.Categories.Count());
                    responseDetail.CatList = await Task.Run(() => entity.GetCategoryListWithFilter(filterDetail.pageIndex, filterDetail.RecordCount).ToList());
                    responseDetail.Status = true;
                }
                else if (filterDetail.Operation == "AllCategory")
                {
                    responseDetail.TotalRecordCount = await Task.Run(() => entity.Categories.Count());
                    //****Sp will return only active categories.
                    responseDetail.CatList = await Task.Run(() => entity.GetAllCategoryList().ToList());
                    responseDetail.Status = true;
                }
            }
            catch (Exception e)
            {
                responseDetail.Message = e.Message;
            }

            return responseDetail;
        }

      

        public async Task<ResponceDetail> ManageProducts(Filters filterDetail)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                if (filterDetail.Operation == "Refresh")
                {
                    await RefreshProductFunction(filterDetail, responseDetail);
                }
                else if (filterDetail.Operation == "Product List")
                {
                    ///*****this sp will give products which are fetch directly from wohoo api.
                    var count = await Task.Run(() => entity.GetCountProductListWithFilter(string.Empty, string.Empty));
                    responseDetail.TotalRecordCount = count.FirstOrDefault() ?? 0;
                    responseDetail.ResultProdList = await Task.Run(() => entity.GetProductListWithFilter(filterDetail.pageIndex, filterDetail.RecordCount, string.Empty, string.Empty).ToList());
                    responseDetail.Status = true;
                }
                else if (filterDetail.Operation == "ProductListByFilter")
                {
                    ///*****this sp will give products which are fetch directly from wohoo api by cat id.
                    var count = await Task.Run(() => entity.GetCountProductListWithFilter(filterDetail.Action, filterDetail.Id.ToString()));
                    responseDetail.TotalRecordCount = count.FirstOrDefault() ?? 0;
                    responseDetail.ResultProdList = await Task.Run(() => entity.GetProductListWithFilter(filterDetail.pageIndex, filterDetail.RecordCount, filterDetail.Action, filterDetail.Id.ToString()).ToList());
                    responseDetail.Status = true;
                }
                else if (filterDetail.Operation == "ProductDetailBySku")
                {
                    //*** call sp to get detail
                    var prodContainer = GetDetailFromDB(filterDetail);
                    if (prodContainer == null || prodContainer.ProductDetail == null)
                    {
                        //***** Sp null then call api and insert in DB
                        responseDetail = await GetDetailAndInsertInDB(filterDetail);
                    }

                    if (responseDetail.Status)
                    {
                        //**** Again call sp to get detail.
                        prodContainer = GetDetailFromDB(filterDetail);
                    }

                    if (prodContainer != null)
                    {
                        responseDetail.ProductDetail = null;
                        responseDetail.ProdDetailContainer = prodContainer;
                        responseDetail.Status = true;
                    }
                    else
                    {
                        responseDetail.Message = "No product detail found..";
                        responseDetail.Status = false;
                    }
                }
            }
            catch (Exception e)
            {
                responseDetail.Message = e.Message;
            }

            return responseDetail;
        }

        public async Task<ResponceDetail> ManageCart(CarteDetail cartDetail, string Operation)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                if (Operation == "AddToCart")
                {
                    if (cartDetail == null || string.IsNullOrEmpty(cartDetail.ProductSku))
                    {
                        responseDetail.Message = "Please send complete detail.";
                    }
                    else
                    {
                        // first check prod sku, price type, prod price.
                        //if same then update qty else add new row.
                        if (cartDetail.Quantity == 0)
                        {
                            cartDetail.Quantity = 1;
                        }
                        cartDetail.TotalPrice = cartDetail.ProdPrice * cartDetail.Quantity;
                        cartDetail.Created = System.DateTime.Now;
                        cartDetail.Posted = "1";
                        var alreadyExist = await Task.Run(() => entity.CarteDetails.FirstOrDefault(c => c.ProductSku == cartDetail.ProductSku && c.PriceType == cartDetail.PriceType && c.ProdPrice == cartDetail.ProdPrice && c.Posted=="1"));
                        if (alreadyExist == null)
                        {
                            responseDetail.Message = "Product Added into cart.";

                            entity.CarteDetails.Add(cartDetail);
                        }
                        else
                        {
                            responseDetail.Message = "Product Updated into cart.";
                            alreadyExist.Quantity = alreadyExist.Quantity + cartDetail.Quantity;
                            alreadyExist.TotalPrice = cartDetail.ProdPrice * alreadyExist.Quantity;
                        }

                        await entity.SaveChangesAsync();
                        responseDetail.Status = true;
                        //add here in db table.
                    }
                }
                if (Operation == "CartCount")
                {
                    responseDetail.CartProductCount = await Task.Run(() => entity.GetCartProductCountByUserId(cartDetail.UserId).FirstOrDefault() ?? 0);
                    responseDetail.Status = true;
                    responseDetail.Message = "Success!!";
                }
                else if (Operation == "ListCartByUserId")
                {
                    responseDetail.CartList = await Task.Run(() => entity.GetCartListByUserId(cartDetail.UserId).ToList());
                    responseDetail.Status = true;
                }
                else if (Operation == "UpdteCartAftrOdrComplete")
                {

                    var Result = (from ord in entity.CarteDetails
                                  where ord.UserId == cartDetail.UserId && ord.Posted=="1"
                                  select ord).ToList();

                    foreach (var item in Result)
                    {
                        item.Posted = "0";
                    }
                    await entity.SaveChangesAsync();
                    await entity.SaveChangesAsync();
                }
                else if (Operation == "DeleteCart" || Operation == "UpdateQuantity")
                {
                    var cart = await Task.Run(() => entity.CarteDetails.FirstOrDefault(c => c.Id == cartDetail.Id && c.Posted=="1"));
                    if (cart == null)
                    {
                        responseDetail.Message = "Product Not Fount.";
                    }
                    else
                    {
                        if (Operation == "DeleteCart")
                        {
                            entity.CarteDetails.Remove(cart);
                            await entity.SaveChangesAsync();
                            responseDetail.Message = "Product Deleted Successfully.";
                        }
                        else if (Operation == "UpdateQuantity")
                        {
                            if (cartDetail.Quantity == 0)
                            {
                                cartDetail.Quantity = 1;
                            }
                            cart.Quantity = cartDetail.Quantity;
                            cart.TotalPrice = cart.ProdPrice * cart.Quantity;
                        }
                    }
                    //*** call sp to get detail
                }

            }
            catch (Exception e)
            {
                responseDetail.Message = e.Message;
            }

            if (Operation != "CartCount")
            {
                responseDetail.CartProductCount = await Task.Run(() => entity.GetCartProductCountByUserId(cartDetail.UserId).FirstOrDefault() ?? 0);
            }
            return responseDetail;
        }

        //***This function will refresh all category, all prod list by catagory, All product detail get sku from prod list. 
        public async Task<ResponceDetail> RefreshFullSystemFromWohoo()
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {

                PineLabRepository repo = new PineLabRepository();

                //first refresh categories
                var filter = new Filters();
                filter.Operation = "Refresh";
                responseDetail = await this.ManageCategory(filter);
                if (responseDetail.Status)
                {
                    //get all cat list from DB
                    var catLst = await Task.Run(() => entity.GetAllCategoryList().ToList());
                    if (catLst != null && catLst.Count > 0)
                    {
                        //**** All prod list based on cat id.
                        foreach (var cat in catLst)
                        {
                            filter.Id = Convert.ToInt32(cat.ApiCatId);
                            responseDetail = await this.ManageProducts(filter);
                            if (!responseDetail.Status)
                            {
                                Console.WriteLine(responseDetail.Message);
                            }
                        }

                        //****Refresh prod detail based on sku.
                        int page = 1, records = 50;
                        var prodList = await Task.Run(() => entity.GetProductListWithFilter(page, records, string.Empty, string.Empty).ToList());
                        while (prodList != null && prodList.Count() > 0)
                        {
                            foreach (var prod in prodList)
                            {
                                filter.Sku = prod.sku;
                                responseDetail = await GetDetailAndInsertInDB(filter);
                            }
                            page++;
                            prodList = await Task.Run(() => entity.GetProductListWithFilter(page, records, string.Empty, string.Empty).ToList());
                        }

                        responseDetail.Message = "All System refresh successfully.";
                    }
                }
            }
            catch (Exception e)
            {
                responseDetail.Message = e.Message;
            }

            return responseDetail;
        }

        private async Task<ResponceDetail> GetDetailAndInsertInDB(Filters filterDetail)
        {
            var res = new ResponceDetail();
            PineLabRepository repo = new PineLabRepository();
            var requestModel = new RequestModel();
            try
            {
                requestModel.Sku = filterDetail.Sku;
                res = await repo.GetProductDetailBySku(requestModel);
                if (res.ProductDetail == null || !res.Status)
                {
                    res.Message = "Product Detail not Found on Wohoo.";
                }
                else
                {
                    SqlParameter ImgParameter = GetProductImageData(res);
                    SqlParameter proCatParameter = GetProductCategoryData(res);
                    SqlParameter contentParameter = GetProductContentData(res);
                    SqlParameter denominationParameter = GetProductDenominationData(res);
                    SqlParameter discountParameter = GetProductDiscountData(res);
                    SqlParameter themeParameter = GetProductThemeData(res);
                    SqlParameter relatedProdParameter = GetRelatedProductIdData(res);
                    SqlParameter prodDetail = GetProductDetailData(res);
                    //var paramArr = new List<SqlParameter>();
                    //paramArr.Add(ImgParameter);
                    //paramArr.Add(proCatParameter);
                    //paramArr.Add(contentParameter);
                    //paramArr.Add(denominationParameter);
                    //paramArr.Add(discountParameter);
                    //paramArr.Add(themeParameter);
                    //paramArr.Add(relatedProdParameter);
                    //paramArr.Add(prodDetail);

                    using (PineLabsEntities db = new PineLabsEntities())
                    {
                        db.Database.ExecuteSqlCommand("exec " + InsertWohooProductImageDetaiSP, ImgParameter);
                        db.Database.ExecuteSqlCommand("exec " + InsertProductCategoryRelationListSP, proCatParameter);
                        db.Database.ExecuteSqlCommand("exec " + InsertProductContentSP, contentParameter);
                        db.Database.ExecuteSqlCommand("exec " + InsertProductDenominationSP, denominationParameter);
                        db.Database.ExecuteSqlCommand("exec " + InsertProductDiscountsSP, discountParameter);
                        db.Database.ExecuteSqlCommand("exec " + InsertProductThemeSP, themeParameter);
                        db.Database.ExecuteSqlCommand("exec " + InsertRelatedProductListSP, relatedProdParameter);
                        db.Database.ExecuteSqlCommand("exec " + InsertProductDetailSP, prodDetail);
                        res.Status = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                res.Status = false;
            }

            return res;
        }

        private static Image GetProductImageDataEF(ResponceDetail res)
        {
            var imgDetail = new Image();
            //call to insert in DB

            if (res.ProductDetail.images != null)
            {
                imgDetail.thumbnail = res.ProductDetail.images.thumbnail;
                imgDetail.@base = res.ProductDetail.images.@base;
                imgDetail.small = res.ProductDetail.images.small;
                imgDetail.mobile = res.ProductDetail.images.mobile;
                imgDetail.ProductId = res.ProductDetail.sku;
                imgDetail.CategoryId = "0";
            }

            return imgDetail;
        }

        private static SqlParameter GetProductImageData(ResponceDetail res)
        {
            //call to insert in DB
            var imageDT = new DataTable();

            imageDT.Columns.Add("thumbnail", typeof(string));
            imageDT.Columns.Add("mobile", typeof(string));
            imageDT.Columns.Add("base", typeof(string));
            imageDT.Columns.Add("small", typeof(string));
            imageDT.Columns.Add("ProductId", typeof(string));
            imageDT.Columns.Add("CategoryId", typeof(string));
            if (res.ProductDetail.images != null)
            {
                imageDT.Rows.Add(res.ProductDetail.images.thumbnail, res.ProductDetail.images.@base, res.ProductDetail.images.small, res.ProductDetail.images.mobile, res.ProductDetail.sku, "0");
            }
            var parameter = new SqlParameter("@pImages", SqlDbType.Structured);
            parameter.Value = imageDT;
            parameter.TypeName = "dbo.ImagesUDT";
            return parameter;
        }

        private static List<ProductCategoryRelationTbl> GetProductCategoryDataEF(ResponceDetail res)
        {
            //call to insert in DB
            var category = new ProductCategoryRelationTbl();
            var catList = new List<ProductCategoryRelationTbl>();

            if (res.ProductDetail.categories != null && res.ProductDetail.categories.Count > 0)
            {
                foreach (var cat in res.ProductDetail.categories)
                {
                    category = new ProductCategoryRelationTbl();
                    category.ProductSku = res.ProductDetail.sku;
                    category.CategoryId = cat;
                    catList.Add(category);
                }
            }

            return catList;
        }

        private static SqlParameter GetProductCategoryData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("ProductSku", typeof(string));
            dt.Columns.Add("CategoryId", typeof(string));

            if (res.ProductDetail.categories != null && res.ProductDetail.categories.Count > 0)
            {
                foreach (var cat in res.ProductDetail.categories)
                {
                    dt.Rows.Add(res.ProductDetail.sku, cat);
                }
            }
            var parameter = new SqlParameter("@pProdCat", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.ProductCategoryRelationUSD";
            return parameter;
        }

        private static ProductContentTbl GetProductContentDataEF(ResponceDetail res)
        {
            var prodContent = new ProductContentTbl();

            if (res.ProductDetail.tnc != null)
            {
                var cont = String.Join(",", res.ProductDetail.tnc.content);

                prodContent = new ProductContentTbl();
                prodContent.ProductSku = res.ProductDetail.sku;
                prodContent.ProdContent = res.ProductDetail.tnc.content;

            }

            return prodContent;
        }

        private static SqlParameter GetProductContentData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("ProductSku", typeof(string));
            dt.Columns.Add("ProdContent", typeof(string));

            if (res.ProductDetail.tnc != null)
            {
                dt.Rows.Add(res.ProductDetail.sku, res.ProductDetail.tnc.content);
            }

            var parameter = new SqlParameter("@pProdContent", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.ProductContentUSD";
            return parameter;
        }

        private static SqlParameter GetProductDenominationData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("ProductSku", typeof(string));
            dt.Columns.Add("Denomination", typeof(string));

            if (res.ProductDetail.price != null && res.ProductDetail.price.denominations != null && res.ProductDetail.price.denominations.Count > 0)
            {
                var cont = String.Join(",", res.ProductDetail.price.denominations);
                dt.Rows.Add(res.ProductDetail.sku, cont);
            }

            var parameter = new SqlParameter("@pProdDenomination", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.ProductDenominationUSD";
            return parameter;
        }

        private static ProductDenomination GetProductDenominationDataEF(ResponceDetail res)
        {
            //call to insert in DB
            var prodDenom = new ProductDenomination();

            if (res.ProductDetail.price.denominations != null && res.ProductDetail.price.denominations != null && res.ProductDetail.price.denominations.Count > 0)
            {
                var cont = String.Join(",", res.ProductDetail.price.denominations);
                prodDenom.ProductSku = res.ProductDetail.sku;
                prodDenom.Denomination = cont;
            }

            return prodDenom;
        }

        private static SqlParameter GetProductDiscountData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("ProductSku", typeof(string));
            dt.Columns.Add("DiscountType", typeof(string));
            dt.Columns.Add("StartDate", typeof(DateTime));
            dt.Columns.Add("EndDate", typeof(DateTime));
            dt.Columns.Add("DiscountAmount", typeof(decimal));
            dt.Columns.Add("DiscountDesc", typeof(string));
            dt.Columns.Add("CounponCode", typeof(string));
            dt.Columns.Add("CouponPriority", typeof(int));

            if (res.ProductDetail.discounts != null && res.ProductDetail.discounts.Count > 0)
            {
                foreach (var disc in res.ProductDetail.discounts)
                {
                    if (disc.discount != null)
                    {
                        dt.Rows.Add(res.ProductDetail.sku, disc.discount.type, disc.startDate, disc.endDate, disc.discount.amount, disc.discount.desc, disc.coupon, disc.priority);
                    }
                }
            }

            var parameter = new SqlParameter("@pProdDiscount", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.ProductDiscountsUSD";
            return parameter;
        }

        private static List<ProductDiscount> GetProductDiscountDataEF(ResponceDetail res)
        {
            //call to insert in DB
            var dis = new ProductDiscount();
            var disList = new List<ProductDiscount>();

            if (res.ProductDetail.discounts != null && res.ProductDetail.discounts.Count > 0)
            {
                foreach (var disc in res.ProductDetail.discounts)
                {
                    if (disc.discount != null)
                    {
                        dis = new ProductDiscount();
                        dis.ProductSku = res.ProductDetail.sku;
                        dis.DiscountType = disc.discount.type;
                        dis.StartDate = disc.startDate;
                        dis.EndDate = disc.endDate;
                        dis.DiscountAmount = disc.discount.amount;
                        dis.DiscountDesc = disc.discount.desc;
                        dis.CounponCode = disc.coupon.code;
                        dis.CouponPriority = disc.priority;
                        disList.Add(dis);
                    }
                }
            }

            return disList;
        }

        private static SqlParameter GetProductThemeData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("ProductSku", typeof(string));
            dt.Columns.Add("Price", typeof(string));
            dt.Columns.Add("Image", typeof(string));
            dt.Columns.Add("ThemeSku", typeof(string));

            if (res.ProductDetail.themes != null && res.ProductDetail.themes.Count > 0)
            {
                foreach (var theme in res.ProductDetail.themes)
                {
                    dt.Rows.Add(res.ProductDetail.sku, theme.price, theme.image, theme.sku);
                }
            }

            var parameter = new SqlParameter("@pProdTheme", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.ProductThemeTblUSD";
            return parameter;
        }

        private static List<Theme> GetProductThemeDataEF(ResponceDetail res)
        {
            //call to insert in DB
            var thme = new Theme();
            var themeList = new List<Theme>();

            if (res.ProductDetail.themes != null && res.ProductDetail.themes.Count > 0)
            {
                foreach (var theme in res.ProductDetail.themes)
                {
                    thme.sku = res.ProductDetail.sku;
                    thme.price = theme.price;
                    thme.image = theme.image;
                    thme.sku = theme.sku;

                    themeList.Add(thme);
                }
            }

            return themeList;
        }

        private static SqlParameter GetRelatedProductIdData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("Sku", typeof(string));
            dt.Columns.Add("RelatedSku", typeof(string));

            if (res.ProductDetail.relatedProducts != null && res.ProductDetail.relatedProducts.Count > 0)
            {
                foreach (var pr in res.ProductDetail.relatedProducts)
                {
                    dt.Rows.Add(res.ProductDetail.sku, pr);
                }
            }

            var parameter = new SqlParameter("@pProrelated", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.RelatedProductListUSD";
            return parameter;
        }

        private static List<RelatedProductList> GetRelatedProductIdDataEF(ResponceDetail res)
        {
            //call to insert in DB
            var relatedProdId = new RelatedProductList();
            var relatedProdIdList = new List<RelatedProductList>();

            if (res.ProductDetail.relatedProducts != null && res.ProductDetail.relatedProducts.Count > 0)
            {
                foreach (var pr in res.ProductDetail.relatedProducts)
                {
                    relatedProdId = new RelatedProductList();
                    relatedProdId.Sku = res.ProductDetail.sku;
                    relatedProdId.RelatedSku = pr.sku;
                    relatedProdIdList.Add(relatedProdId);
                }
            }

            return relatedProdIdList;
        }

        private static SqlParameter GetProductData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("StartDate", typeof(DateTime));
            dt.Columns.Add("EndDate", typeof(DateTime));
            dt.Columns.Add("DiscountAmount", typeof(decimal));
            dt.Columns.Add("DiscountDesc", typeof(string));
            dt.Columns.Add("CounponCode", typeof(string));
            dt.Columns.Add("CouponPriority", typeof(int));

            if (res.ProductDetail.discounts != null && res.ProductDetail.discounts.Count > 0)
            {
                foreach (var disc in res.ProductDetail.discounts)
                {
                    if (disc.discount != null)
                    {
                        dt.Rows.Add(res.ProductDetail.sku, disc.discount.type, disc.startDate, disc.endDate, disc.discount.amount, disc.discount.desc, disc.coupon, disc.priority);
                    }
                }
            }

            var parameter = new SqlParameter("pProdDiscount", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.ProductDiscountsUSD";
            return parameter;
        }

        private static SqlParameter GetProductDetailData(ResponceDetail res)
        {
            //call to insert in DB
            var dt = new DataTable();

            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("PriceType", typeof(string));
            dt.Columns.Add("Min", typeof(decimal));
            dt.Columns.Add("Max", typeof(decimal));
            dt.Columns.Add("Code", typeof(string)); dt.Columns.Add("NumericCode", typeof(string));
            dt.Columns.Add("Symbol", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("ReloadCardNumber", typeof(bool));
            dt.Columns.Add("FormatExpiry", typeof(string));
            dt.Columns.Add("Sku", typeof(string));
            dt.Columns.Add("TncLink", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("VoucherType", typeof(string));

            //dt.Columns.Add("Type", typeof(string));

            if (res.ProductDetail != null)
            {
                dt.Rows.Add(res.ProductDetail.id, res.ProductDetail.description, res.ProductDetail.price.type,
                    res.ProductDetail.price.min, res.ProductDetail.price.max,
                    res.ProductDetail.price.currency.code, res.ProductDetail.price.currency.numericCode,
                    res.ProductDetail.price.currency.symbol, res.ProductDetail.type, res.ProductDetail.reloadCardNumber,
                    res.ProductDetail.formatExpiry, res.ProductDetail.sku, res.ProductDetail.tnc.link,
                    res.ProductDetail.name, res.ProductDetail.type);
            }

            var parameter = new SqlParameter("@pProdDetail", SqlDbType.Structured);
            parameter.Value = dt;
            parameter.TypeName = "dbo.WohooProductListUSD";
            return parameter;
        }

        private static WohooProductList GetProductDetailDataEF(ResponceDetail res)
        {
            //call to insert in DB
            var prodDetail = new WohooProductList();

            if (res.ProductDetail != null)
            {
                prodDetail.Id = res.ProductDetail.id;
                prodDetail.Sku = res.ProductDetail.sku;
                prodDetail.Name = res.ProductDetail.name;
                prodDetail.Description = res.ProductDetail.description;
                prodDetail.PriceType = res.ProductDetail.price.type;
                prodDetail.Min = res.ProductDetail.price.min;
                prodDetail.Max = res.ProductDetail.price.max;
                prodDetail.Code = res.ProductDetail.price.currency.code;
                prodDetail.Symbol = res.ProductDetail.price.currency.symbol;
                prodDetail.NumericCode = res.ProductDetail.price.currency.numericCode;
                prodDetail.VoucherType = res.ProductDetail.type;
                prodDetail.TncLink = res.ProductDetail.tnc.link;
                prodDetail.ReloadCardNumber = res.ProductDetail.reloadCardNumber;
                prodDetail.FormatExpiry = res.ProductDetail.formatExpiry;
            }

            return prodDetail;
        }

        private static ProductDetailContainer GetDetailFromDB(Filters filterDetail)
        {
            var prodDetailContainer = new ProductDetailContainer();
            try
            {
                var db = new PineLabsContext();
                using (var connection = db.Database.Connection)
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "EXEC [dbo].[GetProductDetailBySku] " + filterDetail.Sku;

                    using (var reader = command.ExecuteReader())
                    {
                        var productDetail =
                            ((IObjectContextAdapter)db)
                                .ObjectContext
                                .Translate<WohooProductList>(reader)
                                .ToList();

                        if (productDetail == null || productDetail.Count == 0)
                        {
                            prodDetailContainer.ProductDetail = null;
                        }
                        else
                        {
                            //Assign detail
                            prodDetailContainer.ProductDetail = productDetail.FirstOrDefault();

                            //Assign Images
                            reader.NextResult();
                            prodDetailContainer.ProductImages = ((IObjectContextAdapter)db).ObjectContext.Translate<Images>(reader).FirstOrDefault();

                            //Assign Category
                            reader.NextResult();
                            prodDetailContainer.ProductCategoryRelation = ((IObjectContextAdapter)db).ObjectContext.Translate<ProductCategoryRelationTbl>(reader).ToList();

                            //Assign content
                            reader.NextResult();
                            prodDetailContainer.ProductContentList = ((IObjectContextAdapter)db).ObjectContext.Translate<ProductContentTbl>(reader).ToList();

                            //Assign Denomination
                            reader.NextResult();
                            prodDetailContainer.ProductDenominationList = ((IObjectContextAdapter)db).ObjectContext.Translate<ProductDenomination>(reader).ToList();


                            //Assign Discount
                            reader.NextResult();
                            prodDetailContainer.ProductDiscountList = ((IObjectContextAdapter)db).ObjectContext.Translate<ProductDiscount>(reader).ToList();

                            //Assign Discount
                            reader.NextResult();
                            prodDetailContainer.RelatedProductList = ((IObjectContextAdapter)db).ObjectContext.Translate<ProductList>(reader).ToList();

                            if (prodDetailContainer.ProductDenominationList != null && prodDetailContainer.ProductDenominationList.Count > 0)
                            {
                                var denom = prodDetailContainer.ProductDenominationList.FirstOrDefault();
                                if (denom != null && !string.IsNullOrEmpty(denom.Denomination))
                                {
                                    var arr = denom.Denomination.Split(',');
                                    if (arr != null)
                                    {
                                        prodDetailContainer.ProductDenominationList = new List<ProductDenomination>();
                                        foreach (var arrValue in arr)
                                        {
                                            prodDetailContainer.ProductDenominationList.Add(new ProductDenomination
                                            {
                                                Denomination = arrValue
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                prodDetailContainer = null;
            }


            return prodDetailContainer;
        }

        private async Task RefreshProductFunction(Filters filterDetail, ResponceDetail responseDetail)
        {
            PineLabRepository repo = new PineLabRepository();
            var requestModel = new RequestModel();
            //requestModel.CompanyId = filterDetail.CompanyId;
            requestModel.CategoryId = filterDetail.Id;

            var res = await repo.GetProductListByCategoryId(requestModel);
            var currencyList = new List<Currency>();
            var imageList = new List<Image>();
            var image = new Image();
            if (res == null)
            {
                responseDetail.Message = "Something went wrong. Please try again later.";
            }
            else if (res.ProductList == null || !res.Status)
            {
                if (string.IsNullOrEmpty(res.Message))
                {
                    res.Message = "No Products Founr.";
                }

                responseDetail.Message = res.Message;
            }
            else
            {
                foreach (var prod in res.ProductList.products)
                {
                    prod.categoryId = res.ProductList.id;
                    if (prod.images != null)
                    {
                        if (!(string.IsNullOrEmpty(prod.images.@base) && string.IsNullOrEmpty(prod.images.mobile) && string.IsNullOrEmpty(prod.images.small) && string.IsNullOrEmpty(prod.images.thumbnail)))
                        {
                            image = new Image();
                            image.@base = prod.images.@base;
                            image.mobile = prod.images.mobile;
                            image.small = prod.images.small;
                            image.thumbnail = prod.images.thumbnail;
                            image.ProductId = prod.sku;
                            image.CategoryId = "0";
                            imageList.Add(image);
                        }
                    }

                    if (prod.currency != null)
                    {
                        prod.currency.sku = prod.sku;
                        currencyList.Add(prod.currency);
                    }
                }
                var mainXml = string.Empty;
                if (res.ProductList.products != null && res.ProductList.products.Count > 0)
                {
                    //first insert prod list

                    mainXml = Serialize(res.ProductList.products);
                    await Task.Run(() => entity.InsertProductList(mainXml));
                }

                if (currencyList != null && currencyList.Count > 0)
                {
                    //Insert currency
                    mainXml = string.Empty;
                    mainXml = Serialize(currencyList);
                    await Task.Run(() => entity.InsertProductCurrencyList(mainXml));
                }

                if (imageList != null && imageList.Count > 0)
                {
                    //Insert images
                    mainXml = string.Empty;
                    mainXml = Serialize(imageList);
                    await Task.Run(() => entity.InsertProductImageList(mainXml));

                }

                responseDetail.Message = "Products Uploaded successfully.";
                responseDetail.Status = true;
            }
        }

        public static string Serialize<T>(T dataToSerialize)
        {
            try
            {
                var stringwriter = new System.IO.StringWriter();
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(stringwriter, dataToSerialize);
                return stringwriter.ToString();
            }
            catch
            {
                throw;
            }
        }

        static string Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return Convert.ToBase64String(encrypted);
        }

        /******Decrypt Functions*****/
        static string Decrypt(string data, byte[] Key, byte[] IV)
        {
            byte[] cipherText = Convert.FromBase64String(data);
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

        public DataSet GetOrderReport(string UserId)
        {
           
            DataSet dsReturn = null;
            try
            {
                //var OrderReport = await Task.Run(() => entity.MyOrderReport(UserId));
               
                SqlParameter[] parameters = new SqlParameter[]
              {
                new SqlParameter("@UserId", UserId),
                 //new SqlParameter("@PageIndex", PageIndex),
                 // new SqlParameter("@PageSize", PageSize),
                 //  new SqlParameter("@RecordCount", 0)
              };
                 var CONNECTION_STRING = LiveCONNECTION_STRING;
                using (DataSet ds = SqlHelper.ExecuteDataset(CONNECTION_STRING, "MyOrderReport", parameters))
                //using (DataSet ds = SqlHelper.ExecuteDataset(CONNECTION_STRING, "Sp_MyOrderReport", parameters))
                {
                    //check if any record exist or not
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dsReturn = ds;
                    }

                }
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               
            }


            return dsReturn;
        }

         public DataSet GetOrderDetailReport(string userId,string refNo)
        {
            DataSet dsReturn = null;
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
              {
                  new SqlParameter("@refNo", refNo),
                  new SqlParameter("@UserId", userId)

              };
                var CONNECTION_STRING = LiveCONNECTION_STRING;
                using (DataSet ds = SqlHelper.ExecuteDataset(CONNECTION_STRING, "Sp_MyOrderdetail", parameters))
                {
                    //check if any record exist or not
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dsReturn = ds;
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }


            return dsReturn;
        }

        public async Task<ResponceDetail> OrderReport(User User)
        {
            var res = new ResponceDetail();
            try
            {
                DataSet ds = GetOrderReport(Convert.ToString(User.Id));
                List<OrderReport> order = new List<OrderReport>();
                var OrderReport = new OrderReport();
                if (ds.Tables[0].Rows.Count > 0)
                {

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {

                        object Created = dr["Created"];
                        if (Created == DBNull.Value || Convert.ToString(dr["Created"]) == "")
                        {
                            Created = "";
                        }
                        else
                        {
                            Created = Convert.ToString(dr["Created"]);
                        }
                        object Name = dr["Name"];
                        if (Name == DBNull.Value || Convert.ToString(dr["Name"]) == "")
                        {
                            Name = "";

                        }
                        else
                        {

                        }
                        object Amount = dr["Amount"];
                        if (Amount == DBNull.Value || Convert.ToDecimal(dr["Amount"]) == 0)
                        {
                            Amount = 0;
                        }
                        else
                        {
                            Amount = Convert.ToDecimal(dr["Amount"]);
                        }
                        object OrderID = dr["OrderID"];
                        if (OrderID == DBNull.Value || Convert.ToString(dr["OrderID"]) == "")
                        {
                            OrderID = "";
                        }
                        else
                        {
                            OrderID = Convert.ToString(dr["OrderID"]);
                        }
                        object ProductName = dr["labelProductName"];
                        if (ProductName == DBNull.Value || Convert.ToString(dr["labelProductName"]) == "")
                        {
                            ProductName = "";
                        }
                        else
                        {
                            ProductName = Convert.ToString(dr["labelProductName"]);
                        }
                        object Userid = dr["Userid"];
                        if (Userid == DBNull.Value || Convert.ToString(dr["Userid"]) == "")
                        {
                            Userid = "";
                        }
                        else
                        {
                            Userid = Convert.ToString(dr["Userid"]);
                        }
                        object Status = dr["Status"];
                        if (Status == DBNull.Value || Convert.ToString(dr["Status"]) == "")
                        {
                            Status = "";
                        }
                        else
                        {
                            Status = Convert.ToString(dr["Status"]);
                        }

                        object refno = dr["refno"];
                        if (refno == DBNull.Value || Convert.ToString(dr["refno"]) == "")
                        {
                            refno = "";
                        }
                        else
                        {
                            refno = Convert.ToString(dr["refno"]);
                        }
                        object ProductSku = dr["ProductSku"];
                        if (ProductSku == DBNull.Value || Convert.ToString(dr["ProductSku"]) == "")
                        {
                            ProductSku = 0;
                        }
                        else
                        {
                            ProductSku = Convert.ToString(dr["ProductSku"]);
                        }
                        object Validity = dr["Validity"];
                        if (Validity == DBNull.Value || Convert.ToString(dr["Validity"]) == "")
                        {
                            Validity = "";
                        }
                        else
                        {
                            Validity = Convert.ToString(dr["Validity"]);
                        }
                        object EnrptCardNo = dr["EnrptCardNo"];

                        object EnrptCardPin = dr["EnrptCardPin"];
                        if (EnrptCardPin == DBNull.Value || Convert.ToString(dr["EnrptCardPin"]) == "")
                        {
                            EnrptCardPin = "";
                        }
                        else
                        {
                            EnrptCardPin = Decrypt(Convert.ToString(dr["EnrptCardPin"]), KeyByte, IVByte);
                        }
                        if (EnrptCardNo == DBNull.Value || Convert.ToString(dr["EnrptCardNo"]) == "")
                        {
                            EnrptCardNo = "";

                        }
                        else
                        {
                            EnrptCardNo = Decrypt(Convert.ToString(dr["EnrptCardNo"]), KeyByte, IVByte);
                        }

                        OrderReport od = new OrderReport()
                        {
                            Created = Convert.ToString(Created),
                            Name = Convert.ToString(Name),
                            Amount = Convert.ToDecimal(Amount),
                            OrderID = Convert.ToString(OrderID),
                            ProductName = Convert.ToString(ProductName),
                            Userid = Convert.ToString(Userid),
                            Status = Convert.ToString(Status),
                            Validity = Convert.ToString(Validity),
                            refno = Convert.ToString(refno),
                            ProductSku = Convert.ToString(ProductSku),
                            EnrptCardNo = Convert.ToString(EnrptCardNo),
                            EnrptCardPin = Convert.ToString(EnrptCardPin),

                        };
                        order.Add(od);
                    }




                    var o = order;

                    res.OrderReport = order;
                    res.Status = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return res;
        }
        //public async Task<ResponceDetail> OrderReport(User User)
        //{
        //    var res = new ResponceDetail();
        //    try
        //    {
        //        DataSet ds = GetOrderReport(Convert.ToString(User.Id));
        //        List<OrderReport> order = new List<OrderReport>();
        //        var OrderReport = new OrderReport();
        //        if (ds.Tables[0].Rows.Count > 0)
        //        {

        //             foreach(DataRow dr in ds.Tables[0].Rows)
        //            {

        //                object Created = dr["Created"];
        //                 if(Created== DBNull.Value || Convert.ToString(dr["Created"]) == "")
        //                {
        //                    Created = "";
        //                }
        //                else
        //                {
        //                    Created = Convert.ToString(dr["Created"]);
        //                }
        //                //object Name = dr["Name"];
        //                // if(Name == DBNull.Value || Convert.ToString(dr["Name"]) == "")
        //                //{
        //                //    Name = "";

        //                //}
        //                //else
        //                //{

        //                //}
        //                //object Amount = dr["Amount"];
        //                // if(Amount == DBNull.Value || Convert.ToDecimal(dr["Amount"]) == 0)
        //                //{
        //                //    Amount = 0;
        //                //}
        //                //else
        //                //{
        //                //    Amount = Convert.ToDecimal(dr["Amount"]);
        //                //}
        //                object OrderID = dr["OrderID"];
        //                if (OrderID == DBNull.Value || Convert.ToString(dr["OrderID"]) == "")
        //                {
        //                    OrderID = "";
        //                }
        //                else
        //                {
        //                    OrderID = Convert.ToString(dr["OrderID"]);
        //                }
        //                //object ProductName = dr["labelProductName"];
        //                //if (ProductName == DBNull.Value || Convert.ToString(dr["labelProductName"]) == "")
        //                //{
        //                //    ProductName = "";
        //                //}
        //                //else
        //                //{
        //                //    ProductName = Convert.ToString(dr["labelProductName"]);
        //                //}
        //                //object Userid = dr["Userid"];
        //                //if (Userid == DBNull.Value || Convert.ToString(dr["Userid"]) == "")
        //                //{
        //                //    Userid = "";
        //                //}
        //                //else
        //                //{
        //                //    Userid = Convert.ToString(dr["Userid"]);
        //                //}
        //                object Status = dr["Status"];
        //                if (Status == DBNull.Value || Convert.ToString(dr["Status"]) == "")
        //                {
        //                    Status = "";
        //                }
        //                else
        //                {
        //                    Status = Convert.ToString(dr["Status"]);
        //                }

        //                object refno = dr["refno"];
        //                if (refno == DBNull.Value || Convert.ToString(dr["refno"]) == "")
        //                {
        //                    refno ="";
        //                }
        //                else
        //                {
        //                    refno = Convert.ToString(dr["refno"]);
        //                }
        //                //object ProductSku = dr["ProductSku"];
        //                //if (ProductSku == DBNull.Value || Convert.ToString(dr["ProductSku"]) == "")
        //                //{
        //                //    ProductSku = 0;
        //                //}
        //                //else
        //                //{
        //                //    ProductSku = Convert.ToString(dr["ProductSku"]);
        //                //}
        //                //object EnrptCardNo = dr["EnrptCardNo"];

        //                //object EnrptCardPin = dr["EnrptCardPin"];
        //                // if(EnrptCardPin == DBNull.Value || Convert.ToString(dr["EnrptCardPin"])=="")
        //                //{
        //                //    EnrptCardPin = "";
        //                //}
        //                //else
        //                //{
        //                //    EnrptCardPin = Decrypt(Convert.ToString(dr["EnrptCardPin"]), KeyByte, IVByte);
        //                //}
        //                //  if(EnrptCardNo==DBNull.Value || Convert.ToString(dr["EnrptCardNo"])=="")
        //                //{
        //                //    EnrptCardNo = "";

        //                //}
        //                //else
        //                //{
        //                //    EnrptCardNo = Decrypt(Convert.ToString(dr["EnrptCardNo"]), KeyByte, IVByte);
        //                //}

        //                OrderReport od = new OrderReport()
        //                {
        //                    Created = Convert.ToString(Created),
        //                    //Name = Convert.ToString(Name),
        //                    //Amount = Convert.ToDecimal(Amount),
        //                    OrderID = Convert.ToString(OrderID),
        //                    //ProductName = Convert.ToString(ProductName),
        //                    //Userid = Convert.ToString(Userid),
        //                    Status = Convert.ToString(Status),
        //                    //Validity = Convert.ToString(Validity),
        //                    refno = Convert.ToString(refno),
        //                    //ProductSku = Convert.ToString(ProductSku),
        //                    //EnrptCardNo = Convert.ToString(EnrptCardNo),
        //                  //EnrptCardPin = Convert.ToString(EnrptCardPin),

        //                };
        //                order.Add(od);
        //            }




        //            var o = order;

        //            res.OrderReport = order;
        //            res.Status = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return res;
        //}

        public async Task<ResponceDetail> OrderReportDetail(User user, string refno)
        {
            var res = new ResponceDetail();
            try
            {
                DataSet ds = GetOrderDetailReport(Convert.ToString(user.Id),refno);
                List<MyOrderDetail> order = new List<MyOrderDetail>();
                var OrderDetailReport = new MyOrderDetail();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        object EnrptCardNo = dr["EnrptCardNo"];

                        object EnrptCardPin = dr["EnrptCardPin"];
                        if (EnrptCardPin == DBNull.Value || Convert.ToString(dr["EnrptCardPin"]) == "")
                        {
                            EnrptCardPin = "";
                        }
                        else
                        {
                            EnrptCardPin = Decrypt(Convert.ToString(dr["EnrptCardPin"]), KeyByte, IVByte);
                        }
                        if (EnrptCardNo == DBNull.Value || Convert.ToString(dr["EnrptCardNo"]) == "")
                        {
                            EnrptCardNo = "";

                        }
                        else
                        {
                            EnrptCardNo = Decrypt(Convert.ToString(dr["EnrptCardNo"]), KeyByte, IVByte);
                        }
                        object ProductSku = dr["ProductSku"];
                        if (ProductSku == DBNull.Value || Convert.ToString(dr["ProductSku"]) == "")
                        {
                            ProductSku = 0;
                        }
                        else
                        {
                            ProductSku = Convert.ToString(dr["ProductSku"]);
                        }
                        object Validity = dr["Validity"];
                        if (Validity == DBNull.Value || Convert.ToString(dr["Validity"]) == "")
                        {
                            Validity = "";
                        }
                        else
                        {
                            Validity = Convert.ToString(dr["Validity"]);
                        }
                        object ProductName = dr["labelProductName"];
                        if (ProductName == DBNull.Value || Convert.ToString(dr["labelProductName"]) == "")
                        {
                            ProductName = "";
                        }
                        else
                        {
                            ProductName = Convert.ToString(dr["labelProductName"]);
                        }
                        object Amount = dr["Amount"];
                        if (Amount == DBNull.Value || Convert.ToDecimal(dr["Amount"]) == 0)
                        {
                            Amount = 0;
                        }
                        else
                        {
                            Amount = Convert.ToDecimal(dr["Amount"]);
                        }
                        MyOrderDetail od = new MyOrderDetail()
                        {
                            Amount = Convert.ToDecimal(Amount),
                            ProductName = Convert.ToString(ProductName),
                            Validity = Convert.ToString(Validity),
                            ProductSku = Convert.ToString(ProductSku),
                            EnrptCardNo = Convert.ToString(EnrptCardNo),
                            EnrptCardPin = Convert.ToString(EnrptCardPin),

                        };
                        order.Add(od);
                    }

                    var o = order;

                    res.OrderDetail = order;
                    res.Status = true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return res;
        }
    }
}

