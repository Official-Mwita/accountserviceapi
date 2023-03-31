using BookingApi.Interfaces;

namespace BookingApi.Models
{
    public class MPurchaseOrder: iPurchaseOrder
    {
        public string CostCenter { get; set; } = string.Empty;
        public string ShipsTo { get; set; } = string.Empty;
        public int OrderAmount { get; set; }
        public DateTime FirstDeliveryDate { get; set; }
        public string Narration { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int DeliveryPeriod { get; set; }
        public string VehicleDetails { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public string id {get; set;} = string.Empty;
    }
}
