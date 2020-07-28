using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _2inch.Utils;
using Microsoft.AspNetCore.Mvc;

namespace _2inch.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Models.Auth auth) //Does not get picked up yet. :/
        {
            if (await Database.VerifyAdminCredentials(auth)) return View("AdminPanel"); //Redirects to AdminPanel if returns True
            ViewBag.Passed = false; //Should Update Admin login with 'Incorrect Credentials'
            return View(); //Redirection if AdminLog.CheckCred(auth) returns False
        }
    }
}
