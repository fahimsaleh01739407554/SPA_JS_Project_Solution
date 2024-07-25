using SPA_JS_Project.Models;

namespace SPA_JS_Project.ViewModels
{
    public class DataViewModel
    {

        public int SelectedOrderId { get; set; }
        public IEnumerable<Customer> Customers { get; set; } = default!;
        public IEnumerable<Product> Products { get; set; } = default!;
        public IEnumerable<Order> Orders { get; set; } = default!;
        public IEnumerable<OrderItem> OrderItems { get; set; } = default!;

    }
}
