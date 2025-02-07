using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanHang.Models.ViewModel
{
    public class DonDatHang
    {
        public int MaDDH { get; set; }
        public DateTime NgayDat { get; set; }
        public string TinhTrangGiaoHang { get; set; }
        public DateTime? NgayGiao { get; set; }
        public bool? DaThanhToan { get; set; }
        public int? MaKH { get; set; }
        public string UuDai { get; set; }
        public bool DaXoa { get; set; }

        // Navigation property
        public KhachHang KhachHang { get; set; }
        public ICollection<ChiTietDonDatHang> ChiTietDonDatHangs { get; set; }
    }

}