using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Controllers
{
    public class GioHangController : Controller
    {
        QuanLyBanHangEntities db = new QuanLyBanHangEntities();

        //Hiển thị icon giỏ hàng lên phần header
        public ActionResult GioHangPartial()
        {
            if (TinhTongSoLuong() == 0)
            {
                ViewBag.TongSoLuong = 0;
                ViewBag.TongTien = 0;
                return PartialView();
            }
            ViewBag.TongSoLuong = TinhTongSoLuong();
            ViewBag.TongTien = TinhTongTien();

            return PartialView();
        }

        //Thêm giỏ hàng thông thường không ajax
        public ActionResult ThemGioHang(int MaSP, string strURL)
        {
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.MaSP == MaSP);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            List<ItemGioHang> lstGioHang = LayGioHang();

            ItemGioHang spCheck = lstGioHang.SingleOrDefault(n => n.MaSP == MaSP);
            if (spCheck != null)
            {
                if (sp.SoLuongTon < spCheck.SoLuong)
                    return View("ThongBao");

                spCheck.SoLuong++;
                spCheck.ThanhTien = spCheck.SoLuong * spCheck.DonGia;
                return Redirect(strURL);
            }

            ItemGioHang itemGH = new ItemGioHang(MaSP);
            if (sp.SoLuongTon < itemGH.SoLuong)
                return View("ThongBao");

            lstGioHang.Add(itemGH);
            return Redirect(strURL);
        }

        public ActionResult XemGioHang()
        {
            List<ItemGioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TinhTongSoLuong();
            ViewBag.TongTien = TinhTongTien();

            return View(lstGioHang);
        }

        [HttpGet]
        public ActionResult SuaGioHang(int MaSP)
        {
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.MaSP == MaSP);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            List<ItemGioHang> lstGioHang = LayGioHang();
            ItemGioHang spCheck = lstGioHang.SingleOrDefault(n => n.MaSP == MaSP);
            if (spCheck == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.GioHang = lstGioHang;
            return View(spCheck);
        }

        [HttpPost]
        public ActionResult CapNhatGioHang(ItemGioHang itemGH)
        {
            SanPham spCheck = db.SanPhams.Single(n => n.MaSP == itemGH.MaSP);
            if (spCheck.SoLuongTon < itemGH.SoLuong)
            {
                return View("ThongBao");
            }

            List<ItemGioHang> lstGH = LayGioHang();
            ItemGioHang itemGHUpdate = lstGH.Find(n => n.MaSP == itemGH.MaSP);

            itemGHUpdate.SoLuong = itemGH.SoLuong;
            itemGHUpdate.ThanhTien = itemGHUpdate.SoLuong * itemGHUpdate.DonGia;

            return RedirectToAction("XemGioHang");
        }

        public ActionResult XoaGioHang(int MaSP)
        {
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.MaSP == MaSP);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            List<ItemGioHang> lstGioHang = LayGioHang();
            ItemGioHang spCheck = lstGioHang.SingleOrDefault(n => n.MaSP == MaSP);
            if (spCheck == null)
            {
                return RedirectToAction("Index", "Home");
            }

            lstGioHang.Remove(spCheck);

            return RedirectToAction("XemGioHang");
        }

        [HttpPost]
        public ActionResult DatHang(KhachHang kh)
        {
            if (Session["GioHang"] == null || TinhTongSoLuong() == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra nếu thông tin khách hàng không hợp lệ
            if (string.IsNullOrWhiteSpace(kh.TenKH))
            {
                ModelState.AddModelError("TenKH", "Tên khách hàng không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(kh.DiaChi))
            {
                ModelState.AddModelError("DiaChi", "Địa chỉ không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(kh.Email) || !new EmailAddressAttribute().IsValid(kh.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
            }
            if (string.IsNullOrWhiteSpace(kh.SoDienThoai) || !kh.SoDienThoai.All(char.IsDigit) || kh.SoDienThoai.Length < 10 || kh.SoDienThoai.Length > 15)
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại không hợp lệ. Vui lòng nhập lại.");
            }

            // Nếu có lỗi, trả về trang giỏ hàng và hiển thị thông báo
            if (!ModelState.IsValid)
            {
                List<ItemGioHang> lstGioHang = LayGioHang();
                ViewBag.TongSoLuong = TinhTongSoLuong();
                ViewBag.TongTien = TinhTongTien();
                return View("XemGioHang", lstGioHang);
            }

            KhachHang khach = new KhachHang();
            if (Session["TaiKhoan"] == null)
            {
                khach = kh;
                Models.KhachHang khachHang = db.KhachHangs.Add(khach);
                db.SaveChanges();
            }
            else
            {
                ThanhVien tv = (ThanhVien)Session["TaiKhoan"];
                khach.TenKH = tv.HoTen;
                khach.DiaChi = tv.DiaChi;
                khach.Email = tv.Email;
                khach.SoDienThoai = tv.SoDienThoai;
                db.KhachHangs.Add(khach);
                db.SaveChanges();
            }

            DonDatHang ddh = new DonDatHang
            {
                MaKH = int.Parse(khach.MaKH.ToString()),
                NgayDat = DateTime.Now,
                TinhTrangGiaoHang = false,
                DaThanhToan = false,
                UuDai = 0,
                DaHuy = false,
                DaXoa = false
            };

            db.DonDatHangs.Add(ddh);
            db.SaveChanges();

            List<ItemGioHang> lstGH = LayGioHang();
            foreach (var item in lstGH)
            {
                ChiTietDonDatHang ctdh = new ChiTietDonDatHang
                {
                    MaDDH = ddh.MaDDH,
                    MaSP = item.MaSP,
                    TenSP = item.TenSP,
                    SoLuong = item.SoLuong,
                    Dongia = item.DonGia
                };
                db.ChiTietDonDatHangs.Add(ctdh);
            }
            db.SaveChanges();

            // Clear cart session
            Session["GioHang"] = null;

            // Gửi email
            GuiEmailXacNhanDonHang(khach.Email, ddh);

            return RedirectToAction("ThongBaoDatHang", new { maDDH = ddh.MaDDH });
        }


        public ActionResult ThongBaoDatHang(int maDDH)
        {
            var donDatHang = db.DonDatHangs.Include("ChiTietDonDatHangs").FirstOrDefault(ddh => ddh.MaDDH == maDDH);
            if (donDatHang == null)
            {
                return HttpNotFound();
            }

            return View(donDatHang);
        }

        private void GuiEmailXacNhanDonHang(string email, DonDatHang ddh)
        {
            try
            {
                // Lấy danh sách chi tiết đơn hàng
                var chiTietDonHangs = db.ChiTietDonDatHangs.Where(ct => ct.MaDDH == ddh.MaDDH).ToList();

                // Tạo nội dung email
                string body = $@"
    <html>
        <body>
            <h2>Xin chào,</h2>
            <p>Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi. Dưới đây là thông tin chi tiết đơn hàng của bạn:</p>
            <h3>Thông tin đơn hàng</h3>
            <table border='1' cellpadding='5' cellspacing='0' style='width: 100%; border-collapse: collapse;'>
                <thead>
                    <tr style='background-color: #f2f2f2;'>
                        <th style='text-align: left;'>Tên sản phẩm</th>
                        <th style='text-align: right;'>Đơn giá</th>
                        <th style='text-align: center;'>Số lượng</th>
                        <th style='text-align: right;'>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>";

                decimal tongTien = 0;

                // Định dạng tiền tệ cho từng dòng
                var cultureInfo = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");

                foreach (var item in chiTietDonHangs)
                {
                    decimal dongia = Convert.ToDecimal(item.Dongia);
                    int soLuong = Convert.ToInt32(item.SoLuong);
                    decimal thanhTien = dongia * soLuong;
                    tongTien += thanhTien;

                    body += $@"
                    <tr>
                        <td>{item.TenSP}</td>
                        <td style='text-align: right;'>{dongia.ToString("C", cultureInfo)}</td>
                        <td style='text-align: center;'>{soLuong}</td>
                        <td style='text-align: right;'>{thanhTien.ToString("C", cultureInfo)}</td>
                    </tr>";
                }

                // Tổng cộng tiền
                body += $@"
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan='3' style='text-align: right; font-weight: bold;'>Tổng tiền:</td>
                        <td style='text-align: right; font-weight: bold;'>{tongTien.ToString("C", cultureInfo)}</td>
                    </tr>
                </tfoot>
            </table>
            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc số điện thoại hỗ trợ.</p>
            <p>Trân trọng,</p>
            <p><b>Đội ngũ hỗ trợ khách hàng</b></p>
        </body>
    </html>";

                // Gửi email
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("2121005173@sv.ufm.edu.vn", "HEAD FATACO2");
                mail.To.Add(email);
                mail.Subject = "Xác nhận đơn hàng #" + ddh.MaDDH;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("2121005173@sv.ufm.edu.vn", "imfr vgxw eygi mscd"),
                    EnableSsl = true
                };

                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi gửi email
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }
        }

        #region Methods
        public List<ItemGioHang> LayGioHang()
        {
            List<ItemGioHang> lstGioHang = Session["GioHang"] as List<ItemGioHang>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<ItemGioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }

        public double TinhTongSoLuong()
        {
            List<ItemGioHang> lstGioHang = Session["GioHang"] as List<ItemGioHang>;
            if (lstGioHang == null)
            {
                return 0;
            }
            return lstGioHang.Sum(n => n.SoLuong);
        }

        public decimal TinhTongTien()
        {
            List<ItemGioHang> lstGioHang = Session["GioHang"] as List<ItemGioHang>;
            if (lstGioHang == null)
            {
                return 0;
            }
            return lstGioHang.Sum(n => n.ThanhTien);
        }
        #endregion

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
