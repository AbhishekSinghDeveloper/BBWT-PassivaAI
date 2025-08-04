using BBWM.Core.Services;

using System.Collections.Specialized;

namespace BBWM.Messages.Templates;

public interface IEmailTemplateService :
    IEntityGet<EmailTemplateDTO, int>,
    IEntityCreate<EmailTemplateDTO>,
    IEntityUpdate<EmailTemplateDTO>
{
    Task<EmailTemplateDTO> GetByCode(string code, CancellationToken ct = default);
    void BuildEmail(EmailTemplateDTO template, NameValueCollection tagValues);
    bool CheckEmailTemplateCode(string code, int? id);
    string CreateBrand(string logoUrl);
}
