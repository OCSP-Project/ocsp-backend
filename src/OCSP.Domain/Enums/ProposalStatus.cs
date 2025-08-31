namespace OCSP.Domain.Enums
{
    public enum ProposalStatus
    {
        Draft = 0,       // nhà thầu tạo nháp
        Submitted = 1,   // đã nộp
        Withdrawn = 2,   // rút
        Accepted = 3,    // được chủ nhà chấp nhận
        Rejected = 4     // bị từ chối (khi chủ nhà accept cái khác)
    }
}
