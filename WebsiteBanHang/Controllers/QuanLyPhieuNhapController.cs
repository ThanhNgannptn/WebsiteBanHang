using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Controllers
{
    [Authorize(Roles = "QuanLy,QuanTriWeb")]
    public class QuanLyPhieuNhapController : Controller
    {
        QuanLyBanHangEntities db = new QuanLyBanHangEntities();

        // GET: QuanLyPhieuNhap
        [HttpGet]
        public ActionResult NhapHang()
        {
            ViewBag.MaNCC = db.NhaCungCaps;
            ViewBag.ListSanPham = db.SanPhams;
            ViewBag.NgayNhap = DateTime.Today;

            return View();
        }
        public ActionResult DSPN(DateTime? fromDate, DateTime? toDate, string productName)
        {
            // Default query to get all the data
            var query = db.PhieuNhaps.AsQueryable();

            // Apply filters if they are provided
            if (fromDate.HasValue)
            {
                query = query.Where(pn => pn.NgayNhap >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(pn => pn.NgayNhap <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(pn => pn.ChiTietPhieuNhaps.Any(ct => ct.SanPham.TenSP.Contains(productName)));
            }

            // Select the necessary fields and order by NgayNhap descending
            var chiTietPhieuNhaps = query
                .Select(pn => new
                {
                    MaPN = pn.MaPN,
                    NgayNhap = pn.NgayNhap,
                    TenSP = pn.ChiTietPhieuNhaps.Select(ct => ct.SanPham.TenSP).FirstOrDefault(),
                    SoLuong = pn.ChiTietPhieuNhaps.Select(ct => ct.SoLuongNhap).FirstOrDefault(),
                    DonGiaNhap = pn.ChiTietPhieuNhaps.Select(ct => ct.DonGiaNhap).FirstOrDefault()
                })
                .OrderByDescending(pn => pn.NgayNhap)  // Order by the most recent date
                .ToList();

            // Map the anonymous type to a ViewModel
            var viewModel = chiTietPhieuNhaps.Select(item => new ChiTietPhieuNhap
            {
                MaPN = item.MaPN,
                NgayNhap = item.NgayNhap,
                TenSP = item.TenSP,
                SoLuongNhap = item.SoLuong,
                DonGiaNhap = item.DonGiaNhap
            }).ToList();

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult NhapHang(PhieuNhap model, IEnumerable<ChiTietPhieuNhap> lstModel)
        {
            ViewBag.MaNCC = db.NhaCungCaps;
            ViewBag.NgayNhap = DateTime.Today;

            // Get the selected supplier ID (you might need to pass this in the form or retrieve it from the model)
            int? selectedNCCId = model.MaNCC; // Assuming the MaNCC field is part of the model

            // Get the products filtered by the selected supplier
            if (selectedNCCId.HasValue)
            {
                // Assuming that SanPham has MaNCC and MaNSX as foreign keys to NhaCungCap and NhaSanXuat
                ViewBag.ListSanPham = db.SanPhams
                    .Where(sp => sp.MaNCC == selectedNCCId.Value)
                    .ToList();
            }
            else
            {
                // If no supplier is selected, show all products
                ViewBag.ListSanPham = db.SanPhams.ToList();
            }

            model.NgayNhap = ViewBag.NgayNhap;
            model.DaXoa = false;

            // Save the PhieuNhap to get MaPN (Purchase Order ID)
            db.PhieuNhaps.Add(model);
            db.SaveChanges(); // Save to generate the MaPN for ChiTietPhieuNhap

            // Renamed sp to avoid conflict with the outer scope
            foreach (var item in lstModel)
            {
                var product = db.SanPhams.Single(n => n.MaSP == item.MaSP); // Renamed 'sp' to 'product'
                product.SoLuongTon += item.SoLuongNhap; // Update stock quantity

                item.MaPN = model.MaPN; // Assign MaPN to all ChiTietPhieuNhaps
            }

            db.ChiTietPhieuNhaps.AddRange(lstModel);
            db.SaveChanges();

            return View();
        }


        [HttpGet]
        public ActionResult DSSPHetHang()
        {
            //ds sp gần hết hàng với số lượng tồn bé hơn hoặc bằng 5
            var lstSP = db.SanPhams.Where(n => n.DaXoa == false && n.SoLuongTon <= 5);
            return View(lstSP);
        }

        [HttpGet]
        public ActionResult NhapHangDon(int? id)
        {
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.OrderBy(n => n.TenNCC), "MaNCC", "TenNCC");

            if(id == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            if (sp == null)
                return HttpNotFound();
            return View(sp);
        }

        [HttpPost]
        public ActionResult NhapHangDon(PhieuNhap model, ChiTietPhieuNhap ctpn)
        {
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.OrderBy(n => n.TenNCC), "MaNCC", "TenNCC", model.MaNCC);

            model.NgayNhap = DateTime.Now;
            model.DaXoa = false;
            db.PhieuNhaps.Add(model);
            db.SaveChanges();   //save để lấy MaPN gán cho lst chitietpn

            ctpn.MaPN = model.MaPN;
            SanPham sp = db.SanPhams.Single(n => n.MaSP == ctpn.MaSP);
            sp.SoLuongTon += ctpn.SoLuongNhap;
            db.ChiTietPhieuNhaps.Add(ctpn);
            db.SaveChanges();

            return View(sp);
        }

        //Giải phóng dung lượng biến db, để ở cuối controller
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                    db.Dispose();
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
