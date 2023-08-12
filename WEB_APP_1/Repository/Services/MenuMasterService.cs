using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ViewModels.Models;
using WEB_APP.Repository.Interface;

namespace WEB_APP.Repository.Services
{
    public class MenuMasterService : BaseService, IMenuMasterService
    {
        private readonly IHttpClientFactory _clientFactory;
        private string AccountUrl;

        public MenuMasterService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
        {
            _clientFactory = clientFactory;
            AccountUrl = configuration.GetValue<string>("ServiceUrls:ExcelAPI");

        }

        public Task<T> CreateAsync<T>(MenuMasterModel dto, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = AccountUrl + "/api/MenuMaster/CreateMenuRole",
                Token = token
            });
        }

        public Task<T> DeleteAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = AccountUrl + "/api/MenuMaster/" + id,
                Token = token
            });
        }

        public Task<T> GetAllAsync<T>(string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = AccountUrl + "/api/MenuMaster",
                Token = token
            });
        }

        public Task<T> GetAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = AccountUrl + "/api/MenuMaster/" + id,
                Token = token
            });
        }

        public Task<T> GetMenuFromRole<T>(string roleName, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Url = AccountUrl + "/api/MenuMaster",
                Data = roleName,
                Token = token
            });
        }

        public Task<T> GetMenusFromRoleAnduser<T>(string rolename, string username, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = AccountUrl + "/api/MenuMaster/",
                Data = "rolename:" + rolename + "username:" + username,
                Token = token
            });
        }

        public Task<T> UpdateAsync<T>(MenuMasterModel dto, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = AccountUrl + "/api/MenuMaster/" + dto.MenuIdentity,
                Token = token
            });
        }




    }
}
