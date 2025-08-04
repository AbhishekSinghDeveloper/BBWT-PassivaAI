using BBWM.Core.Web;

namespace BBWM.FormIO;

public static class Routes
{
    public static readonly Route FormIOList = new("/app/formio/list", "Form Designs");
    public static readonly Route FormIOSurveyList = new("/app/formio/survey-list", "Surveys");
    public static readonly Route FormIOSurveyPending = new("/app/formio/survey-pending", "Pending Surveys");
    public static readonly Route FormIOInstancesExplorer = new("/app/formio/form-instances-explorer", "Form Instances Explorer");
    public static readonly Route FormIODataExplorer = new("/app/formio/form-data-explorer", "Form Data Explorer");

    public static readonly Route FormIOBuilder = new("/app/formio/builder", "Form Builder");
    public static readonly Route FormIODisplay = new("/app/formio/display", "Form Display");
    public static readonly Route FormioPDFGenerator = new("/app/formio/pdf", "Form PDF Generator");
    public static readonly Route FormioInstances = new("/app/formio/instances", "Form Instances");
    public static readonly Route UserSignature = new("/app/formio/usersignature", "Signature");
    public static readonly Route FormioDisabled = new("/app/formio/disabled", "Forms Feature Disabled");
    public static readonly Route FormioDetails = new("/app/formio/details", "Form Details");
    public static readonly Route FormioRequests = new("/app/formio/requests", "Form Requests");

    public static readonly Route FormioMultiUserList = new("/app/formio/multiuser", "Multi User Forms List");
    public static readonly Route FormioMultiUserStages = new("/app/formio/multiuser/stages", "Multi User Forms Stages");
    public static readonly Route FormioMultiUserDisplay = new("/app/formio/multiuser/display", "Multi User Forms Display");
    public static readonly Route FormioMultiUserDisplayExternal = new("/app/formio/multiuser/external", "Multi User Forms - External Users");

    public static readonly Route FormIOCategory = new("/app/formio/categories", "Form Categories");

}
