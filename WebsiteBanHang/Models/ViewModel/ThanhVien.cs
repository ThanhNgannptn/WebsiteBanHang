using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanHang.Models.ViewModel
{
    public class ThanhVien
    {
        public int MaThanhVien { get; set; }
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public int? MaLoaiTV { get; set; }

        // Navigation property
        public LoaiThanhVien LoaiThanhVien { get; set; }
        public ICollection<KhachHang> KhachHangs { get; set; }
    }

}