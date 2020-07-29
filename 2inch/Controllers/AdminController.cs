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

        public IActionResult AdminPanel()
        {
            return View("AdminPanel");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult AddLink()
        {
            return View("AddLink");
        }

        public IActionResult EditLinks()
        {
            return View("EditLinks");
        }

        [HttpPost]
        public async Task<IActionResult> Login(Models.Auth auth) //Script for Login
        {
            if (await Database.VerifyAdminCredentials(auth)) return View("AdminPanel"); //Redirects to AdminPanel if returns True
            ViewBag.Passed = false; //Should Update Admin login with 'Incorrect Credentials'
            return View(); //Redirection to the same page if AdminLog.CheckCred(auth) returns False
        }

        [HttpPost]
        public async Task<IActionResult> AddLinks(Models.Link newlink)
        {
            newlink.createdBy = "Testik";
            newlink.clicked = 0;
            await Database.InsertLink(newlink); 
            return View("AdminPanel");
        }
    }
}
