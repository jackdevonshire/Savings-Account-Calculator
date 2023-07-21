using SavingsCalculator.Reports;

namespace SavingsCalculator.SavingsAccount;

public interface ISavingsAccount
{
    private void CalculateFinance()
    {
        throw new NotImplementedException();
    }

    public AccountSummary GetAccountSummary();
    
    public ISavingsAccount Deposit(DateOnly date, double amount);
    public ISavingsAccount Withdraw(DateOnly date, double amount);
    public ISavingsAccount SetupMonthlyDeposit(DateOnly dateFrom, DateOnly dateTo, int dayOfMonth, double amount);
}