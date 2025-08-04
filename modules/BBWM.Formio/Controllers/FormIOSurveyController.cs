using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers
{
    [Produces("application/json")]
    [Route("api/formio-survey")]
    public class FormIOSurveyController : DataControllerBase<FormSurvey, FormSurveyDTO, FormSurveyPageDTO>
    {
        private readonly IFormIOSurveyService _surveyService;

        public FormIOSurveyController(IDataService dataService, IFormIOSurveyService surveyService)
            : base(dataService, surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet]
        [Route("get-all-users")]
        public async Task<IActionResult> GetAllUserSuggestions([FromQuery] QueryCommand command, CancellationToken ct = default)
        {
            return Ok(await _surveyService.GetAllUserSuggestions(command));
        }

        [HttpGet]
        [Route("get-all-forms")]
        public async Task<IActionResult> GetAllFormRevisionSuggestions([FromQuery] QueryCommand command, CancellationToken ct = default)
        {
            return Ok(await _surveyService.GetAllFormRevisionsSuggestions(command));
        }

        [HttpGet]
        [Route("get-survey-data/{id}")]
        public async Task<IActionResult> GetSurveyData([HashedKeyBinder] int id)
        {
            return Ok(await _surveyService.GetSurveyData(id));
        }
    }
}