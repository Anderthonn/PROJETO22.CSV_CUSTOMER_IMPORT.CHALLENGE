using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Validators;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Common.Validators
{
    public class EmailValidatorTests
    {
        [Theory]
        [InlineData("user@example.com")]
        [InlineData("firstname.lastname@domain.co")]
        [InlineData("user+tag@sub.domain.com")]
        public void IsValid_ShouldReturnTrue_ForValidEmails(string email)
        {
            bool result = EmailValidator.IsValid(email);
            Assert.True(result);
        }

        [Theory]
        [InlineData("plainaddress")]
        [InlineData("user@domain")]
        [InlineData("user@.com")]
        [InlineData("user@@domain.com")]
        public void IsValid_ShouldReturnFalse_ForInvalidEmails(string email)
        {
            bool result = EmailValidator.IsValid(email);
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsValid_ShouldReturnFalse_ForNullOrEmptyOrWhitespace(string? email)
        {
            bool result = EmailValidator.IsValid(email!);
            Assert.False(result);
        }
    }
}