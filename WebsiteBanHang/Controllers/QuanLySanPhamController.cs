﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Controllers
{
    [Authorize(Roles = "QuanLy,QuanTriWeb")]
    public class QuanLySanPhamController : Controller
    {
        QuanLyBanHangEntities db = new QuanLyBanHangEntities();
        // GET: QuanLySanPham
        public ActionResult Index(string searchTerm)
        {
            // Lấy danh sách sản phẩm chưa bị xóa
            var sanPhams = db.SanPhams.Where(n => n.DaXoa == false);

            // Nếu có từ khóa tìm kiếm, lọc danh sách sản phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sanPhams = sanPhams.Where(n => n.TenSP.Contains(searchTerm));
            }

            // Sắp xếp danh sách theo mã sản phẩm
            var sortedSanPhams = sanPhams.OrderByDescending(n => n.MaSP);

            // Đảm bảo sản phẩm được trả về sau khi thêm
            return View(sortedSanPhams.ToList());
        }

        [HttpGet]
        public ActionResult TaoMoi()
        {
            // Load dropdownlist nhà cung cấp và dropdownlist loại sp, mã nhà sx
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.OrderBy(n => n.TenNCC), "MaNCC", "TenNCC");
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams.OrderBy(n => n.MaLoaiSP), "MaLoaiSP", "TenLoai");
            ViewBag.MaNSX = new SelectList(db.NhaSanXuats.OrderBy(n => n.MaNSX), "MaNSX", "TenNSX");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult TaoMoi(SanPham sp, HttpPostedFileBase HinhAnh, HttpPostedFileBase HinhAnh1, HttpPostedFileBase HinhAnh2, HttpPostedFileBase HinhAnh3, HttpPostedFileBase HinhAnh4)
        {
            // Load dropdownlist nhà cung cấp và dropdownlist loại sp, mã nhà sx
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.OrderBy(n => n.TenNCC), "MaNCC", "TenNCC");
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams.OrderBy(n => n.MaLoaiSP), "MaLoaiSP", "TenLoai");
            ViewBag.MaNSX = new SelectList(db.NhaSanXuats.OrderBy(n => n.MaNSX), "MaNSX", "TenNSX");

            // Handle image uploads
            #region ThemHinhAnh
            sp.HinhAnh = SaveImage(HinhAnh);
            sp.HinhAnh1 = SaveImage(HinhAnh1);
            sp.HinhAnh2 = SaveImage(HinhAnh2);
            sp.HinhAnh3 = SaveImage(HinhAnh3);
            sp.HinhAnh4 = SaveImage(HinhAnh4);
            #endregion

            // Add the product to the database and save changes
            db.SanPhams.Add(sp);
            db.SaveChanges();

            // Kiểm tra lại nếu sản phẩm được lưu chính xác
            if (sp.MaSP == 0) // Kiểm tra nếu mã sản phẩm không được sinh
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sản phẩm.");
                return View(sp);
            }

            return RedirectToAction("Index");
        }

        // Method to save image and return the file name
        private string SaveImage(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);  // Lấy tên hình
                var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), fileName); // Lưu vào thư mục HinhAnhSP

                // Lưu hình vào thư mục
                file.SaveAs(path);
                return fileName;  // Trả về tên file
            }
            return null;  // Nếu không có hình, trả về null
        }



        [HttpGet]
        public ActionResult ChinhSua(int? id)
        {
            //lấy sp cần chỉnh sửa
            if(id ==null)
            {
                Response.StatusCode = 404;
                return null;
            }
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            if(sp == null)
            {
                return HttpNotFound();
            }

            //Load dropdownlist nhà cung cấp và dropdownlist loại sp, mã nhà sx
            ViewBag.MaNCC = TaoDanhSachMaNCC(sp.MaNCC.Value);
            ViewBag.MaLoaiSP = TaoDanhSachLoaiSP(sp.MaLoaiSP.Value);
            ViewBag.MaNSX = TaoDanhSachMaNSX(sp.MaNSX.Value);

            return View(sp);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ChinhSua([Bind(Include = "MaSP,TenSP,DonGia,NgayCapNhat,CauHinh,MoTa,SoLuongTon,LuotXem,LuotBinhChon,LuotBinhLuan,SoLuotMua,Moi,MaNCC,MaNSX,MaLoaiSP,DaXoa")] SanPham sp)
        {
            
            if (ModelState.IsValid)
            {
                SanPham sanPham = db.SanPhams.Find(sp.MaSP);
                TryUpdateModel(sanPham, new string[] { "MaSP", "TenSP", "DonGia", "NgayCapNhat", "CauHinh", "MoTa",
                "SoLuongTon", "LuotXem", "LuotBinhChon", "LuotBinhLuan", "SoLuotMua","Moi","MaNCC","MaNSX","MaLoaiSP","DaXoa"});
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC", sp.MaNCC);
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams, "MaLoaiSP", "TenLoai", sp.MaLoaiSP);
            ViewBag.MaNSX = new SelectList(db.NhaSanXuats, "MaNSX", "TenNSX", sp.MaNSX);

            return View(sp);
        }

        [HttpGet]
        public ActionResult UploadHinh(int? id)
        {
            if (id == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            SanPham sp = db.SanPhams.Find(id);
            if (sp == null)
            {
                return HttpNotFound();
            }

            return View(sp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadHinh(int id, HttpPostedFileBase HinhAnh, HttpPostedFileBase HinhAnh1, HttpPostedFileBase HinhAnh2, HttpPostedFileBase HinhAnh3, HttpPostedFileBase HinhAnh4)
        {
            SanPham sp = db.SanPhams.Find(id);
            
            if(HinhAnh != null)
            {
                string path = Server.MapPath("~/Content/HinhAnhSP/");
                string Ten = null;
                HinhAnh.SaveAs(path + HinhAnh.FileName);
                Ten = HinhAnh.FileName;

                if(!string.IsNullOrEmpty(sp.HinhAnh))
                {
                    string pathAndFname = Server.MapPath($"~/Content/HinhAnhSP/{sp.HinhAnh}");
                    if (System.IO.File.Exists(pathAndFname)) 
                        System.IO.File.Delete(pathAndFname);
                }
                sp.HinhAnh = Ten;
            }
            if (HinhAnh1 != null)
            {
                string path = Server.MapPath("~/Content/HinhAnhSP/");
                string Ten = null;
                HinhAnh1.SaveAs(path + HinhAnh1.FileName);
                Ten = HinhAnh1.FileName;

                if (!string.IsNullOrEmpty(sp.HinhAnh1))
                {
                    string pathAndFname = Server.MapPath($"~/Content/HinhAnhSP/{sp.HinhAnh1}");
                    if (System.IO.File.Exists(pathAndFname))
                        System.IO.File.Delete(pathAndFname);
                }
                sp.HinhAnh1 = Ten;
            }
            if (HinhAnh2 != null)
            {
                string path = Server.MapPath("~/Content/HinhAnhSP/");
                string Ten = null;
                HinhAnh2.SaveAs(path + HinhAnh2.FileName);
                Ten = HinhAnh2.FileName;

                if (!string.IsNullOrEmpty(sp.HinhAnh2))
                {
                    string pathAndFname = Server.MapPath($"~/Content/HinhAnhSP/{sp.HinhAnh2}");
                    if (System.IO.File.Exists(pathAndFname))
                        System.IO.File.Delete(pathAndFname);
                }
                sp.HinhAnh2 = Ten;
            }
            if (HinhAnh3 != null)
            {
                string path = Server.MapPath("~/Content/HinhAnhSP/");
                string Ten = null;
                HinhAnh3.SaveAs(path + HinhAnh3.FileName);
                Ten = HinhAnh3.FileName;

                if (!string.IsNullOrEmpty(sp.HinhAnh3))
                {
                    string pathAndFname = Server.MapPath($"~/Content/HinhAnhSP/{sp.HinhAnh3}");
                    if (System.IO.File.Exists(pathAndFname))
                        System.IO.File.Delete(pathAndFname);
                }
                sp.HinhAnh3 = Ten;
            }
            if (HinhAnh4 != null)
            {
                string path = Server.MapPath("~/Content/HinhAnhSP/");
                string Ten = null;
                HinhAnh4.SaveAs(path + HinhAnh4.FileName);
                Ten = HinhAnh4.FileName;

                if (!string.IsNullOrEmpty(sp.HinhAnh4))
                {
                    string pathAndFname = Server.MapPath($"~/Content/HinhAnhSP/{sp.HinhAnh4}");
                    if (System.IO.File.Exists(pathAndFname))
                        System.IO.File.Delete(pathAndFname);
                }
                sp.HinhAnh4 = Ten;
            }

            db.SaveChanges();

            //upload thành công
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public ActionResult Xoa(int? id)
        {
            //lấy sp cần chỉnh sửa
            if (id == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            if (sp == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.OrderBy(n => n.TenNCC), "MaNCC", "TenNCC", sp.MaNCC);
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams.OrderBy(n => n.MaLoaiSP), "MaLoaiSP", "TenLoai", sp.MaLoaiSP);
            ViewBag.MaNSX = new SelectList(db.NhaSanXuats.OrderBy(n => n.MaNSX), "MaNSX", "TenNSX", sp.MaNSX);
            
            return View(sp);
        }

        [HttpPost]
        public ActionResult Xoa(int id)
        {
            //lấy sp cần chỉnh sửa
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            if (sp == null)
            {
                return HttpNotFound();
            }
            db.SanPhams.Remove(sp);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        private SelectList TaoDanhSachMaNCC(int IDChon = 0)
        {
            var items = db.NhaCungCaps.Select(p => new { p.MaNCC, ThongTin = p.TenNCC }).ToList();
            var result = new SelectList(items, "MaNCC", "ThongTin", selectedValue: IDChon);
            return result;
        }
        private SelectList TaoDanhSachMaNSX(int IDChon = 0)
        {
            var items = db.NhaSanXuats.Select(p => new { p.MaNSX, ThongTin = p.TenNSX }).ToList();
            var result = new SelectList(items, "MaNSX", "ThongTin", selectedValue: IDChon);
            return result;
        }
        private SelectList TaoDanhSachLoaiSP(int IDChon = 0)
        {
            var items = db.LoaiSanPhams.Select(p => new { p.MaLoaiSP, ThongTin = p.TenLoai }).ToList();
            var result = new SelectList(items, "MaLoaiSP", "ThongTin", selectedValue: IDChon);
            return result;
        }

        //Giải phóng dung lượng biến db, để ở cuối controller
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