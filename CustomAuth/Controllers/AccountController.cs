using System.Security.Claims;
using CustomAuth.Entities;
using CustomAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomAuth.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController (AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public IActionResult Index()
        {
            return View(_context.UserAccounts.ToList());
        }

        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {

            if (ModelState.IsValid)
            {
                UserAccount account= new UserAccount();
                account.Email= model.Email;
                account.FirstName= model.FirstName;     
                account.LastName= model.LastName;   
                account.Password= model.Password;   
                account.UserName= model.UserName;


                try
                {
                    _context.UserAccounts.Add(account);
                    _context.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} registered succesfully.Please Login.";
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Please enter unique Email Or Password");
                    return View(model);
                }
                return View();
            }
            return View(model);
        }
          
         public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {


            if (ModelState.IsValid)
            {
                var user = _context.UserAccounts.Where(x => (x.UserName== model.UserNameOrEmail || x.Email== model.UserNameOrEmail) && x.Password == model.Password).FirstOrDefault();
                if (user != null)
                {
                    // Success , create cookie
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("Name", user.FirstName),
                        new Claim(ClaimTypes.Role, "User"),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("SecurePage");

                }
                else
                {
                    ModelState.AddModelError("", "Username/Email or Password is not correct ");
                }
            }
            return View(model);
        }
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }


        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        
        }
    }
}
