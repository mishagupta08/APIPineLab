using Newtonsoft.Json;
using PineAppAPI.Models;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace PineAppAPI.Repositories
{
    public class BasicmlmRepository
    {
        string accessToken = ConfigurationManager.AppSettings["WalletToken"];
        string BasicMLMURL = "http://basicmlm.bisplindia.in/";
        static HttpClient client = new HttpClient();

        public async Task<ResponceDetail> ValidateTransactionLogin(User objUser)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                string Url = BasicMLMURL+ "CheckLogin?token="+ accessToken + "&UserName="+ objUser.Username + "&Password=" + objUser.Password;
                HttpResponseMessage response = await client.GetAsync(Url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var WalletResponse = JsonConvert.DeserializeObject<WalletResponse>(responseContent);
                    responseDetail.Status = true;
                    responseDetail.WalletResponse = WalletResponse;
                }
            }
            catch (Exception ex)
            {
                responseDetail.Message = ex.Message;
            }
            return responseDetail;
        }

        public async Task<ResponceDetail> GetWalletBalance(User objUser)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                string Url = BasicMLMURL + "CheckLogin?token=" + accessToken + "&UserName=" + objUser.Username + "&Password=" + objUser.Password + "&Wallertype=" + objUser.WalletType + "&action=GetBalance";
                HttpResponseMessage response = await client.GetAsync(Url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var WalletResponse = JsonConvert.DeserializeObject<WalletResponse>(responseContent);
                    responseDetail.Status = true;
                    responseDetail.WalletResponse = WalletResponse;
                }
            }
            catch (Exception ex)
            {
                responseDetail.Message = ex.Message;
            }
            return responseDetail;
        }

        public async Task<ResponceDetail> DeductWalletBalance(User objUser)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                string Url = BasicMLMURL + "CheckLogin?token=" + accessToken + "&UserName=" + objUser.Username + "&Password=" + objUser.Password + "&TxnData="+objUser.TxnData + "&Wallertype=" + objUser.WalletType + "&action=DeductWalletAmount";
                HttpResponseMessage response = await client.GetAsync(Url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var WalletResponse = JsonConvert.DeserializeObject<WalletResponse>(responseContent);
                    responseDetail.Status = true;
                    responseDetail.WalletResponse = WalletResponse;
                }
            }
            catch (Exception ex)
            {
                responseDetail.Message = ex.Message;
            }
            return responseDetail;
        }

        public async Task<ResponceDetail> Confirmvoucher(User objUser)
        {
            ResponceDetail responseDetail = new ResponceDetail();
            try
            {
                string Url = BasicMLMURL + "CheckLogin?token=" + accessToken + "&UserName=" + objUser.Username + "&Password=" + objUser.Password + "&Voucherno=" + objUser.TxnData + "&Wallertype=" + objUser.WalletType + "&action=Confirmvoucher";
                HttpResponseMessage response = await client.GetAsync(Url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var WalletResponse = JsonConvert.DeserializeObject<WalletResponse>(responseContent);
                    responseDetail.Status = true;
                    responseDetail.WalletResponse = WalletResponse;
                }
            }
            catch (Exception ex)
            {
                responseDetail.Message = ex.Message;
            }
            return responseDetail;
        }
    }
}