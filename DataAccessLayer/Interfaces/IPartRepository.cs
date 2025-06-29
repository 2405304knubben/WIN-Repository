// Importeert data model en standaard namespaces
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    /// <summary>
    /// Interface voor onderdeel gerelateerde database operaties.
    /// Definieert de contract voor CRUD operaties op Part entiteiten.
    /// Implementeert het Repository pattern voor data abstractie.
    /// </summary>
    public interface IPartRepository
    {
        /// <summary>
        /// Haalt alle onderdelen op uit de database.
        /// </summary>
        /// <returns>Collectie van alle Part objecten</returns>
        public IEnumerable<Part> GetAllParts();

        /// <summary>
        /// Haalt een specifiek onderdeel op via het ID.
        /// </summary>
        /// <param name="id">Unieke identifier van het onderdeel</param>
        /// <returns>Part object of null als niet gevonden</returns>
        public Part? GetPartById(int id);

        /// <summary>
        /// Voegt een nieuw onderdeel toe aan de database.
        /// </summary>
        /// <param name="part">Het Part object om toe te voegen</param>
        public void AddPart(Part part);

        /// <summary>
        /// Werkt een bestaand onderdeel bij in de database.
        /// </summary>
        /// <param name="part">Het Part object met bijgewerkte gegevens</param>
        public void UpdatePart(Part part);

        /// <summary>
        /// Verwijdert een onderdeel uit de database.
        /// </summary>
        /// <param name="part">Het Part object om te verwijderen</param>
        public void DeletePart(Part part);
    }
}
