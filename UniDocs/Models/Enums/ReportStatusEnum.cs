namespace UniDocs.Models.Enums
{
    public enum ReportStatusEnum
    {
        Pending = 0,    // Đang xử lý
        Valid = 1,      // Hợp lệ (Tài liệu vi phạm thực sự)
        Rejected = 2    // Bỏ qua (Báo cáo sai)
    }
}
