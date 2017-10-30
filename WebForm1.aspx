<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication2.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      Test Page 

        <br/> 
         <ul>
             <li>
                 - SQLRequest.SelectMethod with 2 Parameters(bind)
                 <br/> SQLRequest.SelectMethod( "SELECT  1 as column1 ,2 as column2, 3 as column3,:A - :xxxx as column4 from dual" , 114 ,110  );
                 <br/>
              <asp:GridView runat="server" ID="gv_data" AutoGenerateColumns="true"></asp:GridView>
             </li>
             <li>
                 - SqlRequest.GetOneScalar < String >
                 <br/>  SQLRequest.GetOneScalarData<String>("Select '1' from dual ", false);
                 <br/> 
               <asp:TextBox runat="server" ID="txt_scalarString" ></asp:TextBox>
             </li>
                 <li>
                 - SqlRequest.GetOneScalar < Decimal > With 2 Parameters(bind)
                 <br/> SQLRequest.GetOneScalarData<Decimal>("Select :a - :b from dual ",false, 20 , 62);
                 <br/> 
               <asp:TextBox runat="server" ID="txt_scalarDecimal" ></asp:TextBox>
             </li>
             
         </ul>

    </div>
    </form>
</body>
</html>
