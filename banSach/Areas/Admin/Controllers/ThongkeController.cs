﻿using System;
using System.Linq;
using System.Web.Mvc;
using banSach.Models;
using System.Collections.Generic;
using System.Globalization;
using ClosedXML.Excel;
using System.IO;
using Rotativa;

namespace banSach.Areas.Admin.Controllers
{
    public class ThongkeController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Admin/Thongke
        public ActionResult Index(string loaiThongKe = "thang", DateTime? ngayBatDau = null, DateTime? ngayKetThuc = null)
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
            // Mặc định: 30 ngày trước
            var ketThuc = ngayKetThuc ?? DateTime.Now;
            var batDau = ngayBatDau ?? ketThuc.AddDays(-30);

            // Kiểm tra loại thống kê
            if (loaiThongKe != "ngay" && loaiThongKe != "thang" && loaiThongKe != "nam")
            {
                loaiThongKe = "thang";
            }

            // Lấy danh sách đơn hàng "Hoàn tất" hoặc "Đã thanh toán"
            var donHangs = db.DonDatHangs
                .Include("ChiTietDonHangs") // Sửa từ lambda sang string
                .Where(d =>
                    (d.TrangThai == "Hoàn tất" || d.TrangThai == "Đã thanh toán")
                    && d.NgayDat >= batDau && d.NgayDat <= ketThuc)
                .ToList();

            // Tạo model thống kê
            var model = new ThongKe
            {
                NgayBatDau = batDau,
                NgayKetThuc = ketThuc,
                LoaiThongKe = loaiThongKe,
                TongSoDonHang = donHangs.Count,
                TongDoanhThu = donHangs.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0))),
                ChiTiet = new List<ChiTietThongKe>()
            };

            // Nhóm theo ngày, tháng hoặc năm
            if (loaiThongKe == "ngay")
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => o.NgayDat.Value.Date)
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = g.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => DateTime.ParseExact(r.KhoangThoiGian, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
            }
            else if (loaiThongKe == "nam")
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => o.NgayDat.Value.Year)
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = g.Key.ToString(),
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => int.Parse(r.KhoangThoiGian))
                    .ToList();
            }
            else
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => new { o.NgayDat.Value.Year, o.NgayDat.Value.Month })
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = $"{g.Key.Month}/{g.Key.Year}",
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => DateTime.ParseExact(r.KhoangThoiGian, "M/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
            }

            return View(model);
        }

        public ActionResult ExportExcel(string loaiThongKe, DateTime? ngayBatDau, DateTime? ngayKetThuc)
        {
            // Lấy lại dữ liệu như action Index
            var ketThuc = ngayKetThuc ?? DateTime.Now;
            var batDau = ngayBatDau ?? ketThuc.AddDays(-30);
            if (loaiThongKe != "ngay" && loaiThongKe != "thang" && loaiThongKe != "nam")
            {
                loaiThongKe = "thang";
            }
            var donHangs = db.DonDatHangs
                .Include("ChiTietDonHangs")
                .Where(d => (d.TrangThai == "Hoàn tất" || d.TrangThai == "Đã thanh toán")
                    && d.NgayDat >= batDau && d.NgayDat <= ketThuc)
                .ToList();
            var model = new ThongKe
            {
                NgayBatDau = batDau,
                NgayKetThuc = ketThuc,
                LoaiThongKe = loaiThongKe,
                TongSoDonHang = donHangs.Count,
                TongDoanhThu = donHangs.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0))),
                ChiTiet = new List<ChiTietThongKe>()
            };
            if (loaiThongKe == "ngay")
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => o.NgayDat.Value.Date)
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = g.Key.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => System.DateTime.ParseExact(r.KhoangThoiGian, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();
            }
            else if (loaiThongKe == "nam")
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => o.NgayDat.Value.Year)
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = g.Key.ToString(),
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => int.Parse(r.KhoangThoiGian))
                    .ToList();
            }
            else
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => new { o.NgayDat.Value.Year, o.NgayDat.Value.Month })
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = $"{g.Key.Month}/{g.Key.Year}",
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => System.DateTime.ParseExact(r.KhoangThoiGian, "M/yyyy", System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();
            }
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ThongKe");
                worksheet.Cell(1, 1).Value = "Khoảng Thời Gian";
                worksheet.Cell(1, 2).Value = "Số Đơn Hàng";
                worksheet.Cell(1, 3).Value = "Doanh Thu";
                int row = 2;
                foreach (var item in model.ChiTiet)
                {
                    worksheet.Cell(row, 1).Value = item.KhoangThoiGian;
                    worksheet.Cell(row, 2).Value = item.SoDonHang;
                    worksheet.Cell(row, 3).Value = item.DoanhThu;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ThongKeDoanhThu.xlsx");
                }
            }
        }

        public ActionResult ExportPdf(string loaiThongKe, DateTime? ngayBatDau, DateTime? ngayKetThuc)
        {
            var ketThuc = ngayKetThuc ?? DateTime.Now;
            var batDau = ngayBatDau ?? ketThuc.AddDays(-30);
            if (loaiThongKe != "ngay" && loaiThongKe != "thang" && loaiThongKe != "nam")
            {
                loaiThongKe = "thang";
            }
            var donHangs = db.DonDatHangs
                .Include("ChiTietDonHangs")
                .Where(d => (d.TrangThai == "Hoàn tất" || d.TrangThai == "Đã thanh toán")
                    && d.NgayDat >= batDau && d.NgayDat <= ketThuc)
                .ToList();
            var model = new ThongKe
            {
                NgayBatDau = batDau,
                NgayKetThuc = ketThuc,
                LoaiThongKe = loaiThongKe,
                TongSoDonHang = donHangs.Count,
                TongDoanhThu = donHangs.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0))),
                ChiTiet = new List<ChiTietThongKe>()
            };
            if (loaiThongKe == "ngay")
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => o.NgayDat.Value.Date)
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = g.Key.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => System.DateTime.ParseExact(r.KhoangThoiGian, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();
            }
            else if (loaiThongKe == "nam")
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => o.NgayDat.Value.Year)
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = g.Key.ToString(),
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => int.Parse(r.KhoangThoiGian))
                    .ToList();
            }
            else
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => new { o.NgayDat.Value.Year, o.NgayDat.Value.Month })
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = $"{g.Key.Month}/{g.Key.Year}",
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => System.DateTime.ParseExact(r.KhoangThoiGian, "M/yyyy", System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();
            }
            return new Rotativa.ViewAsPdf("ExportPdf", model)
            {
                FileName = "ThongKeDoanhThu.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Landscape
            };
        }
    }
}
