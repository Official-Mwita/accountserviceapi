using BookingApi.Interfaces;

namespace BookingApi.Models
{
    public class MPurchaseOrderItem: iPurchaseOrderItem
    {
        public string item { get; set; }
        public int quantity { get; set; }
        public double unitCost { get; set; }
        public double extendedCost { get; set; }
        public double taxAmount { get; set; }
        public double discountAmount { get; set; }
        public double lineTotal { get; set; }
    }
}
