using System.ComponentModel.DataAnnotations;

namespace PurchaseApp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? PurchaseRequestId { get; set; }
        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;
        [Display(Name = "Performed By")]
        public string PerformedBy { get; set; } = "System/User";
        [Display(Name = "Details")]
        public string Details { get; set; } = string.Empty;
        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
