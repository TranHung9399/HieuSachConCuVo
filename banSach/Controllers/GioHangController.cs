using banSach.Models;
using banSach.Helper;
using System;
using System.Linq;
using System.Web.Mvc;
using banSach.Other;
using System.Configuration;
using System.Collections.Generic;
using System.Data.Entity;

namespace banSach.Controllers
{
    public class GioHangController : BaseController
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: GioHang
        public ActionResult Index()
        {
			var maKH = Session["MaKH"]?.ToString();
			GioHang gioHang;

			if (!string.IsNullOrEmpty(maKH))
			{
				// Đã đăng nhập: lấy giỏ hàng từ DB
				gioHang = db.GioHangs.Include("ChiTietGioHangs.Sach").FirstOrDefault(g => g.MaKH == maKH);
				if (gioHang == null)
				{
					gioHang = new GioHang
					{
						MaGioHang = Guid.NewGuid().ToString(),
						MaKH = maKH,
						NgayTao = DateTime.Now,
						ChiTietGioHangs = new List<ChiTietGioHang>()
					};
				}
			}
			else
			{
				// Chưa đăng nhập: lấy giỏ hàng từ session
				var cart = Session["Cart"] as List<ChiTietGioHang>;
				if (cart != null)
				{
					foreach (var ct in cart)
					{
						ct.Sach = db.Saches.Find(ct.MaSach);
					}
				}
				var gioHangSession = new GioHang
				{
					MaGioHang = null,
					MaKH = null,
					NgayTao = DateTime.Now,
					ChiTietGioHangs = cart ?? new List<ChiTietGioHang>()
				};
				return View(gioHangSession);
			}
			ViewBag.IsLoggedIn = !string.IsNullOrEmpty(Session["MaKH"]?.ToString());
			return View(gioHang);
		}

		// GET: GioHang/ThemGiohang
		// GET: GioHang/ThemGiohang
		[HttpPost]
		public JsonResult ThemGiohangAjax(string iMasach, int qty = 1)
		{
			var maKH = Session["MaKH"]?.ToString();
			if (string.IsNullOrEmpty(maKH))
			{
				// Chưa đăng nhập: thêm vào session Cart
				var sach = db.Saches.Find(iMasach);
				if (sach == null || sach.Status != 1 || sach.SoLuongTon <= 0)
					return Json(new { success = false, message = "Sách không tồn tại hoặc đã hết hàng." });

				List<ChiTietGioHang> cart;
				if (Session["Cart"] == null)
					cart = new List<ChiTietGioHang>();
				else
					cart = (List<ChiTietGioHang>)Session["Cart"];

				var chiTiet = cart.FirstOrDefault(ct => ct.MaSach == iMasach);
				if (chiTiet == null)
				{
					cart.Add(new ChiTietGioHang
					{
						MaSach = iMasach,
						SoLuong = qty,
						DonGia = sach.GiaBan
					});
				}
				else
				{
					chiTiet.SoLuong = (chiTiet.SoLuong ?? 0) + qty;
				}
				Session["Cart"] = cart;
				var tongSoLuong = cart.Sum(ct => ct.SoLuong ?? 0);
				return Json(new { success = true, message = "Đã thêm sách vào giỏ hàng!", tongsoluong = tongSoLuong });
			}
			else
			{
				// Đã đăng nhập: thêm vào DB
				var sach = db.Saches.Find(iMasach);
				if (sach == null || sach.Status != 1 || sach.SoLuongTon <= 0)
					return Json(new { success = false, message = "Sách không tồn tại hoặc đã hết hàng." });

				var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);
				if (gioHang == null)
				{
					gioHang = new GioHang
					{
						MaGioHang = Guid.NewGuid().ToString(),
						MaKH = maKH,
						NgayTao = DateTime.Now
					};
					db.GioHangs.Add(gioHang);
					db.SaveChanges();
				}

				var chiTiet = db.ChiTietGioHangs.FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaSach == iMasach);
				if (chiTiet == null)
				{
					chiTiet = new ChiTietGioHang
					{
						MaChiTiet = Guid.NewGuid().ToString(),
						MaGioHang = gioHang.MaGioHang,
						MaSach = iMasach,
						SoLuong = qty,
						DonGia = sach.GiaBan
					};
					db.ChiTietGioHangs.Add(chiTiet);
				}
				else
				{
					chiTiet.SoLuong = (chiTiet.SoLuong ?? 0) + qty;
				}
				db.SaveChanges();

				var tongSoLuong = db.ChiTietGioHangs.Where(ct => ct.MaGioHang == gioHang.MaGioHang).Sum(ct => ct.SoLuong) ?? 0;
				return Json(new { success = true, message = "Đã thêm sách vào giỏ hàng!", tongsoluong = tongSoLuong });
			}

		}
		[HttpPost]
        public ActionResult UpdateQuantity(string maGioHang, string maSach, int soLuong)
        {
            try
            {
                var chiTiet = db.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHang == maGioHang && ct.MaSach == maSach);

                if (chiTiet == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });

                var sach = db.Saches.Find(maSach);
                if (sach == null)
                    return Json(new { success = false, message = "Không tìm thấy sách." });

                if (soLuong <= 0)
                {
                    db.ChiTietGioHangs.Remove(chiTiet);
                }
                else
                {
                    if (soLuong > sach.SoLuongTon)
                        return Json(new { success = false, message = "Số lượng vượt quá tồn kho." });

                    chiTiet.SoLuong = soLuong;
                    db.Entry<ChiTietGioHang>(chiTiet).State = System.Data.Entity.EntityState.Modified;
					//db.Entry(chiTiet).State = System.Data.Entity.EntityState.Modified;
				}

                db.SaveChanges();

                decimal newSubtotal = (soLuong <= 0) ? 0 : soLuong * (chiTiet.DonGia ?? 0);
                decimal newTotal = db.ChiTietGioHangs
                    .Where(ct => ct.MaGioHang == maGioHang)
                    .Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));

                return Json(new
                {
                    success = true,
                    subtotal = newSubtotal,
                    total = newTotal,
                    formattedSubtotal = String.Format("{0:N0}", newSubtotal),
                    formattedTotal = String.Format("{0:N0}", newTotal)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult UpdateQuantitySession(string maSach, int soLuong)
        {
	        var cart = Session["Cart"] as List<ChiTietGioHang>;
	        if (cart == null)
		        return Json(new { success = false, message = "Giỏ hàng trống." });

	        var item = cart.FirstOrDefault(ct => ct.MaSach == maSach);
	        if (item == null)
		        return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

	        if (soLuong <= 0)
	        {
		        cart.Remove(item);
	        }
	        else
	        {
		        item.SoLuong = soLuong;
	        }
	        Session["Cart"] = cart;

	        decimal newSubtotal = soLuong <= 0 ? 0 : soLuong * (item.DonGia ?? 0);
	        decimal newTotal = cart.Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));
	        return Json(new
	        {
		        success = true,
		        subtotal = newSubtotal,
		        total = newTotal,
		        formattedSubtotal = String.Format("{0:N0}", newSubtotal),
		        formattedTotal = String.Format("{0:N0}", newTotal)
	        });
        }

		// GET: GioHang/RemoveItem
		public ActionResult RemoveItem(string maGioHang, string maSach)
        {
            var chiTiet = db.ChiTietGioHangs.FirstOrDefault(ct => ct.MaGioHang == maGioHang && ct.MaSach == maSach);
            if (chiTiet != null)
            {
                db.ChiTietGioHangs.Remove(chiTiet);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private void TinhTongSoLuong()
        {
            var maKH = Session["MaKH"]?.ToString();
            if (!string.IsNullOrEmpty(maKH))
            {
                var gioHang = db.GioHangs.Include("ChiTietGioHangs")
                                         .FirstOrDefault(g => g.MaKH == maKH);

                if (gioHang != null && gioHang.ChiTietGioHangs != null)
                {
                    ViewBag.Tongsoluong = gioHang.ChiTietGioHangs.Sum(ct => ct.SoLuong ?? 0);
                }
                else
                {
                    ViewBag.Tongsoluong = 0;
                }
            }
            else
            {
                ViewBag.Tongsoluong = 0;
            }
        }
		[HttpPost]
		public JsonResult RemoveItemSession(string maSach)
		{
			var cart = Session["Cart"] as List<ChiTietGioHang>;
			if (cart != null)
			{
				var item = cart.FirstOrDefault(ct => ct.MaSach == maSach);
				if (item != null)
				{
					cart.Remove(item);
					Session["Cart"] = cart;

					decimal newTotal = cart.Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));
					return Json(new
					{
						success = true,
						formattedTotal = String.Format("{0:N0}", newTotal)
					});
				}
			}
			return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
		}

		public ActionResult Checkout()
		{
			var maKH = Session["MaKH"]?.ToString();
			var model = new CheckoutViewModel();

			if (string.IsNullOrEmpty(maKH))
			{
				// Chưa đăng nhập: lấy giỏ hàng từ session
				var cart = Session["Cart"] as List<ChiTietGioHang>;
				if (cart == null || !cart.Any())
				{
					TempData["Error"] = "Giỏ hàng của bạn đang trống.";
					return RedirectToAction("Index");
				}
				model.CartItems = cart;
				// KHÔNG tự động đổ thông tin người dùng vào form
			}
			else
			{
				// Đã đăng nhập: lấy giỏ hàng từ DB
				var gioHang = db.GioHangs.Include("ChiTietGioHangs.Sach").FirstOrDefault(g => g.MaKH == maKH);

				if (gioHang == null || !gioHang.ChiTietGioHangs.Any())
				{
					TempData["Error"] = "Giỏ hàng của bạn đang trống.";
					return RedirectToAction("Index");
				}

				// KHÔNG tự động điền thông tin KH vào model
				model.CartItems = gioHang.ChiTietGioHangs.ToList();
			}

			return View("Checkout", model);
		}

		[HttpPost]
		public ActionResult DatHang(string HoTen, string SoDienThoai, string Email, string DiaChi, string PhuongThucThanhToan)
		{
			List<ChiTietGioHang> cart;
			var maKH = Session["MaKH"]?.ToString();

			if (string.IsNullOrEmpty(maKH))
			{
				// Khách lẻ: lấy giỏ hàng từ session
				cart = Session["Cart"] as List<ChiTietGioHang>;
			}
			else
			{
				// Đã đăng nhập: lấy giỏ hàng từ database
				var gioHang = db.GioHangs.Include("ChiTietGioHangs").FirstOrDefault(g => g.MaKH == maKH);
				cart = gioHang?.ChiTietGioHangs.ToList();
			}

			if (cart == null || !cart.Any())
			{
				TempData["Error"] = "Giỏ hàng của bạn đang trống.";
				return RedirectToAction("Index");
			}

			// Tính tổng tiền
			decimal tongTien = cart.Sum(item => (item.SoLuong ?? 0) * (item.DonGia ?? 0));

			// Xử lý theo phương thức thanh toán
			if (PhuongThucThanhToan == "2") // Chuyển khoản
			{
				// Lưu thông tin đơn hàng vào session để xử lý sau khi thanh toán thành công
				var orderDetails = new Dictionary<string, object>
		{
			{ "HoTen", HoTen },
			{ "SoDienThoai", SoDienThoai },
			{ "Email", Email },
			{ "DiaChi", DiaChi },
			{ "TotalAmount", tongTien }
		};

				var cartItems = cart.Select(item => new Dictionary<string, object>
		{
			{ "MaSach", item.MaSach },
			{ "SoLuong", item.SoLuong },
			{ "DonGia", item.DonGia }
		}).ToList();

				orderDetails["CartItems"] = cartItems;
				Session["PendingOrder"] = orderDetails;

				// VNPay payment setup
				string url = ConfigurationManager.AppSettings["Url"];
				string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
				string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
				string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

				PayLib pay = new PayLib();
				pay.AddRequestData("vnp_Version", "2.1.0");
				pay.AddRequestData("vnp_Command", "pay");
				pay.AddRequestData("vnp_TmnCode", tmnCode);
				pay.AddRequestData("vnp_Amount", (tongTien * 100).ToString("F0"));
				pay.AddRequestData("vnp_BankCode", "");
				pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
				pay.AddRequestData("vnp_CurrCode", "VND");
				pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
				pay.AddRequestData("vnp_Locale", "vn");
				pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang cho KH {HoTen}");
				pay.AddRequestData("vnp_OrderType", "other");
				pay.AddRequestData("vnp_ReturnUrl", returnUrl);
				pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

				string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
				return Redirect(paymentUrl);
			}
			else // Thanh toán tiền mặt
			{
				// Tạo đơn hàng mới
				var donHang = new DonDatHang
				{
					MaDonHang = Guid.NewGuid().ToString(),
					HoTen = HoTen,
					SoDienThoai = SoDienThoai,
					Email = Email,
					DiaChi = DiaChi,
					NgayDat = DateTime.Now,
					TrangThai = "Chờ xác nhận"
				};
				db.DonDatHangs.Add(donHang);

				decimal tongTienHang = 0;
				string emailBody = $"<div style='color: black;'>" +
					$"<p>Chào {donHang.HoTen},</p>" +
					$"<p>Bạn đã đặt hàng thành công vào lúc {donHang.NgayDat:HH:mm:ss dd/MM/yyyy}.</p>" +
					$"<p>Mã đơn hàng của bạn là: <strong>{donHang.MaDonHang}</strong></p>" +
					"<p>Chi tiết đơn hàng:</p>" +
					"<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%;'>" +
					"<thead><tr>" +
					"<th style='text-align:left;'>Tên sách</th>" +
					"<th>Số lượng</th>" +
					"<th>Đơn giá</th>" +
					"<th>Thành tiền</th>" +
					"</tr></thead><tbody>";

				foreach (var item in cart)
				{
					var sach = db.Saches.Find(item.MaSach);
					var thanhTien = (item.SoLuong ?? 0) * (item.DonGia ?? 0);
					tongTienHang += thanhTien;

					emailBody += $"<tr>" +
								 $"<td>{(sach != null ? sach.TenSach : item.MaSach)}</td>" +
								 $"<td style='text-align:center;'>{item.SoLuong}</td>" +
								 $"<td style='text-align:right;'>{item.DonGia:N0}₫</td>" +
								 $"<td style='text-align:right;'>{thanhTien:N0}₫</td>" +
								 "</tr>";

					// Trừ tồn kho nếu có
					if (sach != null)
						sach.SoLuongTon -= item.SoLuong ?? 0;

					db.ChiTietDonHangs.Add(new ChiTietDonHang
					{
						MaDonHang = donHang.MaDonHang,
						MaSach = item.MaSach,
						SoLuong = item.SoLuong,
						DonGia = item.DonGia
					});
				}

				emailBody += $"<tr>" +
							 $"<td colspan='3' style='text-align:right; font-weight:bold;'>Tổng cộng:</td>" +
							 $"<td style='text-align:right; font-weight:bold;'>{tongTienHang:N0}₫</td>" +
							 $"</tr></tbody></table>" +
							 "<p><strong>Cảm ơn quý khách đã mua hàng tại cửa hàng Sách Trực Tuyến!</strong></p>" +
							 "<p>Hotline hỗ trợ: <strong>+060 (800) 801-582</strong></p>" +
							 "<p><em>Trân trọng,</em><br/>HT STORE</p>" +
							 "</div>";

				db.SaveChanges();

				// Gửi email xác nhận
				SendMail sendMail = new SendMail();
				sendMail.SendMailFunction(Email, "Xác nhận đơn hàng từ Cửa hàng sách BOOKSTORE", emailBody);

				// Xóa giỏ hàng
				if (string.IsNullOrEmpty(maKH))
					Session["Cart"] = null;
				else
				{
					var gioHang = db.GioHangs.Include("ChiTietGioHangs")
											 .FirstOrDefault(g => g.MaKH == maKH);
					if (gioHang != null)
					{
						db.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
						db.GioHangs.Remove(gioHang);
						db.SaveChanges();
					}
				}

				TempData["Success"] = "Đặt hàng thành công!";
				return RedirectToAction("Index", "Home");
			}
		}

		public ActionResult PaymentConfirm()
		{
			if (Request.QueryString.Count > 0)
			{
				string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
				var vnpayData = Request.QueryString;
				PayLib pay = new PayLib();

				foreach (string s in vnpayData)
				{
					if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
					{
						pay.AddResponseData(s, vnpayData[s]);
					}
				}

				long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef"));
				long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo"));
				string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
				string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

				bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret);

				if (checkSignature && vnp_ResponseCode == "00")
				{
					// Payment successful, process the order
					var orderDetails = Session["PendingOrder"] as Dictionary<string, object>;
					if (orderDetails == null)
					{
						ViewBag.Message = "Không tìm thấy thông tin đơn hàng.";
						return View();
					}

					// Extract customer details
					string hoTen = orderDetails["HoTen"]?.ToString();
					string soDienThoai = orderDetails["SoDienThoai"]?.ToString();
					string email = orderDetails["Email"]?.ToString();
					string diaChi = orderDetails["DiaChi"]?.ToString();
					var cartItems = orderDetails["CartItems"] as List<Dictionary<string, object>>;

					if (string.IsNullOrEmpty(hoTen) || cartItems == null || !cartItems.Any())
					{
						ViewBag.Message = "Thông tin đơn hàng không hợp lệ.";
						return View();
					}

					// Create order
					var donHang = new DonDatHang
					{
						MaDonHang = Guid.NewGuid().ToString(),
						HoTen = hoTen,
						SoDienThoai = soDienThoai,
						Email = email,
						DiaChi = diaChi,
						NgayDat = DateTime.Now,
						TrangThai = "Chờ xác nhận"
					};
					db.DonDatHangs.Add(donHang);

					decimal tongTien = 0;
					string emailBody = $"<div style='color: black;'>" +
						$"<p>Chào {donHang.HoTen},</p>" +
						$"<p>Bạn đã đặt hàng thành công vào lúc {donHang.NgayDat:HH:mm:ss dd/MM/yyyy}.</p>" +
						$"<p>Mã đơn hàng của bạn là: <strong>{donHang.MaDonHang}</strong></p>" +
						"<p>Chi tiết đơn hàng:</p>" +
						"<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%;'>" +
						"<thead><tr>" +
						"<th style='text-align:left;'>Tên sách</th>" +
						"<th>Số lượng</th>" +
						"<th>Đơn giá</th>" +
						"<th>Thành tiền</th>" +
						"</tr></thead><tbody>";

					foreach (var item in cartItems)
					{
						string maSach = item["MaSach"]?.ToString();
						int soLuong = Convert.ToInt32(item["SoLuong"]);
						decimal donGia = Convert.ToDecimal(item["DonGia"]);

						var sach = db.Saches.Find(maSach);
						var tenSach = sach != null ? sach.TenSach : maSach;
						var thanhTien = soLuong * donGia;
						tongTien += thanhTien;

						emailBody += $"<tr>" +
									 $"<td>{tenSach}</td>" +
									 $"<td style='text-align:center;'>{soLuong}</td>" +
									 $"<td style='text-align:right;'>{donGia:N0}₫</td>" +
									 $"<td style='text-align:right;'>{thanhTien:N0}₫</td>" +
									 "</tr>";

						// Trừ tồn kho nếu có
						if (sach != null)
							sach.SoLuongTon -= soLuong;

						db.ChiTietDonHangs.Add(new ChiTietDonHang
						{
							MaDonHang = donHang.MaDonHang,
							MaSach = maSach,
							SoLuong = soLuong,
							DonGia = donGia
						});
					}

					emailBody += $"<tr>" +
								 $"<td colspan='3' style='text-align:right; font-weight:bold;'>Tổng cộng:</td>" +
								 $"<td style='text-align:right; font-weight:bold;'>{tongTien:N0}₫</td>" +
								 $"</tr></tbody></table>" +
								 "<p><strong>Cảm ơn quý khách đã mua hàng tại cửa hàng Sách Trực Tuyến!</strong></p>" +
								 "<p>Hotline hỗ trợ: <strong>+060 (800) 801-582</strong></p>" +
								 "<p><em>Trân trọng,</em><br/>HT STORE</p>" +
								 "</div>";

					db.SaveChanges();

					// Gửi email xác nhận
					SendMail sendMail = new SendMail();
					sendMail.SendMailFunction(email, "Xác nhận đơn hàng từ Cửa hàng sách BOOKSTORE", emailBody);

					// Xóa giỏ hàng/session
					Session["PendingOrder"] = null;
					Session["Cart"] = null;

					// Nếu đăng nhập thì xóa giỏ hàng DB
					var maKH = Session["MaKH"]?.ToString();
					if (!string.IsNullOrEmpty(maKH))
					{
						var gioHang = db.GioHangs.Include("ChiTietGioHangs")
												 .FirstOrDefault(g => g.MaKH == maKH);
						if (gioHang != null)
						{
							db.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
							db.GioHangs.Remove(gioHang);
							db.SaveChanges();
						}
					}

					ViewBag.Message = $"Thanh toán thành công hóa đơn {orderId} | Mã giao dịch: {vnpayTranId}";
					TempData["Success"] = "Đặt hàng thành công!";
				}
				else
				{
					// Payment failed
					ViewBag.Message = checkSignature
						? $"Có lỗi xảy ra trong quá trình xử lý hóa đơn {orderId} | Mã giao dịch: {vnpayTranId} | Mã lỗi: {vnp_ResponseCode}"
						: "Có lỗi xảy ra trong quá trình xử lý";
					TempData["Error"] = ViewBag.Message;
				}
			}
			else
			{
				ViewBag.Message = "Không có thông tin thanh toán.";
				TempData["Error"] = ViewBag.Message;
			}

			return View();
		}

		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}