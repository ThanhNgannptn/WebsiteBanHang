using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.Mail;
using WebsiteBanHang.Models;
using CaptchaMvc.HtmlHelpers;
using System.Collections.Generic;

namespace WebsiteBanHang.Controllers
{
    public class HomeController : Controller
    {
        QuanLyBanHangEntities db = new QuanLyBanHangEntities();

        // GET: Home/Index
        public ActionResult Index()
        {
            var lstLTM = db.SanPhams.Where(n => n.MaLoaiSP == 1 && n.Moi == 1 && n.DaXoa == false).ToList();
            ViewBag.ListLTM = lstLTM;

            var lstPCM = db.SanPhams.Where(n => n.MaLoaiSP == 2 && n.Moi == 1 && n.DaXoa == false).ToList();
            ViewBag.ListPCM = lstPCM;

            var lstDTM = db.SanPhams.Where(n => n.MaLoaiSP == 7 && n.Moi == 1 && n.DaXoa == false).ToList();
            ViewBag.ListDTM = lstDTM;

            return View();
        }

        public ActionResult MenuPartial()
        {
            var lstSP = db.SanPhams;

            // Kiểm tra session để hiển thị menu động
            if (Session["MaLoaiTV"] != null)
            {
                int maLoaiTV = (int)Session["MaLoaiTV"];
                ViewBag.IsKhachHang = (maLoaiTV == 3); // Xác định nếu là khách hàng
            }
            else
            {
                ViewBag.IsKhachHang = false; // Không đăng nhập
            }

            return PartialView(lstSP);
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            ViewBag.CauHoi = new SelectList(LoadCauHoi());
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(ThanhVien tv)
        {
            ViewBag.CauHoi = new SelectList(LoadCauHoi());
            if (this.IsCaptchaValid("Captcha không hợp lệ!"))
            {
                if (ModelState.IsValid)
                {
                    tv.MaLoaiTV = 3;  // Đặt loại thành viên là khách hàng
                    ViewBag.ThongBao = "Thêm thành công";
                    db.ThanhViens.Add(tv);
                    db.SaveChanges();
                }
                return View();
            }
            ViewBag.ThongBao = "Sai mã Captcha";
            return View();
        }

        public List<string> LoadCauHoi()
        {
            return new List<string>
            {
                "Họ tên người cha bạn là gì?",
                "Ca sĩ mà bạn yêu thích là ai?",
                "Vật nuôi mà bạn yêu thích là gì?",
                "Sở thích của bạn là gì?",
                "Hiện tại bạn đang làm công việc gì?",
                "Trường cấp ba bạn học là gì?",
                "Năm sinh của mẹ bạn là gì?",
                "Bộ phim mà bạn yêu thích là gì?",
                "Bài nhạc mà bạn yêu thích là gì?"
            };
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection f)
        {
            string taikhoan = f["txtTenDangNhap"].ToString();
            string matKhau = f["txtMatKhau"].ToString();

            ThanhVien tv = db.ThanhViens.SingleOrDefault(n => n.TaiKhoan == taikhoan && n.MatKhau == matKhau);
            if (tv != null)
            {
                Session["MaThanhVien"] = tv.MaThanhVien;
                var lstQuyen = db.LoaiThanhVien_Quyen.Where(n => n.MaLoaiTV == tv.MaLoaiTV);
                string Quyen = "";
                if (lstQuyen.Count() != 0)
                {
                    foreach (var item in lstQuyen)
                    {
                        Quyen += item.Quyen.MaQuyen + ",";
                    }
                    Quyen = Quyen.Substring(0, Quyen.Length - 1);
                    PhanQuyen(tv.TaiKhoan.ToString(), Quyen);

                    Session["TaiKhoan"] = tv;
                    return Content(@"<script>window.location.reload()</script>");
                }
            }
            return Content("Tài khoản hoặc mật khẩu không chính xác.");
        }

        public ActionResult DangXuat()
        {
            // Xóa thông tin người dùng trong session và đăng xuất
            Session["TaiKhoan"] = null;
            Session["MaKH"] = null;
            Session["MaLoaiTV"] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        public ActionResult LoiPhanquyen()
        {
            return View();
        }

        public void PhanQuyen(string Taikhoan, string Quyen)
        {
            FormsAuthentication.Initialize();

            var ticket = new FormsAuthenticationTicket(1,
                                            Taikhoan,
                                            DateTime.Now,
                                            DateTime.Now.AddHours(3),
                                            false,
                                            Quyen,
                                            FormsAuthentication.FormsCookiePath);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            if (ticket.IsPersistent) cookie.Expires = ticket.Expiration;
            Response.Cookies.Add(cookie);
        }

        // Quên mật khẩu
        [HttpGet]
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuenMatKhau(string email)
        {
            ThanhVien tv = db.ThanhViens.SingleOrDefault(n => n.Email == email);

            if (tv != null)
            {
                string newPass = Guid.NewGuid().ToString().Substring(0, 8);

                tv.MatKhau = newPass;
                db.SaveChanges();

                GuiEmail(email, "Yêu cầu cấp lại mật khẩu", $"Mật khẩu mới của bạn là: {newPass}");

                ViewBag.Message = "Mật khẩu mới đã được gửi đến email của bạn.";
                return View();
            }

            ViewBag.Message = "Email không tồn tại trong hệ thống.";
            return View();
        }

        public void GuiEmail(string emailTo, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(emailTo);
                mail.From = new MailAddress("2121005173@sv.ufm.edu.vn", "Website Bán Hàng", System.Text.Encoding.UTF8);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("2121005173@sv.ufm.edu.vn", "imfr vgxw eygi mscd"),
                    EnableSsl = true
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                throw new Exception("Gửi email thất bại: " + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                    db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
