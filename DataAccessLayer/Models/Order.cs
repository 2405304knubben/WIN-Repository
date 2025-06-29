// Importeert benodigde namespaces voor data annotations, collections en LINQ
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    /// <summary>
    /// Entiteit model voor bestellingen in het MatrixInc systeem.
    /// Representeert een bestelling met datum, klant en gekoppelde producten.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unieke identifier voor de bestelling (Primary Key).
        /// Wordt automatisch toegewezen door de database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Datum en tijd waarop de bestelling is geplaatst.
        /// Wordt gebruikt voor tracking en rapportage.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Foreign key naar de klant die deze bestelling heeft geplaatst.
        /// Verwijst naar Customer.Id.
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// Navigatie eigenschap naar de klant van deze bestelling.
        /// Wordt automatisch geladen door Entity Framework bij gebruik.
        /// </summary>
        public Customer Customer { get; set; } = null!;

        /// <summary>
        /// Koppeltabel records die de producten in deze bestelling bevatten.
        /// Elk record bevat ook de hoeveelheid van het specifieke product.
        /// </summary>
        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

        /// <summary>
        /// Berekende eigenschap die de totale prijs van de bestelling berekent.
        /// Sommeert alle producten (prijs × aantal) in de bestelling.
        /// </summary>
        public decimal TotalPrice => OrderProducts.Sum(op => op.Product.Price * op.Aantal);
    }
}
