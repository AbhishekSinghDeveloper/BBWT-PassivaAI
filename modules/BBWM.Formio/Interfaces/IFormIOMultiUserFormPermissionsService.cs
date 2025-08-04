using BBWM.Core.Membership.DTO;
using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Models;
using BBWM.FormIO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.ModelBuilder.Core.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIOMultiUserFormPermissionsService: IEntityQuery<FormIOMultiUserFormPermissionsService>
    {
        Task<bool> NewMultiUserStagePermission(NewMultiUserFormPermissionDTO dto, CancellationToken cancellationToken);
    }
}
