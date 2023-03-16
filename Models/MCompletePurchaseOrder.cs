using System.Collections.Generic;

namespace BookingApi.Models
{
    public class CompletePurchaseOrder
    {
        public MPurchaseOrder FormData{ get; set; }
        //public List<MPurchaseOrderItem> purchaseOrderItems;

        public List<MPurchaseOrderItem> TableData{get;set;}

    }
}