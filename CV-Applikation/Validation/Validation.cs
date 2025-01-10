using System.Text.RegularExpressions;
using CV_Applikation.Models;

namespace CV_Applikation.Validation
{
    public static class Validation
    {
        public static bool CheckEmail(string email)
        {
            // Enkel e-postvalidering med regex
            string pattern = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
            
            // Förhindrar att ha till exempel @gmail.com.com
            int atIndex = email.IndexOf('@');
            string domainPart = email.Substring(atIndex + 1);
            int dotCount = domainPart.Count(c => c == '.');

            return Regex.IsMatch(email, pattern) && dotCount == 1;
        }

        public static bool CheckPhone(string phone)
        {
            // Kollar att telefonnummer är i format 000-0000000
            return Regex.IsMatch(phone, @"^\d{3}-\d{7}$");
        }

        public static bool CheckDate(DateTime startDate, DateTime? endDate)
        {
            // Kollar att slutdatum är efter startdatum om det finns
            return !endDate.HasValue || startDate <= endDate.Value;
        }

        public static bool CheckFutureDate(DateTime date)
        {
            // Kollar att datum inte är i framtiden
            return date <= DateTime.Now;
        }

        public static bool CheckLevel(int level)
        {
            // Kollar att nivån är mellan 1-5
            return level >= 1 && level <= 5;
        }

        public static bool CheckTextLength(string text, int maxLength)
        {
            // Kollar textens längd mot maxgräns
            return !string.IsNullOrEmpty(text) && text.Length <= maxLength;
        }

        public static bool IsIndexValid(int index)
        {
            // Kontrollerar om ett giltigt index är valt i en lista
            return index >= 0;
        }

        public static bool CheckObjectExists(object obj)
        {
            // Kontrollerar att ett objekt inte är null
            return obj != null;
        }

        public static bool CheckRequriedFields(CV cv)
        {
            // Kollar att alla obligatoriska fält är ifyllda
            return !string.IsNullOrEmpty(cv.CVName) &&
                   !string.IsNullOrEmpty(cv.UserId);
        }

        public static bool CheckList<T>(List<T> list)
        {
            // Kollar att en lista inte är null och innehåller minst ett värde
            return list != null && list.Any();
        }
    }
}