using System;
using System.Configuration;
using BBWT.Web.ReportViewer.Services;

namespace BBWT.Web.ReportViewer
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    var token = Request["token"];
                    if (string.IsNullOrEmpty(token))
                    {
                        throw new Exception("Access Denied");
                    }

                    string user;
                    string report;
                    var res = JWTService.ValidateToken(token, out user, out report);

                    if (!res)
                    {
                        throw new Exception("Access denied");
                    }

                    reportControl.ServerReport.ReportServerUrl = new Uri(Environment.GetEnvironmentVariable("SSRS_URL"));
                    reportControl.ServerReport.ReportPath = report;

                    reportControl.ServerReport.Refresh();
                }
                catch (Exception)
                {

                }
            }
        }
    }
}