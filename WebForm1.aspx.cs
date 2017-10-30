using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            TestQuerySelect();
            TestGetScalar();
        }

        void TestGetScalar()
        {
            txt_scalarString.Text = SQLRequest.GetOneScalarData<String>("Select '1' from dual ", false);
            txt_scalarDecimal.Text = SQLRequest.GetOneScalarData<Decimal>("Select :a - :b from dual ",false, 20 , 62).ToString();
        }

        void TestQuerySelect()
        {
           gv_data.DataSource =  SQLRequest.SelectMethod( "SELECT  1 as column1 ,2 as column2, 3 as column3,:A - :xxxx as column4 from dual" , 114 ,110  );
           gv_data.DataBind();
        }
    }
}