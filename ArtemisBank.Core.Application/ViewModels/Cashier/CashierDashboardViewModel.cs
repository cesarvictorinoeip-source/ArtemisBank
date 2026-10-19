namespace ArtemisBank.Core.Application.ViewModels
{
    public class CashierDashboardViewModel
    {
        public int TodayTransactions { get; set; }
        public int TodayPayments { get; set; }
        public int TodayDeposits { get; set; }
        public int TodayWithdrawals { get; set; }
    }
}