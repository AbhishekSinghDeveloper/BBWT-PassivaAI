using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIOParameterListService:
    IEntityQuery<FormParameterList>,
    IEntityCreate<FormParameterListDTO>,
    IEntityUpdate<FormParameterListDTO>,
    IEntityDelete<int>,
    IEntityPage<FormParameterListDTO>
    {
    }
}