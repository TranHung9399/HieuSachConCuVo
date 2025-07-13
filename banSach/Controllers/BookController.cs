using banSach.Models;
using banSach.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class BookController : BaseController
    {
        private QLBanSachEntities db = new QLBanSachEntities();
        // GET: Book
        public ActionResult Index(String id, string type, string sort)
        {
            ViewBag.SidebarCategories = db.Loais.Where(l => l.Status == 1).ToList();

            // Nếu có type là 'new' hoặc 'bestseller' thì hiển thị tương ứng
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "new")
                {
                    ViewBag.Category = new Loai { TenLoai = "Sách mới nhất" };
                    var books = db.Saches.Where(s => s.Status == 1).OrderByDescending(s => s.NgayNhapHang).ToList();
                    return View(books);
                }
                if (type == "bestseller")
                {
                    ViewBag.Category = new Loai { TenLoai = "Sách bán chạy" };
                    var bestSellerIds = db.ChiTietDonHangs
                        .Where(ct => ct.Sach.Status == 1)
                        .GroupBy(ct => ct.MaSach)
                        .Select(g => new {
                            MaSach = g.Key,
                            TotalSold = g.Sum(x => x.SoLuong)
                        })
                        .OrderByDescending(x => x.TotalSold)
                        .ToList()
                        .Select(x => x.MaSach)
                        .ToList();
                    var bestSellerBooks = db.Saches
                        .Where(s => bestSellerIds.Contains(s.MaSach))
                        .ToList();
                    var books = bestSellerIds
                        .Select(bookId => bestSellerBooks.FirstOrDefault(s => s.MaSach == bookId))
                        .Where(s => s != null)
                        .ToList();
                    return View(books);
                }
            }

            // Nếu không có id và không có type => trả về tất cả sách
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Category = new Loai { TenLoai = "Tất cả sách" };
                var books = db.Saches.Where(s => s.Status == 1);

                switch (sort)
                {
                    case "moinhat":
                        books = books.OrderByDescending(s => s.NgayNhapHang);
                        break;
                    case "cunhat":
                        books = books.OrderBy(s => s.NgayNhapHang);
                        break;
                    case "giatangdan":
                        books = books.OrderBy(s => s.GiaBan);
                        break;
                    case "giagiamdan":
                        books = books.OrderByDescending(s => s.GiaBan);
                        break;
                    case "tenaz":
                        books = books.OrderBy(s => s.TenSach);
                        break;
                    case "tenza":
                        books = books.OrderByDescending(s => s.TenSach);
                        break;
                    case "banchay":
                        // Sắp xếp bán chạy nhất
                        var bestSellerIds = db.ChiTietDonHangs
                            .Where(ct => ct.Sach.Status == 1)
                            .GroupBy(ct => ct.MaSach)
                            .Select(g => new {
                                MaSach = g.Key,
                                TotalSold = g.Sum(x => x.SoLuong)
                            })
                            .OrderByDescending(x => x.TotalSold)
                            .Select(x => x.MaSach)
                            .ToList();
                        var bestSellerBooks = books.Where(s => bestSellerIds.Contains(s.MaSach)).ToList();
                        books = bestSellerIds
                            .Select(bookId => bestSellerBooks.FirstOrDefault(s => s.MaSach == bookId))
                            .Where(s => s != null)
                            .AsQueryable();
                        break;
                    default:
                        books = books.OrderByDescending(s => s.NgayNhapHang);
                        break;
                }

                return View(books.ToList());
            }

            // Logic cũ: hiển thị theo thể loại
            var category = db.Loais.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            var booksByCategory = db.Saches.Where(s => s.MaLoai == id && s.Status == 1).ToList();

            switch (sort)
            {
                case "moinhat":
                    booksByCategory = booksByCategory.OrderByDescending(s => s.NgayNhapHang).ToList();
                    break;
                case "cunhat":
                    booksByCategory = booksByCategory.OrderBy(s => s.NgayNhapHang).ToList();
                    break;
                case "giatangdan":
                    booksByCategory = booksByCategory.OrderBy(s => s.GiaBan).ToList();
                    break;
                case "giagiamdan":
                    booksByCategory = booksByCategory.OrderByDescending(s => s.GiaBan).ToList();
                    break;
                case "tenaz":
                    booksByCategory = booksByCategory.OrderBy(s => s.TenSach).ToList();
                    break;
                case "tenza":
                    booksByCategory = booksByCategory.OrderByDescending(s => s.TenSach).ToList();
                    break;
                case "banchay":
                    var bestSellerIds = db.ChiTietDonHangs
                        .Where(ct => ct.Sach.Status == 1)
                        .GroupBy(ct => ct.MaSach)
                        .Select(g => new {
                            MaSach = g.Key,
                            TotalSold = g.Sum(x => x.SoLuong)
                        })
                        .OrderByDescending(x => x.TotalSold)
                        .Select(x => x.MaSach)
                        .ToList();
                    var bestSellerBooks = booksByCategory.Where(s => bestSellerIds.Contains(s.MaSach)).ToList();
                    booksByCategory = bestSellerIds
                        .Select(bookId => bestSellerBooks.FirstOrDefault(s => s.MaSach == bookId))
                        .Where(s => s != null)
                        .ToList();
                    break;
                default:
                    booksByCategory = booksByCategory.OrderByDescending(s => s.NgayNhapHang).ToList();
                    break;
            }

            ViewBag.Category = category;
            return View(booksByCategory.ToList());
        }

        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var book = db.Saches.Find(id);
            if (book == null || book.Status != 1)
            {
                return HttpNotFound();
            }

            // Fetch authors and their roles via VietSach
            var authors = (from vs in db.VietSaches
                           join tg in db.TacGias on vs.MaTG equals tg.MaTG
                           where vs.MaSach == id && tg.Status == 1
                           select new SachChonViewModel
                           {
                               MaSach = vs.MaSach,
                               TenSach = vs.Sach.TenSach,
                               VaiTro = vs.VaiTro,
                               TenTG = tg.TenTG,
                               TieuSu=tg.TieuSu,
                           }).ToList();

            // Log the results for debugging
            System.Diagnostics.Debug.WriteLine($"Book MaSach: {id}, Author Count: {authors.Count}");
            foreach (var author in authors)
            {
                System.Diagnostics.Debug.WriteLine($"Author: {author.TenTG}, Role: {author.VaiTro}");
            }

            ViewBag.Authors = authors;

            // Giả sử Model là 1 quyển sách
            var relatedBooks = db.Saches
                .Where(s => s.MaSach != book.MaSach)
                .OrderByDescending(s => s.NgayNhapHang)
                .Take(10)
                .ToList();
            ViewBag.RelatedBooks = relatedBooks;

            return View(book);
        }
    }
}