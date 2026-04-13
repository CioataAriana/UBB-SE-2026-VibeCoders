using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardRent.Tests.Utils
{
    using BoardRent.Utils;
    using Xunit;

    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_ValidPassword_ReturnsFormattedStringWithSaltAndHash()
        {
            string password = "ParolaFoarteBuna123!";

            string hashedPassword = PasswordHasher.HashPassword(password);

            Assert.False(string.IsNullOrWhiteSpace(hashedPassword));
            Assert.Contains(":", hashedPassword);

            string[] hashComponents = hashedPassword.Split(':');
            Assert.Equal(2, hashComponents.Length);
        }

        [Fact]
        public void HashPassword_SamePasswordHashedTwice_ReturnsDifferentStringsDueToSalting()
        {
            string password = "ParolaFoarteBuna123!";

            string firstHashedPassword = PasswordHasher.HashPassword(password);
            string secondHashedPassword = PasswordHasher.HashPassword(password);


            Assert.NotEqual(firstHashedPassword, secondHashedPassword);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            string password = "ParolaFoarteBuna123!";
            string hashedPassword = PasswordHasher.HashPassword(password);

            bool isMatch = PasswordHasher.VerifyPassword(password, hashedPassword);

            Assert.True(isMatch);
        }

        [Fact]
        public void VerifyPassword_IncorrectPassword_ReturnsFalse()
        {
            string correctPassword = "ParolaFoarteBuna123!";
            string wrongPassword = "WrongPassword999!";
            string hashedPassword = PasswordHasher.HashPassword(correctPassword);

            bool isMatch = PasswordHasher.VerifyPassword(wrongPassword, hashedPassword);

            Assert.False(isMatch);
        }

        [Fact]
        public void VerifyPassword_MalformedHashFormat_ReturnsFalse()
        {
            string password = "ParolaFoarteBuna123!";
            string malformedHashWithoutColon = "InvalidHashStringWithoutTheSeparator";

            bool isMatch = PasswordHasher.VerifyPassword(password, malformedHashWithoutColon);

            Assert.False(isMatch);
        }
    }
}