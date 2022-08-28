﻿namespace Blog.api.Controllers;

using Blog.Domain.Entities;
using Microsoft.AspNetCore.Mvc;


[Controller]
public abstract class BaseController : ControllerBase
{
    // returns the current authenticated account (null if not logged in)
    public Account Account => (Account)HttpContext.Items["Account"];
}