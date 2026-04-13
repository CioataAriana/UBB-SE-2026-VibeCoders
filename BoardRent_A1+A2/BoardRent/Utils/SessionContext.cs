namespace BoardRent.Utils
{
    using System;
    using BoardRent.Domain;
    public class SessionContext
    {
        private static SessionContext instance;

        private SessionContext()
        {
        }

        public Guid UserId { get; private set; }

        public string Username { get; private set; }

        public string DisplayName { get; private set; }

        public string Role { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public static SessionContext GetInstance()
        {
            if (instance == null)
            {
                instance = new SessionContext();
            }

            return instance;
        }

        public void Populate(User user, string role)
        {
            if (user != null)
            {
                this.UserId = user.Id;
                this.Username = user.Username;
                this.DisplayName = user.DisplayName;
                this.Role = role;
                this.IsLoggedIn = true;
            }
        }

        public void Clear()
        {
            this.UserId = Guid.Empty;
            this.Username = null;
            this.DisplayName = null;
            this.Role = null;
            this.IsLoggedIn = false;
        }
    }
}