using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginFacebook.Model
{
    public class FacebookAccount: IFacebookAccount
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Key { get; set; }


    }
}
