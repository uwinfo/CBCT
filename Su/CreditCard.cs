using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Su
{
    public class CreditCard
    {
        public static bool IsCardNumberOk(string cardNumber)
        {
            // Credit card number must be 16 digits; the first 4 digits must be one of these: 1298, 1267, 4512, 4567, 8901, 8933
            //var cardCheck = new Regex(@"^(1298|1267|4512|4567|8901|8933)([\-\s]?[0-9]{4}){3}$");

            //if (!cardCheck.IsMatch(cardNumber))
            //{
            //    return false;
            //} 
            if(cardNumber.Length != 16 || !cardNumber.IsNumeric())
            {
                return false;
            }

            int i, checkSum = 0;

            // Compute checksum of every other digit starting from right-most digit
            for (i = cardNumber.Length - 1; i >= 0; i -= 2)
                checkSum += (cardNumber[i] - '0');

            // Now take digits not included in first checksum, multiple by two,
            // and compute checksum of resulting digits
            for (i = cardNumber.Length - 2; i >= 0; i -= 2)
            {
                int val = ((cardNumber[i] - '0') * 2);
                while (val > 0)
                {
                    checkSum += (val % 10);
                    val /= 10;
                }
            }

            // Number is valid if sum of both checksums MOD 10 equals 0
            return ((checkSum % 10) == 0);
        }

        public static bool IsYmOk(string year, string month)
        {
            if(year.Length == 2)
            {
                year = "20" + year;
            }

            var date = year + "-" + month + "-01";
            if (!date.IsDate())
            {
                return false;
            }

            if (date.ToDate().AddMonths(1) <= DateTime.Now) // AddMonths(1) 己逾期
            {
                return false;
            }

            return true;
        }

        public static bool IsCvvOk(string cvv)
        {
            var cvvCheck = new Regex(@"^\d{3}$");
            if (!cvvCheck.IsMatch(cvv))
            {
                return false;
            }
            return true;
        }
    }
}
