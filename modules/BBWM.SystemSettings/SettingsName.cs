using System.Runtime.Serialization;

namespace BBWM.SystemSettings;

public enum SettingsName
{
    [EnumMember(Value = "FailedAttemptsPassword")] FailedAttemptsPassword,
    [EnumMember(Value = "FeedbackSettings")] FeedbackSettings,
    [EnumMember(Value = "LoadingTimeSettings")] LoadingTimeSettings,
    [EnumMember(Value = "MaintenanceSettings")] MaintenanceSettings,
    [EnumMember(Value = "ProjectSettings")] ProjectSettings,
    [EnumMember(Value = "PwaSettings")] PwaSettings,
    [EnumMember(Value = "RegistrationSettings")] RegistrationSettings,
    [EnumMember(Value = "SessionTimeSettings")] SessionTimeSettings,
    [EnumMember(Value = "UserLoginSettings")] UserLoginSettings,
    [EnumMember(Value = "UserPasswordSettings")] UserPasswordSettings,
    [EnumMember(Value = "UserSessionSettings")] UserSessionSettings,
    [EnumMember(Value = "GoogleSsoSettings")] GoogleSsoSettings,
    [EnumMember(Value = "FacebookSsoSettings")] FacebookSsoSettings,
    [EnumMember(Value = "LinkedInSsoSettings")] LinkedInSsoSettings,
    [EnumMember(Value = "RuntimeEditorSettings")] RuntimeEditorSettings,
    [EnumMember(Value = "FormioSettings")] FormioSettings
}
