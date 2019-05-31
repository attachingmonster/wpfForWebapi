using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfForWebapi.ViewModels
{
    public class ViewModelRetrievePsw
    {
        /// <summary>
        /// 找回密码界面的信息
        /// </summary>
        public String Account { get; set; }
        public String QuestionOrAnswer { get; set; }
        public String NewPassword { get; set; }
        public String SurePassword { get; set; }
    }
}
