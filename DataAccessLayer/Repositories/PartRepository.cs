// Importeert interfaces, models en standaard namespaces
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// Implementatie van IPartRepository voor onderdeel gerelateerde database operaties.
    /// Biedt volledige CRUD functionaliteit voor Part entiteiten.
    /// Gebruikt Entity Framework Core voor data toegang.
    /// </summary>
    public class PartRepository : IPartRepository
    {
        // Database context voor data toegang
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor PartRepository. Injecteert de database context.
        /// </summary>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public PartRepository(MatrixIncDbContext context) 
        {
            _context = context; 
        }   

        /// <summary>
        /// Voegt een nieuw onderdeel toe aan de database en slaat wijzigingen direct op.
        /// </summary>
        /// <param name="part">Het Part object om toe te voegen</param>
        public void AddPart(Part part)
        {
            _context.Parts.Add(part);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Verwijdert een onderdeel uit de database en slaat wijzigingen direct op.
        /// Let op: Dit kan foreign key constraints veroorzaken als onderdeel gekoppeld is aan producten.
        /// </summary>
        /// <param name="part">Het Part object om te verwijderen</param>
        public void DeletePart(Part part)
        {
            _context.Parts.Remove(part);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Haalt alle onderdelen op uit de database.
        /// Eenvoudige query zonder eager loading van gerelateerde entiteiten.
        /// </summary>
        /// <returns>Collectie van alle Part objecten</returns>
        public IEnumerable<Part> GetAllParts()
        {
            return _context.Parts;            
        }

        /// <summary>
        /// Haalt een specifiek onderdeel op via ID.
        /// </summary>
        /// <param name="id">Unieke identifier van het onderdeel</param>
        /// <returns>Part object of null als niet gevonden</returns>
        public Part? GetPartById(int id)
        {
            return _context.Parts.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Werkt een bestaand onderdeel bij in de database en slaat wijzigingen direct op.
        /// Entity Framework tracking zorgt voor automatische change detection.
        /// </summary>
        /// <param name="part">Het Part object met bijgewerkte gegevens</param>
        public void UpdatePart(Part part)
        {
            _context.Parts.Update(part);
            _context.SaveChanges(); // Direct opslaan in database
        }
    }
}
