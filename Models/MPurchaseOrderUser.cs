using BookingApi.Interfaces;

namespace BookingApi.Models
{
    public class MPurchaseOrderUser: iPurchaseOrderUser
    {
        public string userid {get; set;} = string.Empty;
    }
}