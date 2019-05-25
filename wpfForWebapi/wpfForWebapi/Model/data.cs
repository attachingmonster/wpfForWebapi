using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfForWebapi.Model
{
    public class Data
    {
        public int ID { get; set; }
        /// <summary>
        /// 用户账号
        /// </summary>
        public string UserAccount { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string UserPassword { get; set; }
        /// <summary>
        /// 回答拾回问题答案
        /// </summary>
        public string QuestionOrAnswer { get; set; }
        /// <summary>
        /// 记住密码的选择，1表示记忆，0表示不记忆
        /// </summary>
        public string RememberPassword { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public string RoleName { get; set; }
    }
}