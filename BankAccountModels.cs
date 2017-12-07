using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankAccountInterfaces;

namespace BankAccountModels
{
    class Bank : IBank
    {
        private Guid _id;
        public Guid Id
        {
            get { return _id; }
        }

        private double _amount;
        public double Amount
        {
            get { return _amount; }
        }


        public Bank(Guid bankId, double amount)
        {
            _id = bankId;
            _amount = amount;
        }

        public void depositAmount(double amount)
        {
            _amount += amount;
        }
    }

    class TransferRecord : ITransferRecord
    {
        private Guid _id;
        public Guid Id
        {
            get { return _id; }
        }
        public Guid PayerId { get; set; }
        public Guid PayeeId { get; set; }
        public double AmountTransfered { get; }

        public TransferRecord(Guid transferId, Guid payerId, Guid payeeId, double amountTransfered)
        {
            _id = transferId;
            PayerId = payerId;
            PayeeId = payeeId;
            AmountTransfered = amountTransfered;
        }
    }
}
