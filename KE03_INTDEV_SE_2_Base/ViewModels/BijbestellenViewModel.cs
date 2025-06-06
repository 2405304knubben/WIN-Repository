namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class BijbestellenViewModel
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int OrderAmount { get; set; }
    }
}
