namespace UniDocs.Models.Enums
{
    public enum DocumentStatusEnum
    {
        Pending = 0,   // Chờ duyệt / Đang bị ẩn do Report
        Approved = 1,  // Đang hiển thị
        Deleted = 2    // Đã xóa mềm
    }
}
