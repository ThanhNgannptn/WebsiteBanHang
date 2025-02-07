using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Controllers
{
    [Authorize(Roles = "QuanLy,QuanTriWeb")]
    public class QuanLyDonHangController : Controller
    {
        QuanLyBanHangEntities db = new QuanLyBanHangEntities();

        // GET: QuanLyDonHang
        public ActionResult ChuaThanhToan(DateTime? startDate, DateTime? endDate, string customerName, string productName, bool? removeFilter)
        {
            // Kiểm tra nếu cần xóa bộ lọc
            if (removeFilter.HasValue && removeFilter.Value)
            {
                startDate = null;
                endDate = null;
                customerName = null;
            }

            // Lọc và sắp xếp các đơn hàng theo các tham số đã cung cấp
            var lst = db.DonDatHangs
                .Where(n => n.DaThanhToan == false)  // Chỉ lấy các đơn hàng chưa thanh toán
                .Where(n =>
                    // Lọc theo ngày nếu có
                    ((startDate == null && endDate == null) ||
                     (startDate != null && endDate != null && n.NgayDat >= startDate && n.NgayDat <= endDate)) &&

                    // Lọc theo tên khách hàng nếu có
                    (string.IsNullOrEmpty(customerName) || n.KhachHang.TenKH.Contains(customerName)) 

                    // Lọc theo tên sản phẩm nếu có
                )
                .OrderByDescending(n => n.NgayDat);  // Sắp xếp theo ngày mới nhất
                ViewBag.StartDate = startDate;
    ViewBag.EndDate = endDate;
    ViewBag.CustomerName = customerName;
            return View(lst);
        }



        public ActionResult ChuaGiao(DateTime? startDate, DateTime? endDate, string customerName, bool? removeFilter)
        {
            if (removeFilter.HasValue && removeFilter.Value)
            {
                startDate = null;
                endDate = null;
                customerName = null;
            }

            // Lọc và sắp xếp các đơn hàng theo các tham số đã cung cấp
            var lstDSDHCG = db.DonDatHangs
                .Where(n => n.TinhTrangGiaoHang == false && n.DaThanhToan == true)
                .Where(n =>
                    // Lọc theo ngày nếu có
                    ((startDate == null && endDate == null) ||
                     (startDate != null && endDate != null && n.NgayDat >= startDate && n.NgayDat <= endDate)) &&

                    // Lọc theo tên khách hàng nếu có
                    (string.IsNullOrEmpty(customerName) || n.KhachHang.TenKH.Contains(customerName))

                // Lọc theo tên sản phẩm nếu có
                )
                .OrderByDescending(n => n.NgayGiao);  // Sắp xếp theo ngày giao mới nhất
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.CustomerName = customerName;
            return View(lstDSDHCG);
        }

        public ActionResult DaGiaoDaThanhToan(DateTime? startDate, DateTime? endDate, string customerName, bool? removeFilter)
        {
            if (removeFilter.HasValue && removeFilter.Value)
            {
                startDate = null;
                endDate = null;
                customerName = null;
            }

            // Lọc và sắp xếp các đơn hàng theo các tham số đã cung cấp
            var lstDSDHCG = db.DonDatHangs
                .Where(n => n.TinhTrangGiaoHang == true && n.DaThanhToan == true)
               .Where(n =>
                    // Lọc theo ngày nếu có
                    ((startDate == null && endDate == null) ||
                     (startDate != null && endDate != null && n.NgayDat >= startDate && n.NgayDat <= endDate)) &&

                    // Lọc theo tên khách hàng nếu có
                    (string.IsNullOrEmpty(customerName) || n.KhachHang.TenKH.Contains(customerName))

                // Lọc theo tên sản phẩm nếu có
                )
                .OrderByDescending(n => n.NgayGiao);  // Sắp xếp theo ngày giao mới nhất
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.CustomerName = customerName;

            return View(lstDSDHCG);
        }

        [HttpGet]
        public ActionResult DuyetDonHang(int? id)
        {
            // Check if the id is valid
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DonDatHang model = db.DonDatHangs.SingleOrDefault(n => n.MaDDH == id);

            // Check if the order exists
            if (model == null)
            {
                return HttpNotFound();
            }

            // Display the details of the order
            var lstChiTietDH = db.ChiTietDonDatHangs.Where(n => n.MaDDH == id);
            ViewBag.ListChiTietDH = lstChiTietDH;
            ViewBag.TenKH = model.KhachHang.TenKH;

            return View(model);
        }

        [HttpPost]
        public ActionResult DuyetDonHang(DonDatHang ddh)
        {
            DonDatHang ddhUpdate = db.DonDatHangs.Single(n => n.MaDDH == ddh.MaDDH);

            // Check if the order is marked as delivered but not paid
            if (ddh.TinhTrangGiaoHang == true && ddh.DaThanhToan == false)
            {
                // Set the error message
                ViewBag.ErrorMessage = "Không thể chọn Đã giao khi chưa thanh toán. Vui lòng chọn 'Đã Thanh Toán'.";
                return View(ddhUpdate); // Return the view with the error message
            }

            // If the payment is true and delivery status is changed, update the delivery date
            if (ddh.TinhTrangGiaoHang == true && ddhUpdate.TinhTrangGiaoHang == false)
            {
                ddhUpdate.NgayGiao = DateTime.Now; // Set the delivery date to the current date
            }

            // Update payment and delivery status
            ddhUpdate.DaThanhToan = ddh.DaThanhToan;
            ddhUpdate.TinhTrangGiaoHang = ddh.TinhTrangGiaoHang;

            // Save changes to the database
            db.SaveChanges();

            var lstChiTietDH = db.ChiTietDonDatHangs.Where(n => n.MaDDH == ddh.MaDDH);
            ViewBag.ListChiTietDH = lstChiTietDH;

            // Send an email to the customer regarding the order status update
            SendOrderStatusUpdateEmail(ddhUpdate);

            // Set a success message
            ViewBag.SuccessMessage = "Đơn hàng đã được lưu thành công.";

            return View(ddhUpdate);
        }




        private void SendOrderStatusUpdateEmail(DonDatHang order)
        {
            try
            {
                // Lấy email của khách hàng từ đối tượng KhachHang liên quan đến đơn hàng
                var customerEmail = order.KhachHang?.Email;

                if (string.IsNullOrEmpty(customerEmail))
                {
                    Console.WriteLine("Không tìm thấy email của khách hàng.");
                    return;
                }

                var fromAddress = new MailAddress("2121005173@sv.ufm.edu.vn", "HEAD FATACO 2");
                var toAddress = new MailAddress(customerEmail, "Khách Hàng");
                const string subject = "Cập nhật trạng thái đơn hàng";

                string body = $"<html><body>" +
                              $"<p>Chào {order.KhachHang.TenKH},</p>" +
                              $"<p>Đơn hàng của bạn với ID <b>{order.MaDDH}</b> đã được cập nhật:</p>" +
                              $"<p><b>Ngày đặt hàng:</b> {(order.NgayDat.HasValue ? order.NgayDat.Value.ToString("dd/MM/yyyy") : "Không có thông tin")}</p>" +
                              $"<p><b>Trạng thái thanh toán:</b> {(order.DaThanhToan.GetValueOrDefault() ? "Đã Thanh Toán" : "Chưa Thanh Toán")}</p>" +
                              $"<p><b>Trạng thái giao hàng:</b> {(order.TinhTrangGiaoHang.GetValueOrDefault() ? "Đã Giao" : "Chưa giao")}</p>" +
                              $"<p>Danh sách sản phẩm:</p>" +
                              $"<table border='1'>" +
                              $"<tr><th>Sản phẩm</th><th>Số lượng</th><th>Giá</th></tr>";

                foreach (var item in order.ChiTietDonDatHangs)
                {
                    var product = item.SanPham;
                    body += $"<tr>" +
                            $"<td>{product.TenSP}</td>" +
                            $"<td>{item.SoLuong}</td>" +
                            $"<td>{item.Dongia}</td>" +
                            $"</tr>";
                }

                body += "</table>" +
                        $"<p>Cảm ơn quý khách đã tin tưởng mua sắm tại cửa hàng của chúng tôi!</p>" +
                        $"</body></html>";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("2121005173@sv.ufm.edu.vn", "imfr vgxw eygi mscd"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
            }
        }

        // Dispose method to release resources
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
