namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Validators
{
    public static class CpfValidator
    {
        private static readonly int[] FirstWeights = Enumerable.Range(2, 9).Reverse().ToArray();
        private static readonly int[] SecondWeights = Enumerable.Range(2, 10).Reverse().ToArray();

        public static bool IsValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            string digits = new string(cpf.Where(char.IsDigit).ToArray());

            if (digits.Length != 11)
                return false;

            if (Enumerable.Range(0, 10).Any(d => string.Concat(Enumerable.Repeat((char)('0' + d), 11)) == digits))
                return false;

            bool Check(string number, int[] weights, int expectedDigit)
            {
                int sum = number.Select((c, i) => (c - '0') * weights[i]).Sum();
                int mod = (sum * 10) % 11;
                int digit = mod == 10 ? 0 : mod;
                return digit == expectedDigit;
            }

            if (!Check(digits[..9], FirstWeights, digits[9] - '0'))
                return false;

            if (!Check(digits[..10], SecondWeights, digits[10] - '0'))
                return false;

            return true;
        }
    }
}