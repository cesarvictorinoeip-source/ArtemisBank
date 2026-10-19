using ArtemisBank.Core.Application.Interfaces.Services;

namespace ArtemisBank.WebApp.Jobs
{
    public class LoanInstallmentJob
    {
        private readonly ILoanInstallmentService _loanInstallmentService;

        public LoanInstallmentJob(ILoanInstallmentService loanInstallmentService)
        {
            _loanInstallmentService = loanInstallmentService;
        }

        public async Task CheckLateInstallments()
        {
            await _loanInstallmentService.UpdateLateInstallmentsAsync();
        }
    }
}