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
    public class CbillController : Controller
    {
        // private readonly ICbillService _depositService;
        private readonly ICbillService _CbillService;
        private readonly IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;
        private IMenuMasterService _menuMasterService;


        public CbillController(ICbillService villaService, IMapper mapper, IHostingEnvironment hostingEnvironment,IMenuMasterService menuMasterService)
        {
            _CbillService = villaService;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _menuMasterService = menuMasterService;
        }
        public async Task<IActionResult> Import()
        {
            APIResponse aPIResponse = new APIResponse();
            DataTable dtTable = new DataTable();
            List<CbillModel> rowList = new List<CbillModel>();
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
                    sb.Append("<table class='table table-bordered'><tr>");
                    for (int j = 0; j < cellCount; j++)
                    {
                        NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);
                        if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                        dtTable.Columns.Add(new DataColumn(cell.ToString(), typeof(string)));
                        sb.Append("<th>" + cell.ToString() + "</th>");
                    }
                    sb.Append("</tr>");
                    sb.AppendLine("<tr>");
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                    {
                        IRow row = sheet.GetRow(i);
                        CbillModel accountModel = new CbillModel();
                        if (row == null) continue;
                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                        for (int j = row.FirstCellNum; j < cellCount; j++)
                        {
                            if (row.GetCell(j) != null)
                            {
                                sb.Append("<td>" + row.GetCell(j).ToString() + "</td>");
                            }
                        }
                        try
                        {

                            accountModel.AccountName = string.IsNullOrEmpty(row.GetCell(1).ToString()) ? null : Convert.ToString(row.GetCell(1).ToString());
                            accountModel.PancardNo = string.IsNullOrEmpty(row.GetCell(2).ToString()) ? null : Convert.ToString(row.GetCell(2).ToString());
                            accountModel.AadharCardNo = string.IsNullOrEmpty(row.GetCell(3).ToString()) ? 0 : Convert.ToDouble(row.GetCell(3).ToString());
                            accountModel.ElectionCardNo = string.IsNullOrEmpty(row.GetCell(4).ToString()) ? null : Convert.ToString(row.GetCell(4).ToString());
                            accountModel.SocietyName = string.IsNullOrEmpty(row.GetCell(5).ToString()) ? null : Convert.ToString(row.GetCell(5).ToString());
                            accountModel.BranchName = string.IsNullOrEmpty(row.GetCell(6).ToString()) ? null : Convert.ToString(row.GetCell(6).ToString());
                            accountModel.BranchCode = string.IsNullOrEmpty(row.GetCell(7).ToString()) ? null : Convert.ToString(row.GetCell(7).ToString());
                            accountModel.MobileNo = string.IsNullOrEmpty(row.GetCell(8).ToString()) ? 0 : Convert.ToDouble(row.GetCell(8).ToString());
                            accountModel.GLNAME = string.IsNullOrEmpty(row.GetCell(9).ToString()) ? null : Convert.ToString(row.GetCell(9).ToString());
                            accountModel.Amount = string.IsNullOrEmpty(row.GetCell(10).ToString()) ? 0 : Convert.ToDouble(row.GetCell(10).ToString());

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
            List<CbillModel> list = new List<CbillModel>();

            APIResponse menuForRole = await _menuMasterService.GetMenuFromRole<APIResponse>(HttpContext.User.Claims.ToList()[1].Value.ToString(), HttpContext.User.Claims.ToList()[3].Value.ToString());
            if (menuForRole != null && menuForRole.IsSuccess)
            {
                TempData["RolesMenus"] = JsonConvert.DeserializeObject<List<MenuMasterModel>>(Convert.ToString(menuForRole.Result));
            }

            var response = await _CbillService.GetAllAsync<APIResponse>(HttpContext.User.Claims.LastOrDefault().Value);//await HttpContext.GetTokenAsync("access_token"));
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<CbillModel>>(Convert.ToString(response.Result));
            }
            return View(list);
        }

        public async Task<APIResponse> Create(string listOfAccounts)
        {
            List<CbillModel> AccountList = JsonConvert.DeserializeObject<List<CbillModel>>(listOfAccounts);

            var response = await _CbillService.CreateAsync<APIResponse>(AccountList, HttpContext.User.Claims.LastOrDefault().Value);//await HttpContext.GetTokenAsync("access_token"));
            if (response != null && response.IsSuccess)
            {
                //list = JsonConvert.DeserializeObject<List<AccountModel>>(Convert.ToString(response.Result));
            }
            return response;
        }

        public async Task<IActionResult> GetCbillsFor()
        {
            List<CbillModel> list = new List<CbillModel>();

            var response = await _CbillService.GetAllAsync<APIResponse>(HttpContext.User.Claims.LastOrDefault().Value);
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<CbillModel>>(Convert.ToString(response.Result));
                if (list != null && list.Count > 0)
                {
                    // double total = 0;
                    //foreach (var eachloanAccount in list)
                    //{
                    //    total += eachloanAccount.Balance;
                    //}
                    //list.LastOrDefault().SumOfCustomerBal = total.ToString();
                }

                return new JsonResult(list);

            }
            return null;

        }

    }
}
