using DotNano.Shared.DataTypes;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nandro.Nano
{
    public class AccountInfo
    {
        public List<TransactionEntry> LatestTransactions { get; private set; }

        public string Account { get; }

        public decimal Balance { get; private set; }
        public decimal Pending { get; private set; }
        public string Representative { get; private set; }

        private INanoClient _nanoNodeClient;
        private bool _initialized;

        public AccountInfo(string account)
        {
            Account = account;
        }

        public void UpdateTransactions()
        {
            if (!_initialized)
                return;

            try
            {
                var transactions = _nanoNodeClient.GetLatestTransactions(Account, 10);

                if (LatestTransactions.Any() && transactions.History.Any() && LatestTransactions.First().Hash != transactions.History.First().Hash.HexKeyString)
                    GetBalance();

                LatestTransactions = transactions.History.Select(x => new TransactionEntry(x.Hash, x.Amount, x.Type, DateTimeOffset.FromUnixTimeSeconds(x.LocalTimestamp).UtcDateTime)).ToList();
            }
            catch
            {
            }
        }

        public void GetAccountInfo()
        {
            try
            {
                _nanoNodeClient = Locator.Current.GetService<INanoClient>();

                GetBalance();
                Representative = _nanoNodeClient.GetRepresentative(Account);

                if (!_initialized)
                {
                    LatestTransactions = new List<TransactionEntry>();
                    _initialized = true;
                }
            }
            catch
            {
            }
        }

        private void GetBalance()
        {
            var balance = _nanoNodeClient.GetBalance(Account);
            Balance = Tools.ToNano(balance.Balance);
            Pending = Tools.ToNano(balance.Pending);
        }
    }

    public class TransactionEntry
    {
        public string Hash { get; }
        public decimal Amount { get; }
        public string Type { get; }
        public DateTime TimeStamp { get; }
        public string AmountText => Amount.Normalize();
        public string RelativeTime => TimeHelpers.RelativeTime(TimeStamp);

        public TransactionEntry(HexKey64 hash, BigInteger amount, string type, DateTime timeStamp)
        {
            Hash = hash.HexKeyString;
            Amount = Tools.ToNano(amount);
            Type = type;
            TimeStamp = timeStamp;
        }
    }
}
