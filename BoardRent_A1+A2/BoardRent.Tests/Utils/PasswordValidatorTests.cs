using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardRent.Tests.Utils
{
    using BoardRent.Utils;
    using Xunit;

    public class PasswordValidatorTests
    {
        // run the same test 4 times with different inputs
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Short1!")]
        public void Validate_PasswordTooShortOrNull_ReturnsFalseAndLengthError(string invalidPassword)
        {
            var result = PasswordValidator.Validate(invalidPassword);

            Assert.False(result.IsValid);
            Assert.Equal("Password must be at least 8 characters long.", result.Error);
        }

        [Fact]
        public void Validate_MissingUppercase_ReturnsFalseAndUppercaseError()
        {
            string lowercasePassword = "lowercase123!";

            var result = PasswordValidator.Validate(lowercasePassword);

            Assert.False(result.IsValid);
            Assert.Equal("Password must contain at least one uppercase letter.", result.Error);
        }

        [Fact]
        public void Validate_MissingNumber_ReturnsFalseAndNumberError()
        {
            string noNumberPassword = "NoNumbersHere!";

            var result = PasswordValidator.Validate(noNumberPassword);

            Assert.False(result.IsValid);
            Assert.Equal("Password must contain at least one number.", result.Error);
        }

        [Fact]
        public void Validate_MissingSpecialCharacter_ReturnsFalseAndSpecialCharacterError()
        {
            string noSpecialCharPassword = "NoSpecialChar123";

            var result = PasswordValidator.Validate(noSpecialCharPassword);

            Assert.False(result.IsValid);
            Assert.Equal("Password must contain at least one special character.", result.Error);
        }

        [Fact]
        public void Validate_ValidPassword_ReturnsTrueAndNullError()
        {
            string validPassword = "SuperSecurePassword123!";

            var result = PasswordValidator.Validate(validPassword);

            Assert.True(result.IsValid);
            Assert.Null(result.Error);
        }
    }
}