using BookingApi.Models;
namespace BookingApi.Interfaces
{
    public interface iCompletePurchaseOrder
    {
        public MPurchaseOrder FormData { get; set; }

        public List<MPurchaseOrderItem>? TableData{get;set; }
    }
}