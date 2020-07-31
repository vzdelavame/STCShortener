using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace _2inch.Utils
{
    public class LocalDatabase
    {
        public static Dictionary<String, List<Models.Link>> Links = new Dictionary<String, List<Models.Link>>();
        public static Dictionary<String, List<Models.Auth>> Users = new Dictionary<String, List<Models.Auth>>();
        public static Dictionary<String, Models.Link> EditSelectedLink = new Dictionary<String, Models.Link>();
        public static Dictionary<String, Models.Auth> EditSelectedUser = new Dictionary<String, Models.Auth>();
    }
}