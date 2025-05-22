using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class MatrixIncDbInitializer
    {
        public static void Initialize(MatrixIncDbContext context)
        {

            // Look for any customers.
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            // TODO: Hier moet ik nog wat namen verzinnen die betrekking hebben op de matrix.
            // - Denk aan de m3 boutjes, moertjes en ringetjes.
            // - Denk aan namen van schepen
            // - Denk aan namen van vliegtuigen            
            var customers = new Customer[]
            {
                new Customer { Name = "Neo", Address = "123 Elm St" , Active= true },
                new Customer { Name = "Morpheus", Address = "456 Oak St", Active = true },
                new Customer { Name = "Trinity", Address = "789 Pine St", Active = true }
            };
            context.Customers.AddRange(customers);

            var orders = new Order[]
            {
                //new Order { Customer = customers[0], OrderDate = DateTime.Parse("2021-01-01")},
                //new Order { Customer = customers[0], OrderDate = DateTime.Parse("2021-02-01")},
                //new Order { Customer = customers[1], OrderDate = DateTime.Parse("2021-02-01")},
                //new Order { Customer = customers[2], OrderDate = DateTime.Parse("2021-03-01")}
            };  
            context.Orders.AddRange(orders);

            var products = new Product[]
            {
                new Product { Id = 1, Name = "Nebuchadnezzar", Description = "Het schip waarop Neo voor het eerst de echte wereld leert kennen", Price = 10000.00f },
                new Product { Id = 2, Name = "Jack-in Chair", Description = "Stoel met een rugsteun en metalen armen waarin mensen zitten om ingeplugd te worden in de Matrix via een kabel in de nekpoort", Price = 500.50f },
                new Product { Id = 3, Name = "EMP (Electro-Magnetic Pulse) Device", Description = "Wapentuig op de schepen van Zion", Price = 129.99f },
                new Product { Id = 4, Name = "Sentinel", Description = "De robots die de mensen in de Matrix aanvallen", Price = 999.99f },
                new Product { Id = 5, Name = "Matrix Code", Description = "De groene code die de Matrix vormt", Price = 0.01f },
                new Product { Id = 6, Name = "Red Pill", Description = "De pil die Neo neemt om de Matrix te verlaten", Price = 0.50f },
                new Product { Id = 7, Name = "Blue Pill", Description = "De pil die Neo neemt om in de Matrix te blijven", Price = 0.50f },
                new Product { Id = 8, Name = "Tandwiel", Description = "Overdracht van rotatie in bijvoorbeeld de motor of luikmechanismen", Price = 1.60f},
                new Product { Id = 9, Name = "M5 Boutje", Description = "Bevestiging van panelen, buizen of interne modules", Price = 0.80f},
                new Product { Id = 10, Name = "Hydraulische cilinder", Description = "Openen/sluiten van zware luchtsluizen of bewegende onderdelen", Price = 5.99f},
                new Product { Id = 11, Name = "Koelvloeistofpomp", Description = "Koeling van de motor of elektronische systemen.", Price = 4.99f},
                new Product { Id = 12, Name = "M3 Moertje", Description = "Bevestiging van panelen, buizen of interne modules.", Price = 1.75f},
                new Product { Id = 13, Name = "M3 Ringetje", Description = "Bevestiging van panelen, buizen of interne modules.", Price = 0.85f},
                new Product { Id = 14, Name = "12V Batterij", Description = "12V Batterij dat wordt gebruikt om stroom te leveren.", Price = 1.50f},
                new Product { Id = 15, Name = "Zonnebril", Description = "Zonnebril dat wordt gebruikt om de Matrix te kunnen zien.", Price = 2.60f}
            };
            context.Products.AddRange(products);

            var parts = new Part[]
            {
                
            };
            context.Parts.AddRange(parts);

            context.SaveChanges();
        }
    }
}
