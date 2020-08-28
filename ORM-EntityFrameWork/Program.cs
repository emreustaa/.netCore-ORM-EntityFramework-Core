using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ORM_EntityFrameWork.Data.EfCore;

namespace ORM_EntityFrameWork
{

    public class ShopContext : DbContext

    {

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public static readonly ILoggerFactory MyLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseLoggerFactory(MyLoggerFactory)
            // .UseSqlite("Data Source=shop.db");
            .UseMySql(@"server=localhost;port=3306;database=ShopDb;user=root;password=mysqlserver");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Product>().ToTable("Urunler");
            modelBuilder.Entity<ProductCategory>()
            .HasKey(t => new { t.ProductId, t.CategoryId });

            modelBuilder.Entity<Customer>()
            .Property(p => p.IdentityNumber).HasMaxLength(11).IsRequired();

            modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId);


            modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId);
        }
    }


    // conventions
    // data anotations
    // fluent api

    public static class DataSeeding
    {

        public static void Seed(DbContext context)


        {

            if (context.Database.GetPendingMigrations().Count() == 0)
            {

                if (context is ShopContext)
                {

                    ShopContext _context = context as ShopContext;

                    if (_context.Products.Count() == 0)
                    {

                        _context.Products.AddRange(Products);
                        //product ekle
                    }
                    if (_context.Categories.Count() == 0)
                    {
                        _context.Categories.AddRange(Categories);

                        // category ekles

                    }
                }
                context.SaveChanges();

            }

        }

        private static Product[] Products = {

                new Product(){Name="Samsung S6",Price = 2000},
                new Product(){Name="Samsung S7",Price = 3000},
                new Product(){Name="Samsung S8",Price = 4000},
                new Product(){Name="Samsung S9",Price = 5000},

        };

        private static Category[] Categories = {

                new Category(){Name="Telefon"},
                new Category(){Name="Bilgisayar"},
                new Category(){Name="Tablet"},
                new Category(){Name="Televizyon"},
        };


    }
    public class User
    {

        public int Id { get; set; }

        [Required]
        [MinLength(8), MaxLength(15)]

        public string Username { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Email { get; set; }

        public Customer Customer { get; set; }

        public List<Address> Addresses { get; set; } //navigation property

    }

    public class Customer
    {

        [Column("customer_id")]
        public int Id { get; set; }

        [Required]
        public string IdentityNumber { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName { get; set; }

        public User User { get; set; }
        public int UserId { get; set; }

    }

    public class Supplier
    {


        public int Id { get; set; }
        public string Name { get; set; }
        public string TaxNumber { get; set; }
    }


    public class Address
    {
        public int Id { get; set; }
        public string Full { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public User User { get; set; } //navigation property
        public int UserId { get; set; } // int => 0

    }
    public class Product
    {
        // primary key (Id,<type_name>Id)

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public DateTime InsertedDate { get; set; } = DateTime.Now;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;

        public List<ProductCategory> ProductCategories { get; set; }


    }

    public class Category
    {

        public int Id { get; set; }

        public string Name { get; set; }

        //[NotMapped]

        public List<ProductCategory> ProductCategories { get; set; }

    }
    [Table("Urun Kategorileri")]
    public class ProductCategory
    {

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }


    public class CustomerDemo
    {

        public CustomerDemo()
        {
            Orders = new List<OrderDemo>();

        }
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public int OrderCount { get; set; }

        public List<OrderDemo> Orders { get; set; }
    }

    public class OrderDemo
    {

        public int OrderId { get; set; }
        public decimal Total { get; set; }

        public List<ProductDemo> Products { get; set; }
    }
    public class ProductDemo
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

    }
    class Program
    {
        static void Main(string[] args)
        {

            using (var db = new CustomNorthwindContext())
            {

                var customers = db.CustomerOrders
                .FromSqlRaw("select c.id,c.first_name,count(*) as count from customers c inner join orders o on c.id=o.customer_id group by c.id")
                .ToList();

                foreach (var item in customers)
                {
                    Console.WriteLine($" Customer Id: {item.CustomerId} First Name: {item.FirstName} Order Count: {item.OrderCount}");

                }

            }


            /*
                        using (var db = new Data.EfCore.NorthwindContext())

                        {
                            var city = "Miami";

                            var customers = db.Customers.FromSqlRaw("select * from customers where city={0}",city).ToList();

                            foreach (var item in customers)
                            {
                                Console.WriteLine(item.FirstName);
                            }



                            /* ÇOKLU TABLO KISMI
                                            var customers = db.Customers
                                            .Where(i => i.Id == 3)
                                            .Select(i => new CustomerDemo
                                            {

                                                CustomerId = i.Id,
                                                Name = i.FirstName,
                                                OrderCount = i.Orders.Count(),
                                                Orders = i.Orders.Select(a => new OrderDemo
                                                {
                                                    OrderId = a.Id,
                                                    Total = (decimal)a.OrderDetails.Sum(Od => Od.Quantity * Od.UnitPrice),
                                                    Products = a.OrderDetails.Select(p => new ProductDemo
                                                    {
                                                        ProductId = (int)p.ProductId,
                                                        Name = p.Product.ProductName
                                                    }).ToList()

                                                }).ToList()
                                            })
                                            .OrderBy(a => a.OrderCount)
                                            .ToList();

                                            foreach (var customer in customers)
                                            {
                                                Console.WriteLine($"Müşteri id : {customer.CustomerId} Müşteri adı : {customer.Name} Sipariş sayısı : {customer.OrderCount}");

                                                foreach (var order in customer.Orders)
                                                {

                                                    Console.WriteLine($" order id : {order.OrderId} total : {order.Total}");

                                                    foreach (var product in order.Products)
                                                    {
                                                        Console.WriteLine($" product id : {product.ProductId} name : {product.Name}");
                                                    }

                                                }
                                            }









                            // tüm müşteri kayıtlarını getiriniz.

                            // var customers = db.Customers.ToList();

                            // foreach (var item in customers)
                            // {

                            //     Console.WriteLine(item.FirstName + " " + item.LastName);

                            // }

                            // müşteri kayıtlarının sadece first name ve last name alanlarını getiriniz.
                            // var customers = db.Customers.Select(c => new
                            // {
                            //     c.FirstName,
                            //     c.LastName

                            // });

                            // foreach (var item in customers)
                            // {

                            //     Console.WriteLine(item.FirstName + " " + item.LastName);

                            // }

                            // şehirleri new yok olan müşterileri isim sırasına göre getir
                            // var customers = db.Customers
                            //                 .Where(i => i.City == "New York")
                            //                 .Select(s => new { s.FirstName, s.LastName })
                            //                 .OrderBy(i => i.FirstName)
                            //                 .ToList();


                            // foreach (var item in customers)
                            // {

                            //     Console.WriteLine(item.FirstName + " " + item.LastName);

                            // }

                            // Beverages kategorisine ait ürünlerin isimlerini getiriniz

                            // var productNames = db.Products
                            // .Where(c => c.Category == "Beverages")
                            // .Select(s => s.ProductName).ToList();


                            // foreach (var name in productNames)
                            // {

                            //     Console.WriteLine(name);

                            // }

                            // var products = db.Products.OrderByDescending(i => i.Id).Take(5);

                            // foreach (var item in products)
                            // {
                            //     Console.WriteLine(item.ProductName);

                            // }

                            // fiyatı 10 ile 30 arasındaki ürünler

                            // var products = db.Products
                            // .Where(f => 10 < f.ListPrice && f.ListPrice < 30)
                            // .Select(s => new { s.ProductName, s.ListPrice })
                            // .ToList();

                            // foreach (var p in products)
                            // {
                            //     Console.WriteLine(p.ProductName + " " + p.ListPrice);

                            // }

                            //Beverages kategorisindeki ürünlerin ortalama fiyatları
                            // var ortalama = db.Products
                            // .Where(c => c.Category == "Beverages")
                            // .Average(i => i.ListPrice);

                            // Console.WriteLine("Ortalama : " + ortalama);

                            //beverages kategorisindeki toplam ürün miktarı
                            // var adet = db.Products.Count(i => i.Category == "Beverages");

                            // Console.WriteLine("Beverages kategorisindeki toplam ürünler :{0} ", adet);

                            // beverages ve condiments kategorilerimdeki ürünlerin toplam fiyatı

                            // var toplam = db.Products
                            // .Where(c => c.Category == "Beverages" || c.Category == "Condiments")
                            // .Sum(i => i.ListPrice);

                            // Console.WriteLine("Toplam : {0}", toplam);


                            //tea kelimesini içeren ürünleri getiriniz


                            // var products = db.Products.Where(i => i.ProductName.Contains("Tea")).ToList();

                            // foreach (var item in products)
                            // {

                            //     Console.WriteLine(item.ProductName);

                            // }

                            //en pahalı ve en ucuz ürün hangisidir?

                            // var minPrice = db.Products.Select(s => new { s.ProductName, s.ListPrice }).Min(i => i.ListPrice);
                            // var maxPrice = db.Products.Select(s => new { s.ProductName, s.ListPrice }).Max(i => i.ListPrice);

                            // var p = db.Products.Where(i => i.ListPrice == (db.Products.Min(a => a.ListPrice))).FirstOrDefault();
                            // var pMax = db.Products.Where(i => i.ListPrice == (db.Products.Max(a => a.ListPrice))).FirstOrDefault();

                            // Console.WriteLine($"Minimum fiyatlı ürün : {p.ProductName} fiyatı : {p.ListPrice}");
                            // Console.WriteLine($"Maximum fiyatlı ürün : {pMax.ProductName} fiyatı : {pMax.ListPrice}");


                        }*/









            //DataSeeding.Seed(new ShopContext());
            /*
                        using (var db = new Data.EfCore.NorthwindContext())
                        {

                            var products = db.Products.ToList();

                            foreach (var item in products)
                            {

                                Console.WriteLine(item.ProductName);
                            }

                        }*/
            /*
            using (var db = new ShopContext())

            {
                
                var p = new Product
                {
                     Name = "Samsung S6",
                     Price = 2000
                };

                var p = db.Products.FirstOrDefault();
                p.Name = "Samsung Galaxy S10";
                //db.Products.Add(p);
                db.SaveChanges();

                
                                var user = new User()
                                {

                                    Username = "deneme",
                                    Email = "deneme@gmail.com",
                                    Customer = new Customer()
                                    {

                                        FirstName = "Deneme",
                                        LastName = "Deneme",
                                        IdentityNumber = "13213132"
                                    }
                                };

                                db.Users.Add(user);
                                db.SaveChanges();
                // var customer = new Customer()
                // {

                //     IdentityNumber = "123456",
                //     FirstName = "Arda",
                //     LastName = "Usta",
                //     User = db.Users.FirstOrDefault(i => i.Id == 3)
                // };

                // db.Customers.Add(customer);
                // db.SaveChanges();

            }*/

            /*
            using (var db = new ShopContext())
            {

                var user = db.Users.FirstOrDefault(i => i.Username == "emre");

                if (user != null)
                {
                    user.Addresses = new List<Address>();

                    user.Addresses.AddRange(
                        new List<Address>(){

                        new Address() { Full = "Emre", Title = "İş Adresi 1", Body = "İstanbul" },
                        new Address() { Full = "Emre", Title = "İş Adresi 2", Body = "İstanbul" },
                        new Address() { Full = "Emre", Title = "İş Adresi 3", Body = "İstanbul" }
                        }
                    );
                    db.SaveChanges();
                }

            }*/

        }

        static void InsertUsers()
        {

            var users = new List<User>(){
                new User(){Username="emre",Email="emreustaa34@gmail.com"},
                new User(){Username="arda",Email="arda@gmail.com"},
                new User(){Username="tugfe",Email="tugfe@gmail.com"},
                new User(){Username="test",Email="test@gmail.com"},
                new User(){Username="deneme",Email="deneme@gmail.com"}
            };

            using (var db = new ShopContext())
            {

                db.Users.AddRange(users);
                db.SaveChanges();

            }
        }


        static void InsertAddresses()
        {

            var addresses = new List<Address>(){

                new Address(){Full = "Tuğfe",Title="İş Adresi",Body="İstanbul",UserId=2},
                new Address(){Full = "Arda",Title="Ev Adresi",Body="İstanbul",UserId=3},
                new Address(){Full = "Hümeyra",Title="Ev Adresi",Body="İstanbul",UserId=2},
                new Address(){Full = "Test",Title="Ev Adresi",Body="İstanbul",UserId=5},
                new Address(){Full = "D>eneme",Title="Ev Adresi",Body="İstanbul",UserId=1},
                new Address(){Full = "Örnek",Title="Ev Adresi",Body="İstanbul",UserId=4},

            };

            using (var db = new ShopContext())
            {

                db.Addresses.AddRange(addresses);
                db.SaveChanges();

            }
        }
        static void DeleteProduct(int id)
        {

            using (var db = new ShopContext())
            {

                var p = new Product() { ProductId = 5 };
                db.Entry(p).State = EntityState.Deleted;
                db.SaveChanges();
                Console.WriteLine("Veri entry ile silindi..!");

                // var p = db.Products.FirstOrDefault(i => i.ProductId == id);

                // if (p != null)
                // {

                //     db.Products.Remove(p);
                //     db.SaveChanges();
                //     Console.WriteLine("Veri silindi..!");


                // }

            }
        }
        static void UpdateProduct()
        {
            using (var db = new ShopContext())
            {

                var p = db.Products
                .Where(i => i.ProductId == 1)
                .FirstOrDefault();

                if (p != null)
                {
                    p.Price = 2400;
                    db.Products.Update(p);
                    db.SaveChanges();

                }

            }
            // using (var db = new ShopContext())
            // {

            //         var entity = new Product(){ProductId=1};
            //         db.Products.Attach(entity);
            //         entity.Price = 3000;
            //         db.SaveChanges();
            // }


            // using (var db = new ShopContext())
            // {
            // change tracking obje üzerinde değişiklik olduğunda tespiti yapıaln kavram
            //     var p = db.Products.Where(i => i.ProductId == 1).FirstOrDefault();

            //     if (p != null)
            //     {

            //         p.Price *= 1.2m;

            //         db.SaveChanges();
            //         Console.WriteLine("Güncelleme yapıldı..!");



            //     }
            // }


        }
        static void GetProductByName(String name)
        {

            using (var context = new ShopContext())
            {
                var products = context
                .Products
                .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                .Select(p => new
                {

                    p.Name,
                    p.Price

                })
                .ToList();

                foreach (var p in products)
                {
                    Console.WriteLine($"name :  { p.Name} price : {p.Price}");

                }



            }
        }

        static void GetProductById(int id)
        {

            using (var context = new ShopContext())
            {
                var result = context
                .Products
                .Where(p => p.ProductId == id)
                .Select(p => new
                {

                    p.Name,
                    p.Price

                })
                .FirstOrDefault();

                Console.WriteLine($"name :  { result.Name} price : {result.Price}");

            }
        }

        static void GetAllProducts()
        {

            using (var context = new ShopContext())
            {
                var products = context
                .Products
                .Select(p => new
                {

                    p.Name,
                    p.Price

                })
                .ToList();

                foreach (var p in products)
                {
                    Console.WriteLine($"name :  { p.Name} price : {p.Price}");

                }



            }
        }
        static void AddProducts()
        {

            using (var db = new ShopContext())
            {

                var products = new List<Product>()
                {

                    new Product { Name = "Samsun S6", Price = 3000 },
                    new Product { Name = "Samsun S7", Price = 4000 },
                    new Product { Name = "Samsun S8", Price = 5000 },
                    new Product { Name = "Samsun S9", Price =  6000}
                };


                db.AddRange(products);
                db.SaveChanges();

                Console.WriteLine("Veriler eklendi..!");
            }
        }

        static void AddProduct()
        {

            using (var db = new ShopContext())
            {

                var p = new Product { Name = "Samsung S10", Price = 9000 };


                db.Products.Add(p);
                db.SaveChanges();

                Console.WriteLine("Veriler eklendi..!");
            }
        }
    }
}
