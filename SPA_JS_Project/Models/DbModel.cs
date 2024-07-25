using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SPA_JS_Project.Models
{
    public enum Status { Pending=1, Delivered, Cancelled}
    public class Customer
    {
        public int CustomerID { get; set; }
        [Required, StringLength(50), Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = default!;
        [Required, StringLength(50)]
        public string Address { get; set; } = default!;
        [Required, StringLength(50)]
        public string Email { get; set; } = default!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
    public class Order
    {
        public int OrderID { get; set; }
        [Required, Column(TypeName = "date"),
            Display(Name = "Order Date"),
            DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}",
            ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }
        [Column(TypeName = "date"),
            Display(Name = "Delivery Date"),
            DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}",
            ApplyFormatInEditMode = true)]
        public DateTime? DeliveryDate { get; set; }
        [Required, EnumDataType(typeof(Status))]
        public Status Status { get; set; }
        [Required, ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; } = default!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
    public class OrderItem
    {
        [ForeignKey("Order")]
        public int OrderID { get; set; }
        [ForeignKey("Product")]
        public int ProductID { get; set; }
        [Required]
        public int Quantity { get; set; }
        
        public virtual Order? Order { get; set; } = default!;
        public virtual Product? Product { get; set; } = default!;


    }
    public class Product
    {
        public int ProductID { get; set; }
        [Required, StringLength(50), Display(Name = "Product Name")]
        public string ProductName { get; set; } = default!;
        [Required, Column(TypeName = "money"), DisplayFormat(DataFormatString = "{0:0.00}")]
        public decimal Price { get; set; }
        [Required, StringLength(150)]
        public string Picture { get; set; } = default!;
        public bool IsAvailable { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> OrderItems { get; set; } = default!;
        public DbSet<Product> Products { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>().HasKey(o => new { o.OrderID, o.ProductID });
        }
    }
}
