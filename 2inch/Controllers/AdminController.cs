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

        public IActionResult Login()
        {
            return View("Login");
        }

        public IActionResult AddLink()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");
            return View("AddLink");
        }

        public async Task<IActionResult> AdminPanel()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");
            await reloadLinks();

            return View("AdminPanel");
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
            if(!User.Identity.IsAuthenticated) return View("Login");

            if(newlink.shortLink == null || newlink.longLink == null || newlink.shortLink.Count() <= 0 || newlink.longLink.Count() <= 0)
                return View("AddLink");

            newlink.createdBy = User.Identity.Name;
            newlink.clicked = 0;
            ViewBag.Duple = await Database.GetLinkByShortLink(newlink.shortLink);
            if (ViewBag.Duple == null)
            {
                ViewBag.NewLink = newlink;
                await Database.InsertLink(newlink);

                if(ModelState.IsValid)
                    ModelState.Clear();
            }

            return View("AddLink");
        }

        public async Task<IActionResult> EditSelectedLink(int id) {
            if(!User.Identity.IsAuthenticated) return View("Login");

            Models.Link link = await Database.GetLinkById(id);

            if(!LocalDatabase.EditSelectedLink.ContainsKey(User.Identity.Name))
                LocalDatabase.EditSelectedLink.Add(User.Identity.Name, link);
            else
                LocalDatabase.EditSelectedLink[User.Identity.Name] = link;
            return View("AdminPanel");
        }

        public async Task<IActionResult> DeleteSelectedLink(int id) {
            if(!User.Identity.IsAuthenticated) return View("Login");

            string user = User.Identity.Name;
            if (await Database.DeleteLink(id, user))
            {
                await reloadLinks();
                ViewBag.LinkDeleted = id;
            }else {
                ViewBag.CanNotEdit = true;
            }
            return View("AdminPanel");
        }

        public IActionResult DiscardEdit() {
            if(!User.Identity.IsAuthenticated) return View("Login");

            LocalDatabase.EditSelectedLink.Remove(User.Identity.Name);
            ViewBag.DiscardEdit = true;
            return View("AdminPanel");
        }

        public async Task<List<Models.Link>> reloadLinks() {
            List<Models.Link> LinkList = await Database.GetAllLinks(User.Identity.Name);

            LocalDatabase.Links.Remove(User.Identity.Name);
            LocalDatabase.Links.Add(User.Identity.Name, LinkList);
            return LinkList;
        }

        [HttpGet]
        public async Task<IActionResult> UpdateSelectedLink(int id, string shortLink, string longLink, string owner)
        {
            if(!User.Identity.IsAuthenticated) return View("Login");

            Link link = LocalDatabase.EditSelectedLink.ContainsKey(User.Identity.Name) ? LocalDatabase.EditSelectedLink[User.Identity.Name] : null;

            if(link == null || id != link.id) {
                ViewBag.CanNotedit = true;
                return View("AdminPanel");
            }

            link.shortLink = shortLink;
            link.longLink = longLink;

            if(link.shortLink == null || link.longLink == null || link.shortLink.Count() <= 0 || link.longLink.Count() <= 0) {
                ViewBag.CanNotedit = true;
                return View("AdminPanel");
            }

            if(link.createdBy != owner) {
                if(!await Database.CheckOwner(owner)) {
                    ViewBag.OwnerNotExist = true;
                    return View("AdminPanel");
                }
            }

            LocalDatabase.EditSelectedLink.Remove(User.Identity.Name);
            string user = User.Identity.Name;

            if (await Database.EditLink(link, user, owner))
            {
                if (ModelState.IsValid)
                    ModelState.Clear();

                ViewBag.Edited = link;
            }else {
                ViewBag.CanNotedit = true;
            }
            await reloadLinks();
       
            return View("AdminPanel");
        }
    }
}
