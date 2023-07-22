using SavingsCalculator.SavingsAccount;

var test = new InstantAccessAccount("Test", new DateOnly(2023, 01, 01), 0, 5);
test
    .SetupMonthlyDeposit(new DateOnly(2023, 01, 01), new DateOnly(2023, 12, 01), 28, 100);

Console.WriteLine(test.GetAccountSummary(null).FinalBalance);
    