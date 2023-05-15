using System.Collections.Generic;
using BookingApi.Interfaces;

namespace BookingApi.Models
{
    public class CompletePurchaseOrder: iCompletePurchaseOrder
    {
        public MPurchaseOrder FormData { get; set; } = new MPurchaseOrder();
        public List<MPurchaseOrderItem>? TableData{get;set;}

    }
}