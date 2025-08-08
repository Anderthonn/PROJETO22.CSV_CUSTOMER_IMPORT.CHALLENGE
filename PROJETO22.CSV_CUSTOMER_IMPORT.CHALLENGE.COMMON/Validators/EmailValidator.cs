using System.Net.Mail;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Validators
{
    public static class EmailValidator
    {
        public static bool IsValid(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            email = email.Trim();

            try
            {
                MailAddress mailAddress = new MailAddress(email);

                if (!string.Equals(mailAddress.Address, email, StringComparison.Ordinal))
                    return false;

                string host = mailAddress.Host;

                int lastDot = host.LastIndexOf('.');
                if (lastDot <= 0 || lastDot == host.Length - 1)
                    return false;

                string topLevelDomain = host[(lastDot + 1)..];

                if (topLevelDomain.Length < 2)
                    return false;

                if (!(topLevelDomain.StartsWith("xn--", StringComparison.OrdinalIgnoreCase) || topLevelDomain.All(char.IsLetter)))
                    return false;

                if (host.StartsWith("-") || host.EndsWith("-"))
                    return false;

                if (email.Contains(' ') || email.Contains(".."))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}