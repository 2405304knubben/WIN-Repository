// Importeert benodigde namespaces voor data annotations en collections
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    /// <summary>
    /// Entiteit model voor klanten in het MatrixInc systeem.
    /// Representeert een klant met basisgegevens en gekoppelde bestellingen.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Unieke identifier voor de klant (Primary Key).
        /// Wordt automatisch toegewezen door de database.
        /// </summary>
        [Key]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Naam van de klant (verplicht veld).
        /// Wordt gebruikt voor weergave en identificatie.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Adres van de klant (verplicht veld).
        /// Bevat het volledige adres voor levering en facturatie.
        /// </summary>
        [Required]
        public string Address { get; set; }

        /// <summary>
        /// Geeft aan of de klant actief is in het systeem.
        /// Inactieve klanten kunnen geen nieuwe bestellingen plaatsen.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Navigatie eigenschap naar alle bestellingen van deze klant.
        /// Wordt automatisch geladen door Entity Framework bij gebruik.
        /// </summary>
        public ICollection<Order> Orders { get; } = new List<Order>();
    }
}