using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ViewModels.Models;
using WEB_APP.Models;
using WEB_APP.Repository.Interface;

namespace WEB_APP.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]

        public async Task<IActionResult> Login()
        {
            //var accessToken = await HttpContext.GetTokenAsync("access_token");
            //return RedirectToAction(nameof(Index), "Home");
            LoginRequestModel obj = new LoginRequestModel();
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestModel obj)
        {
            APIResponse response = await _authService.LoginAsync<APIResponse>(obj);
            if (response != null && response.IsSuccess)
            {
                LoginResponseModel model = JsonConvert.DeserializeObject<LoginResponseModel>(Convert.ToString(response.Result));

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(model.Token);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "unique_name").Value));
                identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));
                identity.AddClaim(new Claim(ClaimTypes.Email, model.User.Email.ToString()));
                identity.AddClaim(new Claim("access_token", model.Token));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


                HttpContext.Session.SetString(SD.SessionToken, model.Token);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("CustomError", response.ErrorMessages.FirstOrDefault());
                TempData["error"] = response.ErrorMessages.FirstOrDefault();
                return View(obj);
            }
        }

        [HttpGet]
        public async Task<IActionResult> RegisterAsync()
        {

            if (HttpContext.User != null && HttpContext.User.Claims != null && HttpContext.User.Claims.ToList().Count > 0 && HttpContext.User.Claims.LastOrDefault().Value != null)
            {
                APIResponse result = await _authService.GetRoles<APIResponse>();
                List<SelectListItem> roleList = new List<SelectListItem>();
                if (result.IsSuccess == true && result.StatusCode == System.Net.HttpStatusCode.OK && result.Result != null)
                {

                    var roles = JsonConvert.DeserializeObject<string[]>(result.Result.ToString());

                    if (roles != null && roles.Length > 1)
                    {
                        foreach (var role in roles)
                        {
                            roleList.Add(new SelectListItem { Text = role, Value = role });
                        }
                        ViewBag.Roles = roleList;
                    }
                }

                if (HttpContext.User.Claims.ToList()[1].Value == SD.MasterAdminRole)
                {
                    RegisterationRequestModel registerationRequestModel = null;
                    APIResponse response = await _authService.GetMaxSocietyId<APIResponse>();
                    if (response.IsSuccess && response.StatusCode == System.Net.HttpStatusCode.OK && response.Result != null)
                    {
                        registerationRequestModel = new RegisterationRequestModel
                        {
                            Soc_No = (Convert.ToInt32(response.Result) + 1).ToString()
                        };
                    }
                    else if (response.IsSuccess && response.StatusCode == System.Net.HttpStatusCode.NotFound && response.Result == null)
                    {
                        registerationRequestModel = new RegisterationRequestModel
                        {
                            Soc_No = "1"
                        };
                    }

                    return registerationRequestModel != null ? View(registerationRequestModel) : View();
                }
                else if (HttpContext.User.Claims.ToList()[1].Value == SD.AdminRole)
                {
                    if (roleList != null && roleList.Count > 0)
                    {
                        var MasterAdmin = roleList.Where(t => t.Text == SD.MasterAdminRole).FirstOrDefault();
                        roleList.Remove(MasterAdmin);
                        var Admin = roleList.Where(t => t.Text == SD.AdminRole).FirstOrDefault();
                        roleList.Remove(Admin);
                    }
                    APIResponse userDetailsResponse = await _authService.GetUserInfo<APIResponse>(HttpContext.User.Claims.ToList()[0].Value.ToString());
                    RegisterationRequestModel registerationRequestModel = null;
                    if (userDetailsResponse.IsSuccess && userDetailsResponse.StatusCode == System.Net.HttpStatusCode.OK && userDetailsResponse.Result != null)
                    {

                        UserData myDeserializedClass = JsonConvert.DeserializeObject<UserData>(userDetailsResponse.Result.ToString());
                        if (myDeserializedClass != null)
                        {
                            registerationRequestModel = new RegisterationRequestModel();
                            registerationRequestModel.Soc_No = myDeserializedClass.soc_Id;
                            registerationRequestModel.Soc_Name = myDeserializedClass.soc_Name;
                        }
                    }
                    return registerationRequestModel != null ? View(registerationRequestModel) : View();
                }
                else
                {

                    return RedirectToAction("AccessDenied");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationRequestModel obj)
        {
            APIResponse result = await _authService.RegisterAsync<APIResponse>(obj);
            if (result != null && result.IsSuccess)
            {
                TempData["success"] = "Register Sucessfully";
                return RedirectToAction("Register");
            }
            else
            {
                ModelState.AddModelError("CustomError", result.ErrorMessages.FirstOrDefault());
                TempData["error"] = result.ErrorMessages.FirstOrDefault();
            }
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            SignOut("Cookies", "oidc");
            HttpContext.Session.SetString(SD.SessionToken, "");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
