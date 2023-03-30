using System;
using BookingApi.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace BookingApi.Models
{
    public class MPurchaseOrder: iPurchaseOrder
    {
        [Required]
        public string CostCenter { get; set; } = string.Empty;
        [Required]
        public string ShipsTo { get; set; } = string.Empty;
        [Required]
        public int OrderAmount { get; set; }
        [Required]
        public DateTime FirstDeliveryDate { get; set; }
        [Required]
        public string Narration { get; set; } = string.Empty;
        [Required]
        public string Supplier { get; set; } = string.Empty;
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public int DeliveryPeriod { get; set; }
        [Required]
        public string VehicleDetails { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public string id{get; set;} = string.Empty;
    }
}
