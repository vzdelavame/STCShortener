﻿using System;
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

        public async Task<IActionResult> UserAdmin()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");
            await reloadUsers();

            return View("UserAdmin");
        }

        public async Task<IActionResult> AddUserAdmin()
        {
            if(!User.Identity.IsAuthenticated) return View("Login");

            return View("AddUserAdmin");
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
            auth = await Database.VerifyAdminCredentials(auth);
            if (auth != null) {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, auth.Name));
                identity.AddClaim(new Claim(ClaimTypes.Name, auth.Name));
                identity.AddClaim(new Claim("Permission", auth.PermissionLevel + ""));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });

                if(!LocalDatabase.ShowAllLinks.ContainsKey(auth.Name)) {
                    LocalDatabase.ShowAllLinks.Add(auth.Name, false);
                }

                return View("ReRoute"); 
            } //Redirects to AdminPanel if returns True
            ViewBag.Passed = false; //Should Update Admin login with 'Incorrect Credentials'
            return View("Login"); //Redirection to the same page if AdminLog.CheckCred(auth) returns False
        } 

        [HttpPost]
        public async Task<IActionResult> AddLinks(Models.Link newlink)
        {
            if(!User.Identity.IsAuthenticated) return View("Login");

            if(!hasPermission(PermissionLevels.AddLinkPermission)) {
                ViewBag.CanNotAdd = true;
                return View("AddLink");
            }

            if(newlink.shortLink == null || newlink.longLink == null || newlink.shortLink.Count() <= 0 || newlink.longLink.Count() <= 0)
                return View("AddLink");

            newlink.createdBy = User.Identity.Name;
            newlink.clicked = 0;
            ViewBag.Duple = await Database.GetLinkByShortLink(newlink.shortLink);
            if (newlink.shortLink.ToLower() == "admin" || newlink.shortLink.ToLower() == "index" || newlink.shortLink.ToLower() == "404")
            {
                ViewBag.Error = true;
            }
            else if (ViewBag.Duple == null)
            {
                ViewBag.NewLink = newlink;
                await Database.InsertLink(newlink);

                if(ModelState.IsValid)
                    ModelState.Clear();
            }

            return View("AddLink");
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(Models.Auth newUser)
        {
            if(!User.Identity.IsAuthenticated) return View("Login");

            if(!hasPermission(PermissionLevels.AddUsersPermission)) {
                ViewBag.CanNotAdd = true;
                return View("AddUserAdmin");
            }

            ViewBag.Duple = await Database.GetUserByEmail(newUser.Name);
            if (ViewBag.Duple == null)
            {
                ViewBag.NewUser = newUser;
                await Database.InsertUser(newUser);

                if(ModelState.IsValid)
                    ModelState.Clear();
            }

            return View("AddUserAdmin");
        }

        public async Task<IActionResult> EditSelectedLink(int id, string createdBy) {
            if(!User.Identity.IsAuthenticated) return View("Login");

            if(!hasPermission(PermissionLevels.EditLinkPermission) && !hasPermission(PermissionLevels.OverrideEditLinkPermission)) {
                ViewBag.CanNotedit = true;
                return View("AdminPanel");
            }

            Models.Link link = await Database.GetLinkById(id);

            if(!LocalDatabase.EditSelectedLink.ContainsKey(User.Identity.Name))
                LocalDatabase.EditSelectedLink.Add(User.Identity.Name, link);
            else
                LocalDatabase.EditSelectedLink[User.Identity.Name] = link;
            return View("AdminPanel");
        }

        public async Task<IActionResult> EditSelectedUser(int id) {
            if(!User.Identity.IsAuthenticated) return View("Login");

            if(!hasPermission(PermissionLevels.EditUsersPermission)) {
                ViewBag.CanNotedit = true;
                return View("UserAdmin");
            }

            Models.Auth auth = await Database.GetUserById(id);

            if(!LocalDatabase.EditSelectedUser.ContainsKey(User.Identity.Name))
                LocalDatabase.EditSelectedUser.Add(User.Identity.Name, auth);
            else
                LocalDatabase.EditSelectedUser[User.Identity.Name] = auth;
            return View("UserAdmin");
        }

        public async Task<IActionResult> DeleteSelectedLink(int id, string createdBy) {
            if(!User.Identity.IsAuthenticated) return View("Login");

            if(!hasPermission(PermissionLevels.DeleteLinkPermission)) {
                ViewBag.CanNotedit = true;
                return View("AdminPanel");
            }

            if(createdBy != User.Identity.Name && !hasPermission(PermissionLevels.OverrideDeleteLinkPermission)) {
                ViewBag.CanNotedit = true;
                return View("AdminPanel");
            }

            string user = User.Identity.Name;
            await Database.DeleteLink(id, user);

            await reloadLinks();

            ViewBag.LinkDeleted = id;

            return View("AdminPanel");
        }

        public async Task<IActionResult> DeleteSelectedUser(int id) {
            if(!User.Identity.IsAuthenticated) return View("Login");

            if(!hasPermission(PermissionLevels.DeleteUsersPermission)) {
                ViewBag.CanNotedit = true;
                return View("UserAdmin");
            }

            await Database.DeleteUser(id);
            await reloadUsers();
            ViewBag.UserDeleted = id;

            return View("UserAdmin");
        }

        public IActionResult DiscardLinkEdit() {
            if(!User.Identity.IsAuthenticated) return View("Login");

            LocalDatabase.EditSelectedLink.Remove(User.Identity.Name);
            ViewBag.DiscardEdit = true;
            return View("AdminPanel");
        }

        public IActionResult DiscardUserEdit() {
            if(!User.Identity.IsAuthenticated) return View("Login");

            LocalDatabase.EditSelectedUser.Remove(User.Identity.Name);
            ViewBag.DiscardEdit = true;
            return View("UserAdmin");
        }

        public async Task<List<Models.Link>> reloadLinks() {
            bool allLinks = LocalDatabase.ShowAllLinks.ContainsKey(User.Identity.Name) ? LocalDatabase.ShowAllLinks[User.Identity.Name] : false;
            List<Models.Link> LinkList = allLinks ? await Database.GetAllLinks() : await Database.GetAllLinksByUser(User.Identity.Name);

            LocalDatabase.Links.Remove(User.Identity.Name);
            LocalDatabase.Links.Add(User.Identity.Name, LinkList);
            return LinkList;
        }

        public async Task<List<Models.Auth>> reloadUsers() {
            if(!hasPermission(PermissionLevels.ViewUsersPermission)) {
                return null;
            }

            List<Models.Auth> AuthList = await Database.GetAllUsers();

            LocalDatabase.Users.Remove(User.Identity.Name);
            LocalDatabase.Users.Add(User.Identity.Name, AuthList);
            return AuthList;
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

            if(link.createdBy != User.Identity.Name && !hasPermission(PermissionLevels.OverrideEditLinkPermission)) {
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

            await Database.EditLink(link, user, owner);

            if (ModelState.IsValid)
                ModelState.Clear();

            ViewBag.Edited = link;
            await reloadLinks();
       
            return View("AdminPanel");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateSelectedUser(int id, string userEmail, int userPermission)
        {
            if(!User.Identity.IsAuthenticated) return View("Login");

            Auth user = LocalDatabase.EditSelectedUser.ContainsKey(User.Identity.Name) ? LocalDatabase.EditSelectedUser[User.Identity.Name] : null;

            if(user == null || id != user.id) {
                ViewBag.CanNotedit = true;
                return View("UserAdmin");
            }

            user.Name = userEmail;
            user.PermissionLevel = userPermission;

            if(!hasPermission(PermissionLevels.EditUsersPermission)) {
                ViewBag.CanNotedit = true;
                return View("UserAdmin");
            }

            LocalDatabase.EditSelectedUser.Remove(User.Identity.Name);

            await Database.EditUser(user);

            if (ModelState.IsValid)
                ModelState.Clear();

            ViewBag.EditedUser = user;
            await reloadUsers();
       
            return View("UserAdmin");
        }

        public bool hasPermission(int PermissionLevel) {
            return int.Parse(User.FindFirstValue("Permission")) >= PermissionLevel;
        }

        [HttpGet]
        public async Task<IActionResult> FilterLinks(bool value)
        {
            LocalDatabase.ShowAllLinks[User.Identity.Name] = value;
            await reloadLinks();
            //Co tu treba: ZIstit ci je Checkbox Checked, podla toho bud odfiltrovat alebo nie a reloadnut page.
            return View("AdminPanel");
        }
    }
}
