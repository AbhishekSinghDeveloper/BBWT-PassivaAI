using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.Core.Filters;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIOSurveyService :
    IEntityCreate<FormSurveyDTO>,
    IEntityUpdate<FormSurveyDTO>,
    IEntityDelete<int>,
    IEntityPage<FormSurveyPageDTO>
    {
        Task<List<UserSuggestionDTO>> GetAllUserSuggestions(QueryCommand command);
        Task<List<FormRevisionSuggestionDTO>> GetAllFormRevisionsSuggestions(QueryCommand command);
        Task<List<FormSurveyDataDTO>> GetSurveyData(int id);
    }
}
