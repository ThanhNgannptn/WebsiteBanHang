using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Models.ViewModel;

namespace WebsiteBanHang.Controllers
{
    public class ReportController : Controller
    {
        QuanLyBanHangEntities db = new QuanLyBanHangEntities();

        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        // GET: Báo cáo doanh thu
        public ActionResult ReportDT(DateTime? fromDate, DateTime? toDate, string reportType)
        {
            // Kiểm tra tham số đầu vào
            if (fromDate == null || toDate == null || string.IsNullOrEmpty(reportType))
            {
                // Nếu không có tham số đầy đủ, trả về trang báo cáo
                return View("Index");
            }

            var reportData = new List<ReportItem>(); // Sử dụng lớp ReportItem để lưu kết quả báo cáo

            // Lấy dữ liệu báo cáo dựa trên loại báo cáo
            if (reportType == "Revenue")
            {
                // Lấy doanh thu theo ngày trong khoảng thời gian
                var lstDDH = db.DonDatHangs
                    .Where(n => n.NgayDat >= fromDate && n.NgayDat <= toDate)
                    .ToList();

                // Group the data by date and sum the revenue for each day
                var groupedData = lstDDH
                    .GroupBy(item => item.NgayDat.Value.Date)  // Group by date (ignoring time)
                    .Select(group => new
                    {
                        Date = group.Key,
                        TotalRevenue = group.Sum(item => item.ChiTietDonDatHangs.Sum(n => n.SoLuong * n.Dongia) ?? 0m)
                    })
                    .ToList();

                foreach (var item in groupedData)
                {
                    reportData.Add(new ReportItem
                    {
                        Date = item.Date.ToString("dd/MM/yyyy"),
                        Value = item.TotalRevenue
                    });
                }
            }
            else if (reportType == "Quantity")
            {
                // Lấy số lượng sản phẩm bán ra trong khoảng thời gian
                var lstDDH = db.DonDatHangs
                    .Where(n => n.NgayDat >= fromDate && n.NgayDat <= toDate)
                    .ToList();

                // Group the data by date and sum the quantity for each day
                var groupedData = lstDDH
                    .GroupBy(item => item.NgayDat.Value.Date)  // Group by date (ignoring time)
                    .Select(group => new
                    {
                        Date = group.Key,
                        TotalQuantity = group.Sum(item => item.ChiTietDonDatHangs.Sum(n => n.SoLuong) ?? 0)
                    })
                    .ToList();

                foreach (var item in groupedData)
                {
                    reportData.Add(new ReportItem
                    {
                        Date = item.Date.ToString("dd/MM/yyyy"),
                        Value = item.TotalQuantity
                    });
                }
            }

            // Return the data to the view
            ViewBag.ReportData = reportData;
            ViewBag.ReportType = reportType;

            return View("Index");
        }




        // Giải phóng dung lượng biến db
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
