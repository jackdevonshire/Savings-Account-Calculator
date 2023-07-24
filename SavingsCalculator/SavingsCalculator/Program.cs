using SavingsCalculator.Reports;
using SavingsCalculator.SavingsAccount;

var moneyBoxAllocations = new List<int> { 0, 100, 200, 300 };
var santanderAllocations = new List<int> { 0, 100, 200 };
var chipAllocations = new List<int> { 0, 100, 200, 300, 400, 500, 600, 700, 80, 900, 1000 };

double maxTotalBalance = 0;
List<AccountSummary> bestSummaries = new List<AccountSummary>();

foreach (var a in moneyBoxAllocations)
{
    foreach (var b in santanderAllocations)
    {
        foreach (var c in chipAllocations)
        {
            if ((a + b + c) != 1000)
                continue;

            var openingDate = new DateOnly(2023, 07, 28);
            var closingDate = new DateOnly(2024, 07, 28);

            var moneybox = new LifetimeIsaAccount("Moneybox 4%", openingDate, 0, 4)
                .SetupMonthlyDeposit(openingDate, closingDate, 28, a)
                .GetAccountSummary(null);
            
            var santander = new InstantAccessAccount("Santander 5%", openingDate, 0, 5)
                .SetupMonthlyDeposit(openingDate, closingDate, 28, b)
                .GetAccountSummary(null);
            
            var chip = new InstantAccessAccount("Chip 4.51%", openingDate, 0, 4.51)
                .SetupMonthlyDeposit(openingDate, closingDate, 28, c)
                .GetAccountSummary(null);

            var currentBalance = moneybox.FinalBalance + santander.FinalBalance + chip.FinalBalance;
            if (currentBalance > maxTotalBalance)
            {
                maxTotalBalance = currentBalance;
                bestSummaries = new List<AccountSummary>
                {
                    moneybox, santander, chip
                };
            }
        }
    }
}

Console.WriteLine(maxTotalBalance);