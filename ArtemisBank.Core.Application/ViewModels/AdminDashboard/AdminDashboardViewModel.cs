namespace ArtemisBank.Core.Application.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Transacciones
        public int TotalTransactions { get; set; }
        public int TodayTransactions { get; set; }

        // Pagos
        public int TodayPayments { get; set; }
        public int TotalPayments { get; set; }

        // Clientes
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }

        // Productos financieros
        public int TotalFinancialProducts { get; set; }
        public int TotalLoans { get; set; }
        public int TotalCreditCards { get; set; }
        public int TotalSavingsAccounts { get; set; }

        // Deuda promedio
        public decimal AverageDebtPerClient { get; set; }
    }
}