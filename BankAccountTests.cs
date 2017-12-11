using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using BankAccountModels;
using BankAccountController;
using BankAccountInterfaces;
using NSubstitute;
using System.Collections.ObjectModel;
using FluentAssertions;
using FluentAssertions.Execution;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace BankAccountTest
{

    [TestClass]
    public class BankTests
    {
        private readonly Guid _bankId1 = new Guid("01E838E1-E81D-40EB-B833-7A19836CB89A");
        private IBank _sut;

        [Theory]
        [InlineData("01E838E1-E81D-40EB-B833-7A19836CB89A", 0)]
        [InlineData("61A40864-22A7-4C1A-A671-C0A9F3FF1E5D", 500)]
        public void Bank_Constructor_should_create_a_bank_with_initialized_properties(string id, double initialAmount)
        {
            var guid = new Guid(id);
            _sut = new Bank(guid, initialAmount);
            _sut.Id.Should().Be(guid);
            _sut.Amount.Should().Be(initialAmount);
        }

        [Fact]
        public void DepositAmount_should_store_the_amount_given()
        {
            var totalAmountToStore = 400;
            _sut = new Bank(_bankId1, 0);
            _sut.depositAmount(totalAmountToStore);
            _sut.Amount.Should().Be(totalAmountToStore);
        }

    }


    [TestClass]
    public class TransferRecordTests
    {
        private ITransferRecord _sut;

        [Theory]
        [InlineData("01E838E1-E81D-40EB-B833-7A19836CB89A", "61A40864-22A7-4C1A-A671-C0A9F3FF1E5D", 700)]
        public void TransferRecord_Constructor_should_create_a_transaction_record_with_initialized_properties(string payerId, string payeeId, double transactionAmount)
        {
            var transactionId = Guid.NewGuid();
            _sut = new TransferRecord(transactionId, new Guid(payerId), new Guid(payeeId), transactionAmount);
            _sut.Id.Should().Be(transactionId);
            _sut.PayerId.Should().Be(new Guid(payerId));
            _sut.PayeeId.Should().Be(new Guid(payeeId));
            _sut.AmountTransfered.Should().Be(transactionAmount);
        }
    }


    [TestClass]
    public class BankAccountTests
    {

        private IBank _payer = new Bank(new Guid("01E838E1-E81D-40EB-B833-7A19836CB89A"), 0);
        private IBank _payee = new Bank(new Guid("61A40864-22A7-4C1A-A671-C0A9F3FF1E5D"), 300);

        private Collection<ITransferRecord> _transactionsHistory;
        private readonly IBankAccountOperations _sut = new BankAccountOperations();

        private readonly IFakeRecord _fakeRecord = Substitute.For<IFakeRecord>();
        private readonly Fixture _fixture = new Fixture();
        // .Register<ITransferRecord>(() => new TransferRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 30));

        [Fact]
        public void BankAccountTransfer_should_throw_exeption_if_payer_amount_cant_cover_payment()
        {
            try
            {
                _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 200);
                Execute.Assertion.FailWith("No exception thrown");
            }
            catch (Exception ex)
            {
                (ex is ArgumentException && ex.Message == "You don't have enough money").Should().BeTrue();
            }
        }

        [Fact]
        public void BankAccountTransfer_should_transfer_amount_from_payer_to_payee()
        {
            _payer.depositAmount(500);
            _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 200);

            _payer.Amount.Should().Be(300);
            _payee.Amount.Should().Be(500);
        }

        [Fact]
        public void BankAccountSaveTransaction_should_instantiate_new_history_of_transactions_if_collection_is_null()
        {
            _sut.SaveTransaction(ref _transactionsHistory, _payer.Id, _payee.Id, 200);
            _transactionsHistory.Count.Should().Be(1);
        }


        [Fact]
        public void BankAccountSaveTransaction_should_return_a_collection_of_transactions_with_one_element()
        {
            _transactionsHistory = new Collection<ITransferRecord>();
            _sut.SaveTransaction(ref _transactionsHistory, _payer.Id, _payee.Id, 200);

             _transactionsHistory.Count.Should().Be(1);

            var lastTransaction = _transactionsHistory[_transactionsHistory.Count - 1];
            lastTransaction.PayerId.Should().Be(_payer.Id);
            lastTransaction.PayeeId.Should().Be(_payee.Id);
            lastTransaction.AmountTransfered.Should().Be(200);
        }

        [Fact]
        public void BankAccountTransfer_should_store_the_record_in_the_transactions_history_with_all_properties()
        {
            _transactionsHistory = new Collection<ITransferRecord>();
            _payer.depositAmount(500);
            _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 200);
            _transactionsHistory.Count.Should().Be(1);

            var lastTransaction = _transactionsHistory[_transactionsHistory.Count - 1];
            lastTransaction.PayerId.Should().Be(_payer.Id);
            lastTransaction.PayeeId.Should().Be(_payee.Id);
            lastTransaction.AmountTransfered.Should().Be(200);
        }

        [Fact]
        public void MoneyTransfer_should_store_all_the_records_in_the_transactions_history()
        {
            _transactionsHistory = new Collection<ITransferRecord>();
            _payer.depositAmount(500);
            _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 100);
            _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 50);
            _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 60);
            _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 100);
            _transactionsHistory.Count.Should().Be(4);
        }


        [Theory]
        [InlineData("01E838E1-E81D-40EB-B833-7A19836CB89A", 4)]
        [InlineData("61A40864-22A7-4C1A-A671-C0A9F3FF1E5D", 7)]
        public void GetTransactionByBankId_should_store_all_the_records_in_the_transactions_history(string bankId, int numberOfTransactions)
        {


            _fixture.Customize(new AutoMoqCustomization());
            _fixture.Inject<double>(1234);

            _fakeRecord.GetFakeRecord().Returns(new TransferRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 30));
            int numberOfFakeTransactions = 50;

            _transactionsHistory = new Collection<ITransferRecord>();
            _payer.depositAmount(500);

            for (int i = 0; i < numberOfTransactions; i++)
            {
                _sut.MoneyTransfer(ref _transactionsHistory, _payer, _payee, 50);
            }
            for (int i = 0; i < numberOfFakeTransactions; i++)
            {
                _transactionsHistory.Add(_fixture.Create<TransferRecord>());
                _transactionsHistory.Add(_fakeRecord.GetFakeRecord());

            }

            _transactionsHistory.Count.Should().Be(numberOfFakeTransactions * 2 + numberOfTransactions);

            var transactions = _sut.GetTransactionByBankId(_transactionsHistory, new Guid(bankId));
            transactions.Count.Should().Be(numberOfTransactions);
        }
    }

}
