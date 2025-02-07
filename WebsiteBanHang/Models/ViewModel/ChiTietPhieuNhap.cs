using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanHang.Models.ViewModel
{
    public class ChiTietPhieuNhap
    {
        public int MaPN { get; set; }
        public DateTime NgayNhap { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; }
    }
}