using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PayPal.Api;
using PMDWebApplication.DB;
using PMDWebApplication.Repository;
using PMDWebApplication.Models.Home;
using System.Windows.Forms;

namespace PMDWebApplication.Controllers
{

    public class PaymentWithPaypalController : Controller
    {
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        dbVervoerEntities ctx = new dbVervoerEntities();
        // GET: Payment
        public ActionResult PaymentWithPaypal(string Cancel = null) {  
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguaration.GetAPIContext();  
            try {  
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];  
                if (string.IsNullOrEmpty(payerId)) {  
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/PaymentWithPaypal/PaymentWithPaypal?";  
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));  
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);  
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();  
                    string paypalRedirectUrl = null;  
                    while (links.MoveNext()) {  
                        Links lnk = links.Current;  
                        if (lnk.rel.ToLower().Trim().Equals("approval_url")) {  
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;  
                        }  
                    }  
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);  
                    return Redirect(paypalRedirectUrl);  
                } else {  
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];  
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);  
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved") {  
                        return View("Success");  
                    }  
                }  
            } catch (Exception) {  
                return View("Success");  
            }
            //on successful payment, show success page to user.  
            return View("Success");  
        }  
            private PayPal.Api.Payment payment;  
            private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId) {  
                var paymentExecution = new PaymentExecution() {  
                    payer_id = payerId  
                };  
                this.payment = new Payment() {  
                    id = paymentId  
                };  
                return this.payment.Execute(apiContext, paymentExecution);  
            }  
            private Payment CreatePayment(APIContext apiContext, string redirectUrl) {

            var userId = User.Identity.GetUserId();
            List<AspNetCart> cart = _unitOfWork.GetRepositoryInstance<AspNetCart>().GetAllRecordsIQueryable().Where(x => x.MemberId.Equals(userId)).ToList();
            List<AspNetCartInsurance> cartInsurance = _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().GetAllRecordsIQueryable().Where(x => x.MemberId.Equals(userId)).ToList();

            //create itemlist and add item objects to it  
                var itemList = new ItemList() {  
                    items = new List <PayPal.Api.Item > ()  
                };

            foreach (var product in cart)
            {
                itemList.items.Add(new PayPal.Api.Item()
                {
                    name = ctx.AspNetProducts.Find(product.ProductId).ProductName,
                    currency = "USD",
                    price = ((int)ctx.AspNetProducts.Find(product.ProductId).Price).ToString(),
                    quantity = product.Quantity.ToString(),
                    sku = "sku"
                });
            }

            foreach (var insurance in cartInsurance)
            {
                itemList.items.Add(new PayPal.Api.Item()
                {
                    name = ctx.AspNetInsurances.Find(insurance.InsuranceId).InsuranceName,
                    currency = "USD",
                    price = ((int)ctx.AspNetInsurances.Find(insurance.InsuranceId).Price).ToString(),
                    quantity = "1",
                    sku = "sku"
                });
            }

                var payer = new Payer() {  
                    payment_method = "paypal"  
                };  
                // Configure Redirect Urls here with RedirectUrls object  
                var redirUrls = new RedirectUrls() {  
                    cancel_url = redirectUrl + "&Cancel=true",  
                        return_url = redirectUrl  
                };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details() {
                tax = "1",
                shipping = "1",
                subtotal = itemList.items.Sum(x => Convert.ToInt32(x.quantity) * Convert.ToInt32(x.price)).ToString()
                };  
                //Final amount with details  
                var amount = new Amount() {  
                   currency = "USD",  
                      total = (Convert.ToInt32(details.tax) + Convert.ToInt32(details.shipping) + Convert.ToInt32(details.subtotal)).ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                    details = details  
                };  
                var transactionList = new List < Transaction > ();  
                // Adding description about the transaction  
                transactionList.Add(new Transaction() {  
                    description = "Transaction description",  
                        invoice_number = "your generated invoice number", //Generate an Invoice No  
                        amount = amount,  
                        item_list = itemList  
                });  
                this.payment = new Payment() {  
                    intent = "sale",  
                        payer = payer,  
                        transactions = transactionList,  
                        redirect_urls = redirUrls  
                };  
                // Create a payment using a APIContext  
                return this.payment.Create(apiContext);  
            }  
    }
}