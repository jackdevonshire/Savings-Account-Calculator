using Account = SavingsCalculator.SavingsAccount.LifetimeIsaAccount;

namespace SavingsCalculatorTests.SavingsAccount.GivenALifetimeIsaAccount
{
    [Parallelizable]
    public class UserCanDeposit
    {
        private Account _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new Account(
                "Lifetime ISA",
                new DateOnly(2023, 04, 06),
                0,
                5
            );
            
            // Deposit 4K in 2023 tax year
            _subject.Deposit(new DateOnly(2023, 04, 06), 4000);
            
            // Deposit 3K in 2024 tax year
            _subject.Deposit(new DateOnly(2024, 04, 06), 3000);
            
            // Withdraw 1K in 2024 tax year
            _subject.Withdraw(new DateOnly(2024, 04, 06), 1000);

        }
        
        [Test]
        public void ThenCannotDepositMoreIn2023()
        {
            Assert.That(_subject.CanDeposit(new DateOnly(2023, 04, 06), 1), Is.False);
        }
        
        [Test]
        public void ThenCanDepositUpTo1KIn2024()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_subject.CanDeposit(new DateOnly(2024, 04, 06), 1), Is.True);
                Assert.That(_subject.CanDeposit(new DateOnly(2024, 04, 06), 1000), Is.True);
                Assert.That(_subject.CanDeposit(new DateOnly(2024, 04, 06), 1001), Is.False);

                Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 05), 1), Is.True);
                Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 05), 1000), Is.True);
                Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 05), 1001), Is.False);
            });
        }
    }
}