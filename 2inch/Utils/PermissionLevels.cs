using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace _2inch.Utils
{
    public class PermissionLevels
    {
        //Default user
        public static int AddLinkPermission = 5;
        public static int EditLinkPermission = 5;
        public static int DeleteLinkPermission = 5;

        //Admin
        public static int OverrideAddLinkPermission = 25;
        public static int OverrideEditLinkPermission = 25;
        public static int OverrideDeleteLinkPermission = 25;

        //Master Admin
        public static int ViewUsersPermission = 50;
        public static int EditUsersPermission = 50;
        public static int DeleteUsersPermission = 50;
    }
}