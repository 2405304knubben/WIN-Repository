// Importeert benodigde namespaces voor data annotations
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    /// <summary>
    /// Koppeltabel entiteit tussen Order en Product (Many-to-Many relatie).
    /// Bevat welke producten in welke bestellingen zitten en in welke hoeveelheid.
    /// </summary>
    public class OrderProduct
    {
        /// <summary>
        /// Foreign key naar de Order tabel.
        /// Onderdeel van de samengestelde primary key.
        /// </summary>
        public int OrdersId { get; set; }
        
        /// <summary>
        /// Foreign key naar de Product tabel.
        /// Onderdeel van de samengestelde primary key.
        /// </summary>
        public int ProductsId { get; set; }
        
        /// <summary>
        /// Het aantal stuks van dit product in deze bestelling.
        /// Gebruikt voor prijs berekening en voorraad tracking.
        /// </summary>
        public int Aantal { get; set; }

        /// <summary>
        /// Navigatie eigenschap naar de gekoppelde bestelling.
        /// Wordt automatisch geladen door Entity Framework bij gebruik.
        /// </summary>
        public Order Order { get; set; } = null!;
        
        /// <summary>
        /// Navigatie eigenschap naar het gekoppelde product.
        /// Wordt automatisch geladen door Entity Framework bij gebruik.
        /// </summary>
        public Product Product { get; set; } = null!;
    }
}
