using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfForWebapi.ViewModels
{
    /// <summary>
    /// 注册
    /// </summary>
    public class ViewModelRegister
    {
        /// <summary>
        /// 账号
        /// </summary>
        public String Account { get; set; }
        /// <summary>
        /// 密码，需要加密
        /// </summary>
        public String Password { get; set; }
        /// <summary>
        /// 这个确定密码是在UI界面做判断的，不需要提交到webapi
        /// </summary>
        public String SurePassword { get; set; }
        /// <summary>
        /// 问题与答案，属于敏感信息，需要加密，建议改为 QuestionOrAnswer
        /// </summary>
        public String QuestionOrAnswer { get; set; }
        /// <summary>
        /// 记忆密码，自动登录
        /// </summary>
        public String RememberPasswerd { get; set; }
        /// <summary>
        /// 角色，根据场景自动赋予一个初始角色
        /// </summary>
        public String RoleName { get; set; }
    }
}
