namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class VoorraadItemViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int CurrentStock { get; set; }
        public int OrderAmount { get; set; } = 1;
        public required string Type { get; set; }  // Add this property to distinguish between Product/Part
        public decimal Price { get; set; }
        public byte[]? Image { get; set; }
    }
}
