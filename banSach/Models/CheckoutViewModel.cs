namespace banSach.Models
{

	using System.Collections.Generic;
	using banSach.Models;

	public class CheckoutViewModel
	{
		public string HoTen { get; set; }
		public string SoDienThoai { get; set; }
		public string Email { get; set; }
		public string DiaChi { get; set; }
		public List<ChiTietGioHang> CartItems { get; set; }
	}
}