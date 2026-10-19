using ArtemisBank.Core.Domain.Common;

namespace ArtemisBank.Core.Domain.Entities
{
    public class LoanInstallment : BaseEntity
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsLate { get; set; }

        public required Loan Loan { get; set; }
    }
}