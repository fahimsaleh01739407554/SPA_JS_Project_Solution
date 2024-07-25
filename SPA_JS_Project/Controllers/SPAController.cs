using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPA_JS_Project.Models;
using SPA_JS_Project.ViewModels;
using System.Runtime.InteropServices;

namespace SPA_JS_Project.Controllers
{
    public class SPAController : Controller
    {
        ProductDbContext db;
        IWebHostEnvironment env;
        public SPAController(ProductDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        public async Task<IActionResult> Index()
        {
            var id = 0;
            if (db.Orders.Any())
            {
                id = db.Orders.ToList()[0].OrderID;
            }

            DataViewModel data = new DataViewModel();
            data.SelectedOrderId = id;
            data.Customers = await db.Customers.ToListAsync();
            data.Products = await db.Products.ToListAsync();
            data.Orders = await db.Orders.ToListAsync();
            data.OrderItems = await db.OrderItems.Where(oi => oi.OrderID == id).ToListAsync();


            return View(data);
        }
        #region child actions
        public async Task<IActionResult> GetSelectedOrderItems(int id)
        {
            
            var OrderItems = await db.OrderItems.Include(x=> x.Product).Where(oi => oi.OrderID == id).ToListAsync();
            return PartialView("_OrderItemTable", OrderItems);
        }
        public IActionResult CreateCustomer()
        {
            return PartialView("_CreateCustomer");
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer c)
        {
            if (ModelState.IsValid)
            {
                await db.Customers.AddAsync(c);
                await db.SaveChangesAsync();
                return Json(c);
            }
            return BadRequest("Unexpected error");
        }
        public async Task<IActionResult> EditCustomer(int id)
        {
            var data = await db.Customers.FirstOrDefaultAsync(c => c.CustomerID == id);
            return PartialView("_EditCustomer", data);
        }
        [HttpPost]
        public async Task<IActionResult> EditCustomer(Customer c)
        {
            if (ModelState.IsValid)
            {
                db.Entry(c).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(c);
            }
            return BadRequest("Unexpected error");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if(!await db.Orders.AnyAsync(o=> o.CustomerId == id))
            {
                var o = new Customer { CustomerID = id };
                db.Entry(o).State = EntityState.Deleted;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = true, message = "Data deleted" });
            }
            return Json(new { success = false, message = "Cannot delete, item has related child." });
        }
        public IActionResult CreateProduct()
        {
            return PartialView("_CreateProduct");
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductInputModel p)
        {
            if (ModelState.IsValid)
            {
                var product = new Product { ProductName = p.ProductName, Price = p.Price };
                string fileName = Guid.NewGuid() + Path.GetExtension(p.Picture.FileName);
                string savePath = Path.Combine(this.env.WebRootPath, "Pictures", fileName);
                var fs = new FileStream(savePath, FileMode.Create);
                p.Picture.CopyTo(fs);
                fs.Close();
                product.Picture = fileName;
                await db.Products.AddAsync(product);
                await db.SaveChangesAsync();
                return Json(product);


            }
            return BadRequest("Falied to insert product");
        }
        public async Task<IActionResult> EditProduct(int id)
        {
            var data = await db.Products.FirstAsync(x => x.ProductID == id);
            ViewData["CurrentPic"] = data.Picture;
            return PartialView("_EditProduct", new ProductEditModel { ProductID=data.ProductID,ProductName=data.ProductName, Price=data.Price,   IsAvailable=data.IsAvailable});
        }
        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductEditModel p)
        {
            if (ModelState.IsValid)
            {
                var product = await db.Products.FirstAsync(x => x.ProductID == p.ProductID);
                product.ProductName = p.ProductName;
                product.Price = p.Price;
                product.IsAvailable = p.IsAvailable;
                if(p.Picture != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(p.Picture.FileName);
                    string savePath = Path.Combine(this.env.WebRootPath, "Pictures", fileName);
                    var fs = new FileStream(savePath, FileMode.Create);
                    p.Picture.CopyTo(fs);
                    fs.Close();
                    product.Picture = fileName;
                }
               
                
                await db.SaveChangesAsync();
                return Json(product);


            }
            return BadRequest();
        }
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!await db.OrderItems.AnyAsync(o => o.ProductID == id))
            {
                var o = new Product { ProductID = id };
                db.Entry(o).State = EntityState.Deleted;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = true, message = "Data deleted" });
            }
            return Json(new { success = false, message = "Cannot delete, item has related child." });
        }
        public async Task<IActionResult> CreateOrder()
        {
            ViewData["Products"] =await db.Products.ToListAsync();
            ViewData["Customers"] =await db.Customers.ToListAsync();
            return PartialView("_CreateOrder");
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order o, int[] ProductID, int[] Quantity)
        {
            if (ModelState.IsValid)
            {
                for(var i=0; i< ProductID.Length; i++)
                {
                    o.OrderItems.Add(new OrderItem { ProductID = ProductID[i], Quantity= Quantity[i] });
                }
                await db.Orders.AddAsync(o);
                
               await db.SaveChangesAsync();


                var ord = await GetOrder(o.OrderID);
                return Json(ord);
            }
            return BadRequest();
        }
        public async Task<IActionResult> EditOrder(int id)
        {
            ViewData["Products"] = await db.Products.ToListAsync();
            ViewData["Customers"] = await db.Customers.ToListAsync();
            var data = await db.Orders
                .Include(x => x.OrderItems).ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x=> x.OrderID == id);
            return PartialView("_EditOrder", data);
                
        }

        private async Task<Order?> GetOrder(int id)
        {
            var o = await db.Orders.Include(x=> x.Customer).FirstOrDefaultAsync(x => x.OrderID == id);
            return o;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var o = new Order { OrderID = id };
            db.Entry(o).State= EntityState.Deleted;
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Data deleted" });
        }
        public async Task<IActionResult> CreateItem()
        {
            ViewData["Products"] = await db.Products.ToListAsync();
            return PartialView("_CreateItem");
        }
        public async Task<IActionResult> CreateOrderItem(int id)
        {
            ViewData["OrderID"] = id;
            ViewData["Products"] = await db.Products.ToListAsync();
            return PartialView("_CreateOrderItem");
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrderItem(OrderItem oi)
        {
            if (ModelState.IsValid)
            {
                await db.OrderItems.AddAsync(oi);
                await db.SaveChangesAsync();
                var o = await GetOrderItem(oi.OrderID, oi.ProductID);
                return Json(o);
            }
            return BadRequest();
        }
        public async Task<IActionResult> EditOrderItem(int oid, int pid)
        {
            ViewData["Products"] = await db.Products.ToListAsync();
            var oi = await db.OrderItems.FirstAsync(x => x.OrderID == oid && x.ProductID == pid);
            return PartialView("_EdiOrderItem", oi);
        }
        [HttpPost]
        public async Task<IActionResult> EditOrderItem(OrderItem oi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(oi).State = EntityState.Modified;
                await db.SaveChangesAsync();
                var o = await GetOrderItem(oi.OrderID, oi.ProductID);
                return Json(o);
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteOrderItem([FromQuery]int oid, [FromQuery] int pid)
        {

                var o = new OrderItem { ProductID = pid, OrderID=oid};
                db.Entry(o).State = EntityState.Deleted;
                
               await db.SaveChangesAsync();
                               
                return Json(new { success = true, message = "Data deleted" });
           
        }
        private async Task<OrderItem> GetOrderItem(int oid, int pid)
        {
            var oi = await db.OrderItems
                .Include(o=> o.Order)
                .Include(o=> o.Product)
                .FirstAsync(x => x.OrderID == oid && x.ProductID == pid);
            return oi;
        }
        #endregion
    }
}
