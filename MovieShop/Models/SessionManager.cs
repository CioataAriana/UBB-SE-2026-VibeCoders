using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.Models
{
    public static class SessionManager
    {
        public static int CurrentUserID { get; set; } = 1;
        public static bool IsLoggedIn => CurrentUserID > 0;
        public static decimal CurrentUserBalance { get; set; } = 5000.00m;
    }
}
