using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBWM.Core.Membership.DTO
{
    public class Checking2FADTO
    {
        public string UserId { get; set; }

        [Required]
        [StringLength(7, MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string Code { get; set; }
    }
}
