using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ViewModels.Models;
using WEB_APP.Repository.Interface;

namespace WEB_APP.Controllers
{
    public class MenuMasterController : Controller
    {
        private readonly IMenuMasterService _menuMasterService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public MenuMasterController(IMenuMasterService villaService, IAuthService authService, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _menuMasterService = villaService;
            _authService = authService;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            UserWiseMenuModel userWiseMenuModel = new UserWiseMenuModel();
            List<MenuMasterModel> allmenu = new List<MenuMasterModel>();

            var response = await _menuMasterService.GetAllAsync<APIResponse>(HttpContext.User.Claims.LastOrDefault().Value);//await HttpContext.GetTokenAsync("access_token"));
            if (response != null && response.IsSuccess)
            {
                userWiseMenuModel.MenuMasterList = JsonConvert.DeserializeObject<List<MenuMasterModel>>(Convert.ToString(response.Result));
            }

            response = await _authService.GetAllUsers<APIResponse>();
            if (response != null && response.IsSuccess)
            {
                userWiseMenuModel.userModelsList = JsonConvert.DeserializeObject<List<UserModel>>(Convert.ToString(response.Result));
            }

            return View(userWiseMenuModel);
        }

        public async Task<IActionResult> AddMenuForRole()
        {

            APIResponse menuForRole = await _menuMasterService.GetMenuFromRole<APIResponse>(HttpContext.User.Claims.ToList()[1].Value.ToString(), HttpContext.User.Claims.ToList()[3].Value.ToString());
            if (menuForRole != null && menuForRole.IsSuccess)
            {
                TempData["RolesMenus"] = JsonConvert.DeserializeObject<List<MenuMasterModel>>(Convert.ToString(menuForRole.Result));
            }

            MenuMasterModel menuMasterModel = new MenuMasterModel();
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
            return View(menuMasterModel);
        }

        public async Task<APIResponse> GetMenuForRole(string roleName)
        {
            var response = await _menuMasterService.GetMenuFromRole<APIResponse>(roleName, HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {

                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.Result = JsonConvert.DeserializeObject<List<MenuMasterModel>>(Convert.ToString(response.Result));

                }

            }
            else if (response != null && response.StatusCode == HttpStatusCode.NotFound)
            {

            }

            return response;

        }

        
        public ActionResult MenuList(string menuMasterModels)
        {
            return PartialView(JsonConvert.DeserializeObject<List<MenuMasterModel>>(menuMasterModels));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMenuForRole(MenuMasterModel obj)
        {
            if (obj != null)
            {
                if(obj.MenuIdentity==0)
                {
                    //create code
                    obj.CreatedDate = DateTime.Now;
                    APIResponse result = await _menuMasterService.CreateAsync<APIResponse>(obj, HttpContext.User.Claims.LastOrDefault().Value);
                    if (result != null && result.IsSuccess)
                    {
                        TempData["success"] = "Menu Added Sucessfully";
                        return RedirectToAction("AddMenuForRole");
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", result.ErrorMessages.FirstOrDefault());
                        TempData["error"] = result.ErrorMessages.FirstOrDefault();
                        return View(obj);
                    }
                }
                else
                {

                }
              
            }
            return View();
        }
    }
}
