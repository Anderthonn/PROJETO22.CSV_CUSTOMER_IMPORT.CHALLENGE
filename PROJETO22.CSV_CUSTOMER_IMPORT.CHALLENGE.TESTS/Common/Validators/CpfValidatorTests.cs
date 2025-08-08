using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Validators;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Common.Validators
{
    public class CpfValidatorTests
    {
        [Theory]
        [InlineData("529.982.247-25")]
        [InlineData("45317828791")]
        [InlineData("12345678909")]
        public void IsValid_ReturnsTrue_ForValidCpf(string cpf)
        {
            bool result = CpfValidator.IsValid(cpf);
            Assert.True(result);
        }

        [Theory]
        [InlineData("12345678900")]
        [InlineData("111.111.111-11")]
        [InlineData("123")]
        public void IsValid_ReturnsFalse_ForInvalidCpf(string cpf)
        {
            bool result = CpfValidator.IsValid(cpf);
            Assert.False(result);
        }

        [Fact]
        public void IsValid_ReturnsFalse_ForEmptyInput()
        {
            bool result = CpfValidator.IsValid(string.Empty);
            Assert.False(result);
        }
    }
}