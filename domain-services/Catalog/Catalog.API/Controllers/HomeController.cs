﻿using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

public class HomeController : Controller
{
    // GET: /<controller>/
    public IActionResult Index()
    {
        return new RedirectResult("~/swagger");
    }
}
