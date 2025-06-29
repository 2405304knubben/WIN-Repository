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
    /// Entiteit model voor producten in het MatrixInc systeem.
    /// Representeert een product met alle eigenschappen, voorraad en relaties.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unieke identifier voor het product (Primary Key).
        /// Wordt automatisch toegewezen door de database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Naam van het product (verplicht veld).
        /// Wordt gebruikt voor weergave en zoekfunctionaliteit.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Beschrijving van het product (optioneel).
        /// Bevat gedetailleerde informatie over het product.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Prijs van het product in decimalen (verplicht).
        /// Moet groter zijn dan 0 volgens de validatie regel.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Prijs moet groter zijn dan 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Aantal stuks van dit product in voorraad.
        /// Wordt gebruikt voor voorraad beheer en beschikbaarheid.
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Afbeelding van het product opgeslagen als byte array (optioneel).
        /// Null indien geen afbeelding beschikbaar is.
        /// </summary>
        public byte[]? Image { get; set; }

        /// <summary>
        /// Koppeltabel records die aangeven in welke bestellingen dit product zit.
        /// Gebruikt voor order tracking en verkoop statistieken.
        /// </summary>
        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

        /// <summary>
        /// Many-to-many relatie met onderdelen (Parts).
        /// Geeft aan welke onderdelen gebruikt worden om dit product te maken.
        /// </summary>
        public ICollection<Part> Parts { get; } = new List<Part>();
    }
}
