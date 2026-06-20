namespace academy_API.Models;

public class Payment
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public string InvoiceNo { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Method { get; set; } = null!;
    public string? SlipUrl { get; set; }
    public string? Note { get; set; }
    public string? ReceiptPdfUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
}
