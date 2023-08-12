using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ViewModels.Models;
using WEB_API.Models;
using WEB_API.Repository.Interface;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;

namespace WEB_API.Controllers
{
    [Route("api/MenuMaster")]
    [ApiController]
    public class MenuMasterController : ControllerBase
    {
        protected ViewModels.Models.APIResponse _response;
        private readonly IMenuMaster _menuMasterDbService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepo;

        public MenuMasterController(IMenuMaster account, IMapper mapper, IUserRepository userRepository)
        {
            _menuMasterDbService = account;
            _mapper = mapper;
            _response = new ViewModels.Models.APIResponse();
            _userRepo = userRepository;
        }
        [HttpGet]
        //[ResponseCache(CacheProfileName = "Default30")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ViewModels.Models.APIResponse>> getMenuMaster([FromQuery(Name = "filterOccupancy")] int? occupancy,
            [FromQuery] string? search, int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                string Role = null;
                string UserName = null;
                var TokenValidations = WEB_API.Helpers.Helpers.TokenValidation(HttpContext.Request.Headers["Bearer"]);
                if (TokenValidations.Key != null && TokenValidations.Value == true)
                {
                    var list = TokenValidations.Key.Split("|");
                    Role = list[0];
                    UserName = list[1];
                }
                else if (TokenValidations.Value == false)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages
                         = new List<string>() { "Session Expire" };
                    return _response;
                }

                IEnumerable<MenuMaster> AccountList = null;

                if (!string.IsNullOrEmpty(search))
                {
                    AccountList = AccountList.Where(u => u.User_Roll.ToLower().Contains(search));
                }
                Pagination pagination = new Pagination() { PageNumber = pageNumber, PageSize = pageSize };

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));
                _response.Result = _mapper.Map<List<MenuMasterModel>>(AccountList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;

        }

        [HttpPost]
        // [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ViewModels.Models.APIResponse>> GetMenuFromRole([FromBody] string rolename)
        {
            try
            {

                string Role = null;
                string UserName = null;
                var TokenValidations = WEB_API.Helpers.Helpers.TokenValidation(HttpContext.Request.Headers["Bearer"]);
                if (TokenValidations.Key != null && TokenValidations.Value == true)
                {
                    var list = TokenValidations.Key.Split("|");
                    Role = list[0];
                    UserName = list[1];
                }
                else if (TokenValidations.Value == false)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages
                         = new List<string>() { "Session Expire" };
                    return _response;
                }

                var menuRecords = await _menuMasterDbService.GetAllAsync(u => u.User_Roll == rolename);
                if (menuRecords != null && menuRecords.Count == 0)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return Ok(_response);

                }
                _response.Result = _mapper.Map<List<MenuMasterModel>>(menuRecords);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Route("CreateMenuRole")]
        [HttpPost]
        // [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ViewModels.Models.APIResponse>> CreateMenuRole([FromBody] MenuMasterModel menuMasterModel)
        {
            try
            {
                string Role = null;
                string UserName = null;
                var TokenValidations = Helpers.Helpers.TokenValidation(HttpContext.Request.Headers["Bearer"]);
                if (TokenValidations.Key != null && TokenValidations.Value == true)
                {
                    var list = TokenValidations.Key.Split("|");
                    Role = list[0];
                    UserName = list[1];
                }
                else if (TokenValidations.Value == false)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages
                         = new List<string>() { "Session Expire" };
                    return _response;
                }

                if (menuMasterModel != null)
                {
                    if (menuMasterModel.MenuIdentity == 0)
                    {
                        //insert mode
                        MenuMaster menuMaster = _mapper.Map<MenuMaster>(menuMasterModel);
                        await _menuMasterDbService.CreateAsync(menuMaster);
                        if (menuMaster != null && menuMaster.MenuIdentity > 0)
                        {
                            _response.IsSuccess = true;
                            _response.StatusCode = HttpStatusCode.OK;
                            _response.Result = _mapper.Map<MenuMasterModel>(menuMaster);
                        }

                    }
                    else
                    {
                        //update mode
                        MenuMaster menuMaster = _mapper.Map<MenuMaster>(menuMasterModel);
                        await _menuMasterDbService.UpdateAsync(menuMaster);
                        if (menuMaster != null && menuMaster.MenuIdentity > 0)
                        {
                            _response.IsSuccess = true;
                            _response.StatusCode = HttpStatusCode.OK;
                            _response.Result = _mapper.Map<MenuMasterModel>(menuMaster);
                        }
                    }
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages
                         = new List<string>() { "Error in passing model" };
                    return _response;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

    }
}
