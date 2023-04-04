using BookingApi.Interfaces;

namespace BookingApi.Models
{
    public class MPurchaseOrderItem: iPurchaseOrderItem
    {
        public string item { get; set; } = string.Empty;
        public int quantity { get; set; }
        public double unitCost { get; set; }
        public double extendedCost { get; set; }
        public double taxAmount { get; set; }
        public double discountAmount { get; set; }
        public double lineTotal { get; set; }
        public int partitionKey { get; set; }
        public string createdBy { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
    }
}
