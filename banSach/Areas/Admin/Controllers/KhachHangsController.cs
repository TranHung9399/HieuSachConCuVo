using System.Linq;
using System.Net;
using System.Web.Mvc;
using banSach.Models;
using PagedList;

namespace banSach.Areas.Admin.Controllers
{
    public class KhachHangsController : Controller
    {
        private banSach.Models.QLBanSachEntities db = new banSach.Models.QLBanSachEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["ChucVu"] == null || Session["ChucVu"].ToString() != "Quản Lý")
            {
                filterContext.Result = new RedirectResult(Url.Action("Index", "Login", new { area = "Admin" }));
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Admin/KhachHangs
        public ActionResult Index(string search, int? page)
        {
            var khachHangs = db.KhachHangs.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                khachHangs = khachHangs.Where(k => k.HoTen.Contains(search) || k.Email.Contains(search) || k.DiaChi.Contains(search) || k.SoDienThoai.Contains(search));
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.CurrentFilter = search;
            return View(khachHangs.OrderByDescending(k => k.MaKH).ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/KhachHangs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/KhachHangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TenKH,Email,DiaChi,SDT")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.KhachHangs.Add(khachHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var khachHang = db.KhachHangs.Find(id);
            if (khachHang == null) return HttpNotFound();
            return View(khachHang);
        }

        // POST: Admin/KhachHangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaKH,TenKH,Email,DiaChi,SDT")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khachHang).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Delete/5
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var khachHang = db.KhachHangs.Find(id);
            if (khachHang == null) return HttpNotFound();
            return View(khachHang);
        }

        // POST: Admin/KhachHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            db.KhachHangs.Remove(khachHang);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/KhachHangs/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var khachHang = db.KhachHangs.Find(id);
            if (khachHang == null) return HttpNotFound();
            return View(khachHang);
        }
    }
}
