using banSach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace banSach.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();
        // GET: Admin/HomeAdmin
        public ActionResult Index()
        {
            try
            {
                if (Session["AdminUser"] == null)
                {
                    return RedirectToAction("Index", "Login", new { area = "Admin" });
                }

                var user = Session["AdminUser"] as NhanVien;
                if (user == null)
                {
                    return RedirectToAction("Index", "Login", new { area = "Admin" });
                }

                ViewBag.HoTen = user.HoTen;
                ViewBag.TongSach = db.Saches.Count();
                ViewBag.TongKhachHang = db.KhachHangs.Count();
                // Lấy 10 sách bán chạy nhất
                var bestSellerIds = db.ChiTietDonHangs
                    .Where(ct => ct.Sach.Status == 1)
                    .GroupBy(ct => ct.MaSach)
                    .Select(g => new {
                        MaSach = g.Key,
                        TotalSold = g.Sum(x => x.SoLuong)
                    })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(10)
                    .ToList()
                    .Select(x => x.MaSach)
                    .ToList();
                var bestSellerBooks = db.Saches
                    .Where(s => bestSellerIds.Contains(s.MaSach))
                    .ToList();
                ViewBag.BestSellerBooks = bestSellerIds
                    .Select(bookId => bestSellerBooks.FirstOrDefault(s => s.MaSach == bookId))
                    .Where(s => s != null)
                    .ToList();

                // Tạo đối tượng ViewModel để lưu trữ thống kê
                var thongKe = new ThongKeDonHang
                {
                    ChoXacNhan = db.DonDatHangs.Count(d => d.TrangThai == "Chờ xác nhận"),
                    DaHuy = db.DonDatHangs.Count(d => d.TrangThai == "Đã hủy"),
                    DaThanhToanVaHoanTat = db.DonDatHangs.Count(d => d.TrangThai == "Đã thanh toán" || d.TrangThai == "Hoàn tất")
                };

                System.Diagnostics.Debug.WriteLine($"ThongKe: ChoXacNhan={thongKe.ChoXacNhan}, DaHuy={thongKe.DaHuy}, DaThanhToanVaHoanTat={thongKe.DaThanhToanVaHoanTat}");

                // Truyền ViewModel sang view
                return View(thongKe);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để kiểm tra
                System.Diagnostics.Debug.WriteLine($"Error in HomeAdminController: {ex.Message}\nStackTrace: {ex.StackTrace}");
                // Trả về ViewModel mặc định nếu có lỗi
                var thongKeMacDinh = new ThongKeDonHang
                {
                    ChoXacNhan = 0,
                    DaHuy = 0,
                    DaThanhToanVaHoanTat = 0
                };
                return View(thongKeMacDinh);
            }
        }
    }
}