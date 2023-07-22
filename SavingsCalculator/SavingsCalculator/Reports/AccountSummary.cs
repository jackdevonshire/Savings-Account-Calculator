namespace SavingsCalculator.Reports;

public class AccountSummary
{
    public AccountSummary()
    {
        Transactions = new List<Transaction>();
    }

    public string Account;
    public DateOnly DateFrom;
    public DateOnly DateTo;
    
    public double FinalBalance;
    public double TotalCumulativeInterestAndBenefits;
    
    public List<Transaction> Transactions;
}