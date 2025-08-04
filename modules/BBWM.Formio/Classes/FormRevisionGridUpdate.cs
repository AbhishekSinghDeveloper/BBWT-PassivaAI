using BBWM.FormIO.DTO.FormViewDTOs;

namespace BBWM.FormIO.Classes;

public class FormRevisionGridUpdate
{
    public IList<FormRevisionGridDTO> Created { get; set; } = new List<FormRevisionGridDTO>();
    public IList<FormRevisionGridDTO> Updated { get; set; } = new List<FormRevisionGridDTO>();
    public IList<FormRevisionGridDTO> Deleted { get; set; } = new List<FormRevisionGridDTO>();
}