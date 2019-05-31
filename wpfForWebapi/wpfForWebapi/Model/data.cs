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
      
        public string UserAccount { get; set; }
      
        public string UserPassword { get; set; }
       
        public string QuestionOrAnswer { get; set; }
       
        public string RememberPassword { get; set; }
        public string RoleName { get; set; }
    }
}
