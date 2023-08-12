using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB_API.Repository.Interface;
using WEB_API.Models;
using WEB_API.Data;

namespace WEB_API.Repository.ServiceClass
{
    public class MenuMasterDBService : Repository<MenuMaster>, IMenuMaster
    {
        private readonly ApplicationDbContext _db;
        public MenuMasterDBService(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<MenuMaster> GetMenuMaster()
        {
            return _db.MenuMaster.AsEnumerable();

        }

        public IEnumerable<MenuMaster> GetMenuMaster(string UserRole)
        {
            var result = _db.MenuMaster.Where(m => m.User_Roll == UserRole).ToList();
            return result;
        }

        public IEnumerable<MenuMaster> GetMenuMasterByRoleandUser(string UserRole, string userid)
        {
            List<MenuMaster> menus = new List<MenuMaster>();
            var result = _db.MenuMaster.Where(m => m.User_Roll == UserRole).ToList();
            var userwisemenu = _db.UserWiseMenu.Where(t => t.UserName == userid);
            if (result != null && result.Count > 0)
            {
                if (userwisemenu != null && userwisemenu.Count() > 0)
                {
                    foreach (var eachmenu in userwisemenu)
                    {
                        var menu = result.Where(t => t.MenuID == eachmenu.MenuID).FirstOrDefault();
                        if (menu != null)
                        {
                            menus.Add(menu);
                        }
                    }
                }
                else
                {
                    menus = result;
                }
            }

            return menus;
        }
        public async Task<MenuMaster> UpdateAsync(MenuMaster entity)
        {
            _db.MenuMaster.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteDataAsync(List<MenuMaster> entities)
        {
            _db.MenuMaster.RemoveRange(entities);
            return true;
        }

        
    }
}
