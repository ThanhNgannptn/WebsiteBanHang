using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly QuanLyBanHangEntities _context;

        public KhachHangController()
        {
            _context = new QuanLyBanHangEntities();
        }

        // GET: KhachHang/Index
        public ActionResult Index()
        {
            // Kiểm tra nếu người dùng chưa đăng nhập
            if (Session["MaThanhVien"] == null)
            {
                return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
            }

            int currentMaThanhVien = (int)Session["MaThanhVien"];

            // Lấy thông tin thành viên dựa vào MaThanhVien
            var customer = _context.ThanhViens.FirstOrDefault(tv => tv.MaThanhVien == currentMaThanhVien);

            // Kiểm tra nếu không tìm thấy thành viên
            if (customer == null)
            {
                return Content("Không tìm thấy thông tin khách hàng!");
            }

            // Kiểm tra MaLoaiTV (chỉ cho phép MaLoaiTV = 3 hoặc 4)
            if ((customer.MaLoaiTV == 1) || (customer.MaLoaiTV == 2))  // Fixed the logic
            {
                return RedirectToAction("LoiPhanQuyen", "Home"); // Chuyển hướng đến trang lỗi phân quyền
            }

            // Trả về danh sách khách hàng cho view
            var customerList = new List<WebsiteBanHang.Models.ThanhVien> { customer }; // Explicitly refer to the ThanhVien model in the Models namespace
            return View(customerList); // Truyền danh sách khách hàng cho view
        }

        // GET: KhachHang/ChinhSua/{id}
        public ActionResult ChinhSua(int id)
        {
            // Tìm khách hàng theo ID
            var thanhVien = _context.ThanhViens.FirstOrDefault(t => t.MaThanhVien == id);

            // Nếu không tìm thấy khách hàng
            if (thanhVien == null)
            {
                return HttpNotFound();
            }

            // Trả về view và truyền dữ liệu khách hàng
            return View(thanhVien); // Trả về đối tượng thanhVien thay vì customer
        }

        // POST: KhachHang/ChinhSua/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChinhSua(ThanhVien model)
        {
            if (ModelState.IsValid)
            {
                var thanhVien = _context.ThanhViens.FirstOrDefault(tv => tv.MaThanhVien == model.MaThanhVien);

                if (thanhVien == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thành viên!";
                    return RedirectToAction("Index");
                }

                thanhVien.TaiKhoan = model.TaiKhoan;
                thanhVien.MaLoaiTV = model.MaLoaiTV;

                thanhVien.HoTen = model.HoTen;
                thanhVien.DiaChi = model.DiaChi;
                thanhVien.Email = model.Email;
                thanhVien.SoDienThoai = model.SoDienThoai;

                thanhVien.MatKhau = model.MatKhau;
                thanhVien.CauHoi = model.CauHoi;
                thanhVien.CauTraLoi = model.CauTraLoi;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình chỉnh sửa!";
            ViewBag.LoaiThanhVien = new SelectList(_context.LoaiThanhViens, "Value", "Text", model.MaLoaiTV);
            return View(model);
        }


        // Define the HashPassword method here
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashedBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Optional: Dispose database context
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
