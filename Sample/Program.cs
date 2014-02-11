using System;
using System.Data.Entity;
using System.Linq;
using DynamicLINQ;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            NorthwindContext northwind = new NorthwindContext();

            var query = northwind.Products.DynamicWhere(p => Filter(p))
                                          .DynamicWhere(p => p["UnitsInStock"] < 20)
                                          .DynamicWhere(p => 40 > p["UnitsInStock"])
                                          .DynamicWhere(p => 10 != p["UnitsInStock"])
                                          .DynamicWhere(p => 4 == p["UnitsInStock"])
                                          .DynamicWhere(p => p["UnitsInStock"] == 10)
                                          .DynamicWhere(p => p["UnitsInStock"] != 4)
                                          .DynamicOrderBy(p => p["Discontinued"])
                                          .DynamicThenByDescending(p => p["UnitsInStock"])
                                          .Select(p => new
                                          {
                                              p.ProductName,
                                              p.Discontinued,
                                              p.UnitsInStock
                                          });

            // Expression is translated here.
            Console.WriteLine(query);

            foreach (var p in query)
            {
                Console.WriteLine(p);
            }
        }

        private static dynamic Filter(dynamic d)
        {
            return !d.Discontinued;
        }
    }

    public class NorthwindContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public NorthwindContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<NorthwindContext>());
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int UnitsInStock { get; set; }
        public bool Discontinued { get; set; }
    }
}
