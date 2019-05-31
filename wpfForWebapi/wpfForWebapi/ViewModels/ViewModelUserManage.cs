using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfForWebapi.ViewModels
{
    public class ViewModelUserManage
    {
        /// <summary>
        /// 用户管理界面的信息
        /// </summary>
        public string ViewUserAccount { get; set; }
        public string ViewRoleName { get; set; }
        public string ViewRoleDec { get; set; }

        public static implicit operator ViewModelUserManage(List<ViewModelUserManage> v)
        {
            throw new NotImplementedException();
        }
    }
}
