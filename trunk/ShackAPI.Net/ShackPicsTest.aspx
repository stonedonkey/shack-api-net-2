<%@ Page Language="C#" AutoEventWireup="true" Inherits="ShackPicsTest" Codebehind="ShackPicsTest.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server" >
    <div>
    
    <asp:TextBox runat="server" id="username"></asp:TextBox><br />
    <asp:TextBox runat="server" id="password"></asp:TextBox><br />    
    <asp:FileUpload runat="server" id="filename" /><br />
    
    <asp:Button runat="server" OnClick="ButtonSubmit_Click" id="ButtonSubmit" Text="Test"></asp:Button>    
    
    </div>
    </form>
    <br />
    <br />
    Not .Net
    
    <form>
    
    
    </form>
    
</body>
</html>
