using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Models;
using WEB_APP.Repository.Interface;

namespace WEB_APP.Controllers
{

    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;
        private object _context;
        private string Products;
        private IMenuMasterService _menuMasterService;
        public AccountController(IAccountService villaService, IMapper mapper, IHostingEnvironment hostingEnvironment,IMenuMasterService menuMasterService)
        {
            _accountService = villaService;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _menuMasterService = menuMasterService;
        }

        public async Task<IActionResult> Import()
        {
            APIResponse aPIResponse = new APIResponse();
            DataTable dtTable = new DataTable();
            List<AccountModel> rowList = new List<AccountModel>();
            IFormFile file = Request.Form.Files[0];
            string folderName = "UploadExcel";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            if (file.Length > 0)
            {
                string sFileExtension = Path.GetExtension(file.FileName).ToLower();
                ISheet sheet;
                string fullPath = Path.Combine(newPath, file.FileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                    stream.Position = 0;
                    if (sFileExtension == ".xls")
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                    }
                    else
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
                    }
                    IRow headerRow = sheet.GetRow(0); //Get Header Row
                    int cellCount = headerRow.LastCellNum;
                    sb.Append("<table class='table table-responsive table-bordered'><tr>");
                    for (int j = 0; j < cellCount; j++)
                    {
                        NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);
                        if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                        dtTable.Columns.Add(new DataColumn(cell.ToString(), typeof(string)));
                        sb.Append("<th class='w-25'>" + cell.ToString() + "</th>");
                    }
                    sb.Append("</tr>");
                    sb.AppendLine("<tr>");
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                    {
                        IRow row = sheet.GetRow(i);
                        AccountModel accountModel = new AccountModel();
                        if (row == null) continue;
                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                        for (int j = row.FirstCellNum; j < cellCount; j++)
                        {
                            if (row.GetCell(j) != null)
                            {
                                if (j == 11)
                                {
                                    var item = row.GetCell(11).DateCellValue.TimeOfDay;
                                    sb.Append("<td>" + item + "</td>");
                                }
                                else
                                    sb.Append("<td class='w-5'>" + row.GetCell(j).ToString() + "</td>");
                            }
                        }
                        try
                        {
                            accountModel.AccountNo = Convert.ToDouble(row.GetCell(1).ToString());
                            accountModel.GL_CODE = Convert.ToInt32(row.GetCell(2).ToString());
                            accountModel.GL_NAME = Convert.ToString(row.GetCell(3).ToString());
                            accountModel.AccountHolder_Name = Convert.ToString(row.GetCell(4).ToString());
                            accountModel.Balance = Convert.ToDouble(row.GetCell(5).ToString());
                            accountModel.Mobile_No = Convert.ToString(row.GetCell(6).ToString());
                            accountModel.Customer_ID = Convert.ToString(row.GetCell(7).ToString());
                            accountModel.Society_Name = Convert.ToString(row.GetCell(8).ToString());
                            accountModel.Branch_Name = Convert.ToString(row.GetCell(9).ToString());
                            accountModel.Report_Date = Convert.ToDateTime(row.GetCell(10).ToString());
                            accountModel.Time = Convert.ToString(row.GetCell(11).DateCellValue.TimeOfDay.ToString());
                            accountModel.Aadhar_No = Convert.ToString(row.GetCell(12).ToString());
                            accountModel.Soc_No = Convert.ToString(row.GetCell(13).ToString());

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        rowList.Add(accountModel);
                        sb.AppendLine("</tr>");
                    }




                    sb.Append("</table>");
                    aPIResponse.Result = rowList;
                    aPIResponse.StatusCode = HttpStatusCode.OK;
                    aPIResponse.ErrorMessages.Add(sb.ToString());
                }
            }
            return Ok(aPIResponse);
        }
        public async Task<IActionResult> Index()
        {

            List<AccountModel> list = new List<AccountModel>();

            APIResponse menuForRole = await _menuMasterService.GetMenuFromRole<APIResponse>(HttpContext.User.Claims.ToList()[1].Value.ToString(), HttpContext.User.Claims.ToList()[3].Value.ToString());
            if (menuForRole != null && menuForRole.IsSuccess)
            {
                TempData["RolesMenus"] = JsonConvert.DeserializeObject<List<MenuMasterModel>>(Convert.ToString(menuForRole.Result));
            }

            var response = await _accountService.GetAllAsync<APIResponse>(HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<AccountModel>>(Convert.ToString(response.Result));
                if (list != null && list.Count > 0)
                {
                    double total = 0;
                    foreach (var eachloanAccount in list)
                    {
                        total += eachloanAccount.Balance;
                    }
                    list.LastOrDefault().SumOfCustomerBal = total.ToString();
                }

                ViewData["lastupdatedon"] = list != null && list.Count > 0 ? Convert.ToDateTime(list.LastOrDefault().Report_Date).ToString("dd/MMM/yyyy") + " " + list.LastOrDefault().Time : "";
                
            }
            return View(list);
        }

        // [HttpPost]

        public async Task<APIResponse> Create(string listOfAccounts)
        {
            List<AccountModel> AccountList = JsonConvert.DeserializeObject<List<AccountModel>>(listOfAccounts);

            var response = await _accountService.CreateAsync<APIResponse>(AccountList, HttpContext.User.Claims.LastOrDefault().Value);//await HttpContext.GetTokenAsync("access_token"));
            if (response != null && response.IsSuccess)
            {
                //list = JsonConvert.DeserializeObject<List<AccountModel>>(Convert.ToString(response.Result));
            }
            return response;
        }



        public async Task<IActionResult> UpdateAccount(int villaId)
        {
            var response = await _accountService.GetAsync<APIResponse>(villaId, HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {

                AccountModel model = JsonConvert.DeserializeObject<AccountModel>(Convert.ToString(response.Result));
                return View(_mapper.Map<AccountModel>(model));
            }
            return NotFound();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(AccountModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["success"] = "Account updated successfully";
                var response = await _accountService.UpdateAsync<APIResponse>(model, HttpContext.User.Claims.LastOrDefault().Value);
                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["error"] = "Error encountered.";
            return View(model);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAccount(int villaId)
        {
            var response = await _accountService.GetAsync<APIResponse>(villaId, HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {
                AccountModel model = JsonConvert.DeserializeObject<AccountModel>(Convert.ToString(response.Result));
                return View(model);
            }
            return NotFound();
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(AccountModel model)
        {

            var response = await _accountService.DeleteAsync<APIResponse>(model.Id, HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Account deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Error encountered.";
            return View(model);
        }

        public async Task<IActionResult> GetAccountsFor()
        {
            List<AccountModel> list = new List<AccountModel>();

            var response = await _accountService.GetAllAsync<APIResponse>(HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<AccountModel>>(Convert.ToString(response.Result));
                if (list != null && list.Count > 0)
                {
                    double total = 0;
                    foreach (var eachloanAccount in list)
                    {
                        total += eachloanAccount.Balance;
                    }
                    list.LastOrDefault().SumOfCustomerBal = total.ToString();
                }

                return new JsonResult(list);

            }
            return null;

        }
        [HttpPost]
        public async Task<ActionResult<DataTableResponse>> GetAccounts()
        {


            List<AccountModel> list = new List<AccountModel>();

            var response = await _accountService.GetAllAsync<APIResponse>(HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<AccountModel>>(Convert.ToString(response.Result));
                if (list != null && list.Count > 0)
                {
                    double total = 0;
                    foreach (var eachloanAccount in list)
                    {
                        total += eachloanAccount.Balance;
                    }
                    list.LastOrDefault().SumOfCustomerBal = total.ToString();
                }

            }
            return new DataTableResponse
            {
                RecordsTotal = list.Count(),
                RecordsFiltered = 10,
                Data = list.ToArray()
            };
        }


    }
}
