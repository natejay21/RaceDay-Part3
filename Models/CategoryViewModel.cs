namespace RaceDay.MVC.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal DistanceKM { get; set; }
        public decimal EntryFee { get; set; }
        public int EventId { get; set; }
    }
}