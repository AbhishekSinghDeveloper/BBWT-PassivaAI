using BBWM.Core.Data;
using BBWM.Core.ModuleLinker;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System.Text;

namespace BBWM.Messages.Templates;

public class DataModuleLinkage : IInitialDataModuleLinkage
{
    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var context = serviceScope.ServiceProvider.GetService<IDbContext>();
        await EnsureInitialData(context);
    }

    public async Task EnsureInitialData(IDbContext context)
    {
        var parametersSet = context.Set<EmailTemplateParameter>();
        var templatesSet = context.Set<EmailTemplate>();

        // seed Email Template Parameters
        GenerateEmailTemplateParameters(parametersSet);

        // seed Email Tempalates
        GenerateEmailTemplates(templatesSet);

        await context.SaveChangesAsync();
    }

    private void GenerateEmailTemplateParameters(DbSet<EmailTemplateParameter> set)
    {
        foreach (var param in EmailTemplateParameters)
        {
            if (!set.Any(x => x.Title == param.Title))
            {
                set.Add(param);
            }
        }
    }

    private static IEnumerable<EmailTemplateParameter> EmailTemplateParameters
    {
        get
        {
            yield return new EmailTemplateParameter { Title = "$DateTime", Notes = "The current date & time" };
            yield return new EmailTemplateParameter { Title = "$UserName", Notes = "The current user name" };
            yield return new EmailTemplateParameter { Title = "$AppName", Notes = "The application name" };
            yield return new EmailTemplateParameter { Title = "$OldEmail", Notes = "Old email" };
            yield return new EmailTemplateParameter { Title = "$NewEmail", Notes = "New Email" };
            yield return new EmailTemplateParameter { Title = "$CallbackUrl", Notes = "Callback url" };
        }
    }

    private void GenerateEmailTemplates(DbSet<EmailTemplate> set)
    {
        foreach (var template in EmailTemplates)
        {
            if (!set.Any(x => x.Code == template.Code))
            {
                set.Add(template);
            }
        }
    }

    private IEnumerable<EmailTemplate> EmailTemplates
    {
        get
        {
            yield return GenerateEmailTemplateResetPassword();
            yield return GenerateEmailTemplatePasswordChanged();
            yield return GenerateEmailTemplateChangeEmail();
            yield return GenerateEmailTemplateUserInvitation();
            yield return GenerateEmailTemplateEmailConfirmation();
        }
    }

    private static EmailTemplate GenerateEmailTemplateResetPassword()
    {
        var body = new StringBuilder();

        body.AppendEmailHeader()
            .AppendLine(
                "To initiate the password reset process for your <span class=\"ql-bbwt-template-marker badge badge-warning\"><span contenteditable=\"false\">$AppName</span></span> Account, click <a href='$CallbackUrl'>here.</a><br/><br/>")
            .AppendLine(
                "If clicking the link above doesn't work, please copy and paste the URL in a new browser window instead. ")
            .AppendLine(
                "If you've received this mail in error, it's likely that another user entered your email address by mistake while trying to reset a password.<br/><br/>")
            .AppendLine(
                "If you didn't initiate the request, you don't need to take any further action and can safely disregard this email.")
            .AppendEmailFooter();

        return new EmailTemplate
        {
            Code = "ResetPassword",
            Title = "Reset Password",
            From = "noreply@bbwt3.bbconsult.co.uk",
            IsSystem = true,
            Subject = "Reset password for $AppName",
            Body = body.ToString()
        };
    }

    private static EmailTemplate GenerateEmailTemplatePasswordChanged()
    {
        var body = new StringBuilder();

        body.AppendEmailHeader()
            .AppendLine("You have successfully changed your account's password.")
            .AppendEmailFooter();

        return new EmailTemplate
        {
            Code = "PasswordChanged",
            Title = "Password Changed",
            From = "noreply@bbwt3.bbconsult.co.uk",
            IsSystem = true,
            Subject = "Password Changed",
            Body = body.ToString()
        };
    }
    private static EmailTemplate GenerateEmailTemplateChangeEmail()
    {
        var body = new StringBuilder();

        body.AppendEmailHeader()
            .AppendLine("This is to inform you that your email address has been change in $AppName and this address is no longer in use.<br/>")
            .AppendLine("If you have changed your email address recently then no further action is required.<br/>")
            .AppendLine("If you have not changed your email address then you should contact your systems administrator as soon as possible.<br/>")
            .AppendEmailFooter();

        return new EmailTemplate
        {
            Code = "ChangeEmail",
            Title = "Change Email",
            From = "noreply@bbwt3.bbconsult.co.uk",
            IsSystem = true,
            Subject = "Change Email for $AppName",
            Body = body.ToString()
        };
    }

    private static EmailTemplate GenerateEmailTemplateUserInvitation()
    {
        var body = new StringBuilder();

        body.AppendEmailHeader()
            .AppendLine(
                "Click <a href='$CallbackUrl' target='_blank'>here</a> to complete your <span class=\"ql-bbwt-template-marker badge badge-warning\"><span contenteditable=\"false\">$AppName</span></span> account registration. <br/>")
            .AppendLine(
                "If clicking the link above doesn't work, please copy and paste the URL in a new browser window instead.<br/>")
            .AppendEmailFooter();

        return new EmailTemplate
        {
            Code = "UserInvitation",
            Title = "User Invitation",
            From = "noreply@bbwt3.bbconsult.co.uk",
            IsSystem = true,
            Subject = "Welcome to $AppName",
            Body = body.ToString()
        };
    }

    private static EmailTemplate GenerateEmailTemplateEmailConfirmation()
    {
        var body = new StringBuilder();

        body.AppendEmailHeader()
            .AppendLine("Click <a href='$CallbackUrl' target='_blank'>here</a> to confirm your email address <br/>")
            .AppendLine("If clicking the link above doesn't work, please copy and paste the URL in a new browser window instead.")
            .AppendEmailFooter();

        return new EmailTemplate
        {
            Code = "EmailConfirmation",
            Title = "Email Confirmation",
            From = "noreply@bbwt3.bbconsult.co.uk",
            IsSystem = true,
            Subject = "Please confirm Email in $AppName",
            Body = body.ToString()
        };
    }
}

internal static class EmailFormatterExtensions
{
    public static StringBuilder AppendEmailHeader(this StringBuilder body)
    {
        return body.AppendLine(
            "Hello <span class=\"ql-bbwt-template-marker badge badge-warning\"><span contenteditable=\"false\">$UserName</span></span>.")
            .AppendLine("<br/><br/>");
    }

    public static StringBuilder AppendEmailFooter(this StringBuilder body)
    {
        return body
            .AppendLine("<br/><br/>")
            .AppendLine("Thank you for using $AppName.<br/>")
            .AppendLine("Best regards, <span class=\"ql-bbwt-template-marker badge badge-warning\"><span contenteditable=\"false\">$AppName</span></span> team. <br/><br/>")
            .AppendLine("This is an automated email, replies are not monitored or answered.");
    }
}
