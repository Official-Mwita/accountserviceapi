using System;
using System.Collections.Generic;

namespace BookingApi.Interfaces
{
    public interface iPurchaseOrder
    {
        public string CostCenter { get; set; }
        public string ShipsTo { get; set; }
        public int OrderAmount { get; set; }
        public DateTime FirstDeliveryDate { get; set; }
        public string Narration { get; set; }
        public string Supplier { get; set; }
        public DateTime OrderDate { get; set; }
        public int DeliveryPeriod { get; set; }
    }
}
