using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanHang.Models.ViewModel
{
    public class ChiTietSanPham
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal DonGia { get; set; }
        public string MoTa { get; set; }
        public int? SoLuongTon { get; set; }
        public int? LuotXem { get; set; }
        public int? LuotBinhChon { get; set; }
        public int? LuotBinhLuan { get; set; }
        public int? SoLuongBan { get; set; }
        public int? Moi { get; set; }
        public int? MaNCC { get; set; }
        public int? MaNSX { get; set; }
        public int? MaLoaiSP { get; set; }
        public bool? DaXoa { get; set; }

        // Additional properties for related data
        public string TenNCC { get; set; }  // Tên nhà cung cấp
        public string TenNSX { get; set; }  // Tên nhà sản xuất
        public string TenLoaiSP { get; set; } // Tên loại sản phẩm

        // Navigation properties (optional)
        public LoaiSanPham LoaiSanPham { get; set; }
        public NhaSanXuat NhaSanXuat { get; set; }
        public NhaCungCap NhaCungCap { get; set; }
    }
}