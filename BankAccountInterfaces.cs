using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountInterfaces
{

    interface IBank
    {
        Guid Id { get; }
        double Amount { get; }
        void depositAmount(double amount);
    }

    public interface ITransferRecord
    {
        Guid Id { get; }
        Guid PayerId { get; }
        Guid PayeeId { get; }
        double AmountTransfered { get; set; }
    }

    interface IBankAccountOperations
    {
        void MoneyTransfer(ref Collection<ITransferRecord> transactions, IBank payer, IBank payee, double amount);
        void SaveTransaction(ref Collection<ITransferRecord> transactions, Guid payerId, Guid payeeId, double transactionAmount);
        ICollection<ITransferRecord> GetTransactionByBankId(Collection<ITransferRecord> transactions, Guid payerId);
    }

    public interface IFakeRecord
    {
        ITransferRecord GetFakeRecord();
    }
}
