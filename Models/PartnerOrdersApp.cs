using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stroika.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string PartnerType { get; set; }
        public string PartnerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int Rating { get; set; }
        public decimal TotalCost { get; set; }

        public string FullAddress => $"Юридический адрес: {Address}";
        public string PhoneDisplay => $"+7 {Phone}";
        public string RatingDisplay => $"Рейтинг: {Rating}";
    }
    public class PartnerShortModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal MinPartnerPrice { get; set; } 
    }


    public class PartnerEditModel
    {
        public int Id { get; set; }
        public int PartnerTypeId { get; set; }
        public string CompanyName { get; set; }
        public string Inn { get; set; }
        public string DirectorSurname { get; set; }
        public string DirectorName { get; set; }
        public string DirectorPatronymic { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Building { get; set; }
    }

    public class ProductItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal MinPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Quantity * MinPrice;
    }
    public class OrderItemModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Quantity * Price; 
    }
}
