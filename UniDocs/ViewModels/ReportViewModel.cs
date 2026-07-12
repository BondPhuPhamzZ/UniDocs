using System.ComponentModel.DataAnnotations;

namespace UniDocs.ViewModels
{
    public class ReportViewModel
    {
        [Required]
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; } = string.Empty;
    }
}
