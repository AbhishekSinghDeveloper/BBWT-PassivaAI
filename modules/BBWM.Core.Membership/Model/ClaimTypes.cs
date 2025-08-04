namespace BBWM.Core.Membership.Model;

public static class ClaimTypes
{
    public const string BelongsToGroup = "bbwt.claims.belongs_to_group";

    public static class Authentication
    {
        public const string UserRequiredSetupTwoFactor = "bbwt.claim.authentication.user_required_setup_two_factor";
        public const string AuthSecurityStamp = "bbwt.claim.authentication.authentication_security_stamp";
    }

    public static class Impersonation
    {
        public const string OriginalUserId = "bbwt.claims.impersonate.original_user_id";
        public const string OriginalUserName = "bbwt.claims.impersonate.original_user_name";
        public const string IsImpersonating = "bbwt.claims.impersonate.is_impersonating";
    }
}
