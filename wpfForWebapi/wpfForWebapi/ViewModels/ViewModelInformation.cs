using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfForWebapi.ViewModels
{
    /// <summary>
    /// Webapi返回信息，可以按需要添加属性
    /// </summary>
    public class ViewModelInformation
    {
        /// <summary>
        /// WebAPI返回的信息，包括注册、登陆等
        /// </summary>
        public String message { get; set; }
    }
}
