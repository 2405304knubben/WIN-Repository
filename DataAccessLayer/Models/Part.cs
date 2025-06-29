// Importeert benodigde namespaces voor collections
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    /// <summary>
    /// Entiteit model voor onderdelen (parts) in het MatrixInc systeem.
    /// Representeert een onderdeel dat gebruikt kan worden in meerdere producten.
    /// </summary>
    public class Part
    {
        /// <summary>
        /// Unieke identifier voor het onderdeel (Primary Key).
        /// Wordt automatisch toegewezen door de database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Naam van het onderdeel (verplicht veld).
        /// Wordt gebruikt voor identificatie en weergave.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Beschrijving van het onderdeel (optioneel).
        /// Bevat technische details en specificaties.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Many-to-many relatie met producten.
        /// Geeft aan in welke producten dit onderdeel wordt gebruikt.
        /// </summary>
        public ICollection<Product> Products { get; } = new List<Product>();

        /// <summary>
        /// Aantal stuks van dit onderdeel in voorraad.
        /// Gebruikt voor voorraad beheer en productie planning.
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Prijs van het onderdeel in decimalen.
        /// Gebruikt voor kostencalculatie van producten.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Afbeelding van het onderdeel opgeslagen als byte array (optioneel).
        /// Null indien geen afbeelding beschikbaar is.
        /// </summary>
        public byte[]? Image { get; set; }
    }
}
