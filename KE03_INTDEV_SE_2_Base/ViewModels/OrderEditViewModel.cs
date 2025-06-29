using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor het bewerken van bestaande bestellingen
    /// Bevat de benodigde gegevens voor de order edit view inclusief huidige orderproducten en beschikbare items
    /// </summary>
    public class OrderEditViewModel
    {
        /// <summary>
        /// Unieke identificatie van de bestelling
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Datum waarop de bestelling is geplaatst
        /// </summary>
        public DateTime OrderDate { get; set; }
        
        /// <summary>
        /// Lijst van producten die momenteel in de bestelling zitten
        /// Geïnitialiseerd als lege lijst om null reference exceptions te voorkomen
        /// </summary>
        public List<OrderProductEditViewModel> OrderProducts { get; set; } = new List<OrderProductEditViewModel>();
        
        /// <summary>
        /// Lijst van beschikbare items (producten/onderdelen) die toegevoegd kunnen worden aan de bestelling
        /// Gebruikt voor dropdown of selectie-interface in de view
        /// </summary>
        public List<OrderItemViewModel> AvailableItems { get; set; } = new List<OrderItemViewModel>();

        /// <summary>
        /// Factory method om een OrderEditViewModel te maken vanuit een Order entity
        /// Transformeert de database entiteit naar het view-specifieke model
        /// </summary>
        /// <param name="order">De Order entiteit uit de database</param>
        /// <returns>Een nieuwe OrderEditViewModel instance met de order gegevens</returns>
        public static OrderEditViewModel FromOrder(Order order)
        {
            return new OrderEditViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                // Transformeer OrderProducts naar edit-vriendelijke ViewModels
                OrderProducts = order.OrderProducts.Select(op => new OrderProductEditViewModel
                {
                    ProductId = op.Product.Id,
                    ProductName = op.Product.Name,
                    CurrentStock = op.Product.Stock,  // Nodig voor voorraad validatie
                    Quantity = op.Aantal,
                    Price = op.Product.Price
                }).ToList()
            };
        }
    }

    /// <summary>
    /// ViewModel voor het bewerken van individuele producten binnen een bestelling
    /// Bevat alle benodigde informatie voor het weergeven en valideren van product wijzigingen
    /// </summary>
    public class OrderProductEditViewModel
    {
        /// <summary>
        /// Unieke identificatie van het product
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// Naam van het product voor weergave
        /// Required om null values te voorkomen
        /// </summary>
        public required string ProductName { get; set; }
        
        /// <summary>
        /// Huidige voorraad van het product
        /// Gebruikt voor voorraad validatie bij het bewerken van hoeveelheden
        /// </summary>
        public int CurrentStock { get; set; }
        
        /// <summary>
        /// Bestelde hoeveelheid van dit product
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Prijs per stuk van het product
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Berekende totaalprijs voor dit product (hoeveelheid × prijs)
        /// Read-only property die automatisch wordt berekend
        /// </summary>
        public decimal TotalPrice => Quantity * Price;
    }
}
