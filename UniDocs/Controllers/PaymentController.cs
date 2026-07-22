using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniDocs.Data;
using UniDocs.Services;

namespace UniDocs.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        // Bảng giá
        [AllowAnonymous]
        public IActionResult Pricing()
        {
            return View();
        }

        public IActionResult CreatePayment(int planId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            // Giá tiền các gói
            long amount = 0;
            string orderInfo = "";

            if (planId == 1)
            {
                amount = 49000;
                orderInfo = $"Mua goi VIP 1 Thang cho User {userIdStr}";
            }
            else if (planId == 2)
            {
                amount = 119000;
                orderInfo = $"Mua goi VIP 3 Thang cho User {userIdStr}";
            }
            else if (planId == 3)
            {
                amount = 399000;
                orderInfo = $"Mua goi VIP 1 Nam cho User {userIdStr}";
            }
            else
            {
                return BadRequest("Gói cước không hợp lệ.");
            }

            string vnp_TmnCode = _configuration["VnPay:TmnCode"];
            string vnp_HashSecret = _configuration["VnPay:HashSecret"];
            string vnp_Url = _configuration["VnPay:BaseUrl"];
            string vnp_Version = _configuration["VnPay:Version"];
            string vnp_Returnurl = $"{Request.Scheme}://{Request.Host}/Payment/PaymentCallback";

            // Tạo request VNPay
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", vnp_Version);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString()); 
            
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            
            // Xác định được mua gói nào khi callback
            string txnRef = planId.ToString() + "_" + DateTime.Now.Ticks.ToString();
            vnpay.AddRequestData("vnp_TxnRef", txnRef);

            // Url
            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Redirect(paymentUrl);
        }

        // Kqua from VNPay
        public async Task<IActionResult> PaymentCallback()
        {
            if (Request.Query.Count > 0)
            {
                string vnp_HashSecret = _configuration["VnPay:HashSecret"];
                var vnpayData = Request.Query;
                var vnpay = new VnPayLibrary();

                foreach (var s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value.ToString());
                    }
                }

                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = Request.Query["vnp_SecureHash"];
                string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef"); 

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        // Thanh toán thành công
                        int planId = int.Parse(vnp_TxnRef.Split('_')[0]);
                        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!string.IsNullOrEmpty(userIdStr))
                        {
                            int userId = int.Parse(userIdStr);
                            var user = await _context.Users.FindAsync(userId);
                            if (user != null)
                            {
                                int addMonths = planId == 1 ? 1 : (planId == 2 ? 3 : 12);
                                
                                // Chưa có VIP hoặc đã hết hạn -> cộng từ hôm nay
                                if (!user.VipExpirationDate.HasValue || user.VipExpirationDate.Value < DateTime.Now)
                                {
                                    user.VipExpirationDate = DateTime.Now.AddMonths(addMonths);
                                }
                                else 
                                {
                                    // Đã là VIP -> cộng dồn
                                    user.VipExpirationDate = user.VipExpirationDate.Value.AddMonths(addMonths);
                                }

                                await _context.SaveChangesAsync();
                                TempData["SuccessMessage"] = "Chúc mừng bạn đã nâng cấp VIP thành công!";
                            }
                        }
                        return RedirectToAction("Profile", "Account");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Thanh toán thất bại";
                        return RedirectToAction("Pricing");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Đã có lỗi xảy ra trong quá trình xác thực dữ liệu";
                    return RedirectToAction("Pricing");
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
