using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB_API.Models;

namespace WEB_API.Repository.Interface
{
    public interface IMenuMaster : IRepository<MenuMaster>
    {
        IEnumerable<MenuMaster> GetMenuMaster();
        IEnumerable<MenuMaster> GetMenuMaster(String UserRole);
        Task<MenuMaster> UpdateAsync(MenuMaster entity);

    }
}
