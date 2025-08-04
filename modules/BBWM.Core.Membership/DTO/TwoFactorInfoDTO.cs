using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBWM.Core.Membership.DTO
{
    public class TwoFactorInfoDTO
    {
        public string UserId { get; set; }
        public bool U2fEnabled { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool IsRequireUserTwoFactorAuthenticationForSettings { get; set; }
    }
}
