using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace _2inch.Controllers
{
    public class MainIndexController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}