using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankAccountModels;
using BankAccountInterfaces;
using System.Collections.ObjectModel;

namespace BankAccountController
{
    class BankAccountOperations : IBankAccountOperations
    {
        public void MoneyTransfer(ref Collection<ITransferRecord> transactions, IBank payer, IBank payee, double transactionAmount)
        {

            if (payer.Amount < transactionAmount)
            {
                throw new ArgumentException("You don't have enough money");
            }
            payer.depositAmount(-transactionAmount);
            payee.depositAmount(transactionAmount);
            SaveTransaction(ref transactions, payer.Id, payee.Id, transactionAmount);
        }

        public void SaveTransaction(ref Collection<ITransferRecord> transactions, Guid payerId, Guid payeeId, double transactionAmount)
        {
            if (object.Equals(transactions, null))
            {
                transactions = new Collection<ITransferRecord>();
            }
            var transactionId = Guid.NewGuid();
            var transaction = new TransferRecord(transactionId, payerId, payeeId, transactionAmount);
            transactions.Add(transaction);
        }

        public ICollection<ITransferRecord> GetTransactionByBankId(Collection<ITransferRecord> transactions, Guid bankId)
        {
            var bankTransaction = transactions.Where(elem => elem.PayeeId == bankId || elem.PayerId == bankId).ToList();
            return bankTransaction;
        }
    }
}
