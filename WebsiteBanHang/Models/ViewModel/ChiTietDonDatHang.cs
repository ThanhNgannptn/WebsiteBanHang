using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanHang.Models.ViewModel
{
    public class ChiTietDonDatHang
    {
        public int MaCTDonDH { get; set; }
        public int MaDDH { get; set; }
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        // Navigation property
        public DonDatHang DonDatHang { get; set; }
        public SanPham SanPham { get; set; }
    }

}