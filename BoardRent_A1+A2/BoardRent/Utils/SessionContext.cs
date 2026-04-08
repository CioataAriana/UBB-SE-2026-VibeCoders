using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoardRent.Domain;

namespace BoardRent.Utils
{
    public class SessionContext
    {
        private static SessionContext _instance;

        public Guid UserId { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string Role { get; private set; }
        public bool IsLoggedIn { get; private set; }

        private SessionContext()
        {
        }

        public static SessionContext GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SessionContext();
            }
            return _instance;
        }

        public void Populate(User user, string role)
        {
            if (user != null)
            {
                UserId = user.Id;
                Username = user.Username;
                DisplayName = user.DisplayName;
                Role = role;
                IsLoggedIn = true;
            }
        }

        public void Clear()
        {
            UserId = Guid.Empty;
            Username = null;
            DisplayName = null;
            Role = null;
            IsLoggedIn = false;
        }
    }
}
