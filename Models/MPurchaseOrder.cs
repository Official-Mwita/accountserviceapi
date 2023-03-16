using System;
using BookingApi.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace BookingApi.Models
{
    public class MPurchaseOrder: iPurchaseOrder
    {
        [Required]
        public string CostCenter { get; set; }
        [Required]
        public string ShipsTo { get; set; }
        [Required]
        public int OrderAmount { get; set; }
        [Required]
        public DateTime FirstDeliveryDate { get; set; }
        [Required]
        public string Narration { get; set; }
        [Required]
        public string Supplier { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public int DeliveryPeriod { get; set; }
        [Required]
        public string VehicleDetails { get; set; }
    }
}
