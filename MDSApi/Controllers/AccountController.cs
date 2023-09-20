using MDSApi.Authentication;
using MDSApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDSApi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly SignInManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager
            //, SignInManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            //_roleManager = roleManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string _domain = model.Domain;

                if (_domain != null)
                    _domain = _domain.ToUpper();
                else
                    _domain = string.Empty;

                string _username = model.UserName;

                if (_username != null)
                    _username = _username.ToUpper();

                string _email = model.Email;

                if (_email != null)
                    _email = _email.ToUpper();

                var _user = new ApplicationUser
                {
                    UserName = _username,
                    Email = model.Email,
                    Domain = _domain
                };

                //Check user:
                var _checkAcc = _userManager.Users.FirstOrDefault(s => s.Domain == _domain && s.UserName == _username);

                if (_checkAcc != null)
                {
                    ModelState.AddModelError(string.Empty, "User Existed");
                    return View(model);
                }

                var result = await _userManager.CreateAsync(_user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Users", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

            }
            return View(model);
        }
       

        [AllowAnonymous]
        public IActionResult Users()
        {
            var _users = _userManager.Users.Select(s => new UserViewModel
            {
                Id = s.Id,
                Username = s.UserName,
                Email = s.Email,
                Domain = s.Domain
            }).ToList();
            return View(_users);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Create()
        {
            return RedirectToAction("Register");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Edit(string? id)
        {
            var _user = _userManager.Users.Where(s => s.Id == id).Select(s => new UserEditModel
            {
                Id = s.Id,
                Username = s.UserName,
                Email = s.Email,
                Domain = s.Domain
            }).First();
            return View(_user);
        }

        [HttpPost]
        public ActionResult Edit(UserEditModel user)
        {
            if (ModelState.IsValid)
            {
                var _user = _userManager.Users.Where(s => s.Id == user.Id).First();
                var token = _userManager.GeneratePasswordResetTokenAsync(_user).Result;
                var result = _userManager.ResetPasswordAsync(_user, token, user.Password).Result;
                var _result = _userManager.UpdateAsync(_user).Result;
                if (_result.Succeeded)
                    return RedirectToAction("Users", "Account");
                else
                    return View(user);
            }
            return View(user);
        }

        public IActionResult Delete(string? id)
        {
            var _user = _userManager.Users.Where(s => s.Id == id).Select(s => new UserViewModel
            {
                Id = s.Id,
                Username = s.UserName,
                Email = s.Email,
                Domain = s.Domain
            }).First();
            return View(_user);
        }


        [HttpPost]

        public IActionResult Delete(UserViewModel user)
        {
            var _user = _userManager.Users.FirstOrDefault(s => s.Id == user.Id);
            var result = _userManager.DeleteAsync(_user).Result;
            if (result.Succeeded)
                return RedirectToAction("Users", "Account");
            else
                return View(_user);
        }


        public IActionResult ResetPassword(string? id)
        {
            var _user = _userManager.Users.Where(s => s.Id == id).Select(s => new SetPassword
            {
                Id = s.Id,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            }).First();
            return View(_user);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(SetPassword user)
        {
            var _user = _userManager.Users.Where(s => s.Id == user.Id).First();
            var token = _userManager.GeneratePasswordResetTokenAsync(_user).Result;
            var result = _userManager.ResetPasswordAsync(_user, token, user.Password).Result;
            
            if (result.Succeeded)
                return RedirectToAction("Users", "Account");
            else
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Bad Password" });
            }

            //return View(user);
        }
    }
}
