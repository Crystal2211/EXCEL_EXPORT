using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ViewModels.Models;
using WEB_APP.Models;
using WEB_APP.Repository.Interface;

namespace WEB_APP_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;
        private readonly IMenuMasterService _menuMasterService;

        public HomeController(ILogger<HomeController> logger, IAuthService authService, IMenuMasterService menuMasterService)
        {
            _logger = logger;
            _authService = authService;
            _menuMasterService = menuMasterService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            APIResponse result = await _authService.IsAnyUserPresent<APIResponse>();
            if (result != null && result.IsSuccess)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //create a master user
                    APIResponse result1 = await _authService.RegisterAsync<APIResponse>(new RegisterationRequestModel
                    {
                        UserName = "crystal",
                        Password = "Crystal@123",
                        Role = SD.MasterAdminRole,
                        Name = "MasterAdmin"

                    });
                    if (result1 != null && result1.IsSuccess)
                    {
                        return View();
                    }
                }

            }

            if (HttpContext.User != null && HttpContext.User.Claims != null && HttpContext.User.Claims.ToList().Count > 0 && HttpContext.User.Claims.LastOrDefault().Value != null)
            {
                //check role wise menu present or not

                // check for admin
                APIResponse menuForAdmin = await _menuMasterService.GetMenuFromRole<APIResponse>("admin", HttpContext.User.Claims.ToList()[3].Value.ToString());
                if (menuForAdmin != null && menuForAdmin.IsSuccess && menuForAdmin.Result == null)
                {
                    //admin menus
                    MenuMasterModel AccountmenuMasterModel = new MenuMasterModel { MenuID = "ACCMENU", MenuName = "Share", Parent_MenuID = "*", User_Roll = "admin", MenuFileName = "Index", MenuURL = "Account", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel DepositmenuMasterModel = new MenuMasterModel { MenuID = "DEPMENU", MenuName = "Deposit", Parent_MenuID = "*", User_Roll = "admin", MenuFileName = "Index", MenuURL = "Deposit", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel LoanmenuMasterModel = new MenuMasterModel { MenuID = "LONMENU", MenuName = "Loan", Parent_MenuID = "*", User_Roll = "admin", MenuFileName = "Index", MenuURL = "Loan", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel cbillmenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "Index", MenuID = "CBIMENU", MenuName = "Cbill", MenuURL = "Cbill", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "admin" };
                    MenuMasterModel RegistermenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "Register", MenuID = "REGMENU", MenuName = "Register", MenuURL = "Auth", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "admin" };
                    MenuMasterModel AssignMenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "AssignMenu", MenuID = "ROL", MenuName = "AssignMenu", MenuURL = "", Parent_MenuID = "*", USE_YN = "N", User_Roll = "admin" };
                    MenuMasterModel RolemenuMasterModel = new MenuMasterModel { MenuID = "ROLLWISEMENU", MenuName = "Role Menu", Parent_MenuID = "ROL", User_Roll = "admin", MenuFileName = "AddMenuForRole", MenuURL = "MenuMaster", USE_YN = "N", CreatedDate = DateTime.Now };
                    MenuMasterModel UsermenuMasterModel = new MenuMasterModel { MenuID = "USERROLEMENU", MenuName = "User Menu", Parent_MenuID = "ROL", User_Roll = "admin", MenuFileName = "AddMenuForRole", MenuURL = "MenuMaster", USE_YN = "N", CreatedDate = DateTime.Now };

                    _ = await _menuMasterService.CreateAsync<APIResponse>(AccountmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(DepositmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(LoanmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(cbillmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(RegistermenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(AssignMenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(RolemenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(UsermenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);

                }

                //check for masterAdmin
                APIResponse menuforMasterAdmin = await _menuMasterService.GetMenuFromRole<APIResponse>("MasterAdmin", HttpContext.User.Claims.ToList()[3].Value.ToString());
                if (menuforMasterAdmin != null && menuforMasterAdmin.IsSuccess && menuforMasterAdmin.Result == null)
                {
                    //masteradmin menus
                    MenuMasterModel AccountmenuMasterModel = new MenuMasterModel { MenuID = "ACCMENU", MenuName = "Share", Parent_MenuID = "*", User_Roll = "MasterAdmin", MenuFileName = "Index", MenuURL = "Account", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel DepositmenuMasterModel = new MenuMasterModel { MenuID = "DEPMENU", MenuName = "Deposit", Parent_MenuID = "*", User_Roll = "MasterAdmin", MenuFileName = "Index", MenuURL = "Deposit", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel LoanmenuMasterModel = new MenuMasterModel { MenuID = "LONMENU", MenuName = "Loan", Parent_MenuID = "*", User_Roll = "MasterAdmin", MenuFileName = "Index", MenuURL = "Loan", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel cbillmenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "Index", MenuID = "CBIMENU", MenuName = "Cbill", MenuURL = "Cbill", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "MasterAdmin" };
                    MenuMasterModel RegistermenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "Register", MenuID = "REG", MenuName = "Register", MenuURL = "Auth", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "MasterAdmin" };
                    MenuMasterModel AssignMenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "AssignMenu", MenuID = "ROL", MenuName = "AssignMenu", MenuURL = "", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "MasterAdmin" };
                    MenuMasterModel RolemenuMasterModel = new MenuMasterModel { MenuID = "ROLLWISEMENU", MenuName = "Role Menu", Parent_MenuID = "ROL", User_Roll = "MasterAdmin", MenuFileName = "AddMenuForRole", MenuURL = "MenuMaster", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel UsermenuMasterModel = new MenuMasterModel { MenuID = "USERROLEMENU", MenuName = "User Menu", Parent_MenuID = "ROL", User_Roll = "MasterAdmin", MenuFileName = "AddMenuForRole", MenuURL = "MenuMaster", USE_YN = "Y", CreatedDate = DateTime.Now };

                    _ = await _menuMasterService.CreateAsync<APIResponse>(AccountmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(DepositmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(LoanmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(cbillmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(RegistermenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(AssignMenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(RolemenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(UsermenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                }

                //check for customer
                APIResponse menuforcustomer = await _menuMasterService.GetMenuFromRole<APIResponse>("customer", HttpContext.User.Claims.ToList()[3].Value.ToString());
                if (menuforcustomer != null && menuforcustomer.IsSuccess && menuforcustomer.Result == null)
                {
                    //customer menus
                    MenuMasterModel AccountmenuMasterModel = new MenuMasterModel { MenuID = "ACCMENU", MenuName = "Share", Parent_MenuID = "*", User_Roll = "customer", MenuFileName = "Index", MenuURL = "Account", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel DepositmenuMasterModel = new MenuMasterModel { MenuID = "DEPMENU", MenuName = "Deposit", Parent_MenuID = "*", User_Roll = "customer", MenuFileName = "Index", MenuURL = "Deposit", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel LoanmenuMasterModel = new MenuMasterModel { MenuID = "LONMENU", MenuName = "Loan", Parent_MenuID = "*", User_Roll = "customer", MenuFileName = "Index", MenuURL = "Loan", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel cbillmenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "Index", MenuID = "CBIMENU", MenuName = "Cbill", MenuURL = "Cbill", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "customer" };
                    MenuMasterModel RegistermenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "Register", MenuID = "REG", MenuName = "Register", MenuURL = "Auth", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "customer" };
                    MenuMasterModel AssignMenuMasterModel = new MenuMasterModel { CreatedDate = DateTime.Now, MenuFileName = "AssignMenu", MenuID = "ROL", MenuName = "AssignMenu", MenuURL = "", Parent_MenuID = "*", USE_YN = "Y", User_Roll = "customer" };
                    MenuMasterModel RolemenuMasterModel = new MenuMasterModel { MenuID = "ROLLWISEMENU", MenuName = "Role Menu", Parent_MenuID = "ROL", User_Roll = "customer", MenuFileName = "AddMenuForRole", MenuURL = "MenuMaster", USE_YN = "Y", CreatedDate = DateTime.Now };
                    MenuMasterModel UsermenuMasterModel = new MenuMasterModel { MenuID = "USERROLEMENU", MenuName = "User Menu", Parent_MenuID = "ROL", User_Roll = "customer", MenuFileName = "AddMenuForRole", MenuURL = "MenuMaster", USE_YN = "Y", CreatedDate = DateTime.Now };
                    _ = await _menuMasterService.CreateAsync<APIResponse>(AccountmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(DepositmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(LoanmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(cbillmenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(RegistermenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(AssignMenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(RolemenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);
                    _ = await _menuMasterService.CreateAsync<APIResponse>(UsermenuMasterModel, HttpContext.User.Claims.LastOrDefault().Value);

                }


                APIResponse menuForRole = await _menuMasterService.GetMenuFromRole<APIResponse>(HttpContext.User.Claims.ToList()[1].Value.ToString(), HttpContext.User.Claims.ToList()[3].Value.ToString());
                if (menuForRole != null && menuForRole.IsSuccess)
                {
                    TempData["RolesMenus"] = JsonConvert.DeserializeObject<List<MenuMasterModel>>(Convert.ToString(menuForRole.Result));
                }

                APIResponse result1 = await _authService.GetUserInfo<APIResponse>(HttpContext.User.Claims.ToList()[0].Value.ToString());
                if (result1 != null && result1.IsSuccess && result1.StatusCode == System.Net.HttpStatusCode.OK && result1.Result != null)
                {
                    UserData myDeserializedClass = JsonConvert.DeserializeObject<UserData>(result1.Result.ToString());
                    if (myDeserializedClass != null && myDeserializedClass.soc_Name != null)
                    {
                        return View(myDeserializedClass);
                    }
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
