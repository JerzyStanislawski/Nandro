using System;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Nandro.Nano
{
    public static class Tools
    {
        public static BigInteger ToRaw(decimal nanoAmount)
        {
            var fraction = nanoAmount % 1.0m;
            var exponent = 30;
            
            while (fraction != 0.0m && exponent > 0)
            {
                nanoAmount *= 10;
                exponent--;

                fraction = nanoAmount % 1.0m;
            }

            var factor = BigInteger.Pow(10, exponent);
            var result = new BigInteger(nanoAmount) * factor;

            return result;
        }

        public static decimal ToNano(BigInteger raw)
        {
            var exponent = 30;
            var position = Math.Pow(10, -exponent);
            var nano = 0m;

            while (raw > 0 && exponent > 0)
            {
                exponent--;

                var digit = (decimal) (raw % 10);
                nano += digit * ((decimal)position);
               
                raw /= 10;
                position *= 10;
            }

            nano += (decimal)raw;

            return nano;
        }

        internal static string ShortenAccount(string nanoAccount)
        {
            if (String.IsNullOrEmpty(nanoAccount))
                return String.Empty;

            return $"{nanoAccount.Substring(0, 13)}...{nanoAccount.Substring(nanoAccount.Length - 8)}";
        }

        public static bool ValidateAccount(string account)
        {
            const string pattern = "^(nano|xrb)_[13]{1}[13456789abcdefghijkmnopqrstuwxyz]{59}$";
            var regex = new Regex(pattern);
            return regex.IsMatch(account);
        }

        public static void ViewTransaction(string hash)
        {
            var uri = $"https://nanolooker.com/block/{hash}";
            Process.Start("explorer", uri);
        }

        public static void ViewAccountHistory(string account)
        {
            var uri = $"https://nanolooker.com/account/{account}";
            Process.Start("explorer", uri);
        }
    }
}
