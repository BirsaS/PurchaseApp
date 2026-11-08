using System.ComponentModel.DataAnnotations;

namespace PurchaseApp.Models
{
    public class PurchaseRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vnesite naslov zahtevka.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vnesite znesek.")]
        [Range(0.01, 1000000, ErrorMessage = "Znesek mora biti večji od 0.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vnesite oddelek.")]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Proposed Supplier")]
        public string? ProposedSupplier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vnesite željeni datum dobave.")]
        [DataType(DataType.Date)]
        [Display(Name = "Desired Delivery Date")]
        public DateTime DesiredDeliveryDate { get; set; }

        [Required(ErrorMessage = "Vnesite ime vlagatelja.")]
        [Display(Name = "Applicant Name")]
        public string ApplicantName { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Draft";

        [Display(Name = "Department Head Approved")]
        public bool IsDepartmentHeadApproved { get; set; } = false;

        [Display(Name = "Finance Approved")]
        public bool IsFinanceApproved { get; set; } = false;

        [Display(Name = "Director Approved")]
        public bool IsDirectorApproved { get; set; } = false;

        [Display(Name = "Is Hidden")]
        public bool IsHidden { get; set; } = false;

        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        [Display(Name = "Pur. Order Number")]
        public string? PurchaseOrderNumber { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "API Status Message")]
        public string? ApiStatusMessage { get; set; }
    }
}
