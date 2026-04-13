namespace BoardRent.Tests.Utils
{
    using System;
    using BoardRent.Domain;
    using BoardRent.Utils;
    using Xunit;

    public class SessionContextTests
    {
        [Fact]
        public void Populate_ValidUser_SetsIdentityDisplayNameAndRole()
        {
            SessionContext sessionContext = new SessionContext();
            User user = new User
            {
                Id = Guid.NewGuid(),
                Username = "signed_in_user",
                DisplayName = "Signed In User"
            };

            sessionContext.Populate(user, "Administrator");

            Assert.True(sessionContext.IsLoggedIn);
            Assert.Equal(user.Username, sessionContext.Username);
            Assert.Equal(user.DisplayName, sessionContext.DisplayName);
            Assert.Equal("Administrator", sessionContext.Role);
        }
    }
}
