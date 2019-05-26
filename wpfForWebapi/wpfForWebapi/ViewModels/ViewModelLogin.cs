using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfForWebapi.ViewModels
{
    /// <summary>
    /// 登陆
    /// </summary>
    public class ViewModelLogin
    {
        /// <summary>
        /// 用户账号
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 记住密码的选择，1表示记忆，0表示不记忆
        /// </summary>
        public string RememberPassword { get; set; }
    }
}
