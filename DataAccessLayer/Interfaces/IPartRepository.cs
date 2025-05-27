using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IPartRepository
    {
        public IEnumerable<Part> GetAllParts();

        public Part? GetPartById(int id);

        public void AddPart(Part part);

        public void UpdatePart(Part part);

        public void DeletePart(Part part);
    }
}
