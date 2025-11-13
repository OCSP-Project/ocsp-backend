namespace OCSP.Domain.Enums
{
    /// <summary>
    /// Vai trò chi tiết của thành viên trong dự án
    /// </summary>
    public enum ProjectParticipantRole
    {
        // Giám sát
        MainSupervisor = 1,      // Giám sát chính
        SubSupervisor = 2,       // Giám sát phụ

        // Nhà thầu
        MainContractor = 3,      // Nhà thầu chính
        SubContractor = 4,       // Nhà thầu phụ

        // Chủ nhà (owner của project)
        Homeowner = 5
    }
}
