using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewModels.Models;

namespace WEB_APP.Repository.Interface
{
    public interface IMenuMasterService
    {
        Task<T> GetAllAsync<T>(string token);
        Task<T> GetAsync<T>(int id, string token);
        Task<T> CreateAsync<T>(MenuMasterModel dto, string token);
        Task<T> UpdateAsync<T>(MenuMasterModel dto, string token);
        Task<T> DeleteAsync<T>(int id, string token);
        Task<T> GetMenusFromRoleAnduser<T>(string rolename, string username, string token);
        Task<T> GetMenuFromRole<T>(string roleName, string token);       

    }
}
