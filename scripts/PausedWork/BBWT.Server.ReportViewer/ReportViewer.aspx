<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="BBWT.Web.ReportViewer.ReportViewer" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" /> 
    <title></title>
</head>
<body>
    <form id="reportForm" runat="server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>        
        <rsweb:ReportViewer 
            ID="reportControl" 
            runat="server" 
            ProcessingMode="Remote" 
            Width="99.9%" 
            Height="100%" 
            AsyncRendering="true" 
            ZoomMode="Percent" 
            KeepSessionAlive="true" 
            SizeToReportContent="true" >
        </rsweb:ReportViewer>
    </form>
</body>
</html>