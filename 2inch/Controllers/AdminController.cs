using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _2inch.Models;
using _2inch.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace _2inch.Controllers
{
    public class AdminController : Controller
    {

        public IActionResult AdminPanel()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");
            return View("AdminPanel");
        }

        public IActionResult Login()
        {
            return View("Login");
        }

        public IActionResult AddLink()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");
            return View("AddLink");
        }

        public async Task<IActionResult> EditLinks()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");
            await reloadLinks();

            return View("EditLinks");
        }

        public async Task<IActionResult> Logout()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");

            ViewBag.loggedOut = true;
            await HttpContext.SignOutAsync();

            return View("Login");
        }

        public IActionResult NotFoundPage()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");
            return View("NotFound");
        }

        [HttpPost]
        public async Task<IActionResult> Login(Models.Auth auth) //Script for Login
        {
            if (await Database.VerifyAdminCredentials(auth)) {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, auth.Name));
                identity.AddClaim(new Claim(ClaimTypes.Name, auth.Name));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });

                return View("ReRoute"); 
            } //Redirects to AdminPanel if returns True
            ViewBag.Passed = false; //Should Update Admin login with 'Incorrect Credentials'
            return View("Login"); //Redirection to the same page if AdminLog.CheckCred(auth) returns False
        } 

        [HttpPost]
        public async Task<IActionResult> AddLinks(Models.Link newlink)
        {
            if(newlink.shortLink == null || newlink.longLink == null || newlink.shortLink.Count() <= 0 || newlink.longLink.Count() <= 0)
                return View("AddLink");

            newlink.createdBy = User.Identity.Name;
            newlink.clicked = 0;
            ViewBag.Duple = await Database.CheckDuples(newlink.shortLink);
            if (ViewBag.Duple == false)
            {
                await Database.InsertLink(newlink);
            }
            if(ModelState.IsValid)
                ModelState.Clear();
            return View("AddLink");
        }

        public async Task<IActionResult> EditSelectedLink(int id) {
            Models.Link link = await Database.GetLinkById(id);

            LocalDatabase.EditSelectedLink = link;
            return View("EditLinks");
        }

        public async Task<IActionResult> DeleteSelectedLink(int id) {
            
            string user = User.Identity.Name;
            if (await Database.DeleteLink(id, user))
            {
                await reloadLinks();
                ViewBag.LinkDeleted = id;
            }
            return View("EditLinks");
        }

        public async Task<List<Models.Link>> reloadLinks() {
            List<Models.Link> LinkList = await Database.GetAllLinks();
            LocalDatabase.Links = LinkList;
            return LinkList;
        }

        [HttpGet]
        public async Task<IActionResult> UpdateSelectedLink(string shortLink, string longLink)
        {
            Console.WriteLine(shortLink + " " + longLink);
            Link link = LocalDatabase.EditSelectedLink;

            link.shortLink = shortLink;
            link.longLink = longLink;

            LocalDatabase.EditSelectedLink = null;

            if(link.shortLink == null || link.longLink == null || link.shortLink.Count() <= 0 || link.longLink.Count() <= 0) {
                return View("EditLinks");
            }

            string user = User.Identity.Name;

            await Database.EditLink(link, user);

            if(ModelState.IsValid)
                ModelState.Clear();

            ViewBag.Edited = link;

            await reloadLinks();
            return View("EditLinks");
        }

    }
}
