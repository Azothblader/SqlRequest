# SqlRequest
Custom ADO.NET connector for using System.Data.OracleClient;   which is deprecated in framework 4.0++


Make your life easier when you need to modify old project which use C# framework 3.5 and below connect to Oracle DB.

The core methods(which has many overloads) are
   -  GetOneScalarData<T> 
   -  SelectMethod        
   -  InsertUpdateMethod  // *has additional overload can get the list of OracleCommands to execute as transaction
   -  AddCommandTxtParameter // for create OracleCommand from String and list of parameters 
  
The example of how to use 


*ต้องการดึงข้อมูล 1 ค่า (get one scalar value) >>
  
            String xx = SQLRequest.GetOneScalarData<String>(
            @"select asset_no from table_mas where created_by =:userx 
            and to_number(status) = :statuscodeInt   ", false, "9999", 20);
  
*ต้องการ select อะไรสักอย่างมาเป็น DataTable (select something as DataTable) >>
  
            DataTable dtd = SQLRequest.SelectMethod(@"select asset_no from table_mas where updated_by =:xuser 
            and to_number(status) = :statuscodeInt and model_code = :modelz   ", "4042", 20, "0007");

*ต้องการสั่ง insert /update/delete (Inline cmd) >>
   
          SQLRequest.InsertUpdateMethod(@"update table_mas set device_tel_no =:newtelno   where updated_by =:xuser 
          and to_number(status) = :statuscodeInt and model_code = :modelz  ", false, "5678", "9999", 20, "0007");
     
*ต้องการเรียก Stored Procedure [Still not improve from the native way that much] >>
 
            string tel_no = "";
            OracleCommand oraComd = new OracleCommand();
            oraComd.CommandType = CommandType.StoredProcedure;
            oraComd.CommandText = "p_package.Sp_X";  
            OracleParameter p_user_id = new OracleParameter("I_USER_ID", OracleType.VarChar, 10);
            p_user_id.Direction = ParameterDirection.Input;
            p_user_id.Value = user_id;
            OracleParameter p_tel_no = new OracleParameter("O_PHONE_NUMBER", OracleType.VarChar, 20);
            p_tel_no.Direction = ParameterDirection.ReturnValue;
            oraComd.Parameters.Add(p_user_id);
            oraComd.Parameters.Add(p_tel_no);
            SQLRequest.InsertUpdateMethod(ref oraComd, false);
            
            tel_no = (p_tel_no.Value != DBNull.Value) ? p_tel_no.Value.ToString() : ""; //ค่า paramter out ก็ return นะ
            return tel_no;
      

*นอกจากนั้น ฟังก์ชันยังมี overload สำหรับส่ง OracleCommand เข้าไปตรงๆได้ 
“และ” InsertUpdateMethod สามารถทำเป็น batchของคำสั่งได้ โดยการส่งlist ของ oracle command เข้าไป
(Deal with OracleCommand as one Transaction!)
 
List<OracleCommand> oraList = new List<OracleCommand>();
            foreach (DataRow dr in dtforSave.Rows)
            { //สร้าง Oracle command ที่ต้องการทำ มาใส่ List (Create OracleCommand to List)

               
              String sqlcmd = "UPDATE table_mas SET batch_no = :batchno , update_ = :userupdate , updated_date = sysdate where asset_no = :assetno ";
              Decimal para1_ = 9999 ;
              String para2_ = "wow value2";
              Decimal para3_ =  8888 ;
            
            oraList.Add(    SQLRequest.AddCommandTxtParameter(sqlcmd, para1 , para2 , para3)   );
               
            }
            SQLRequest.InsertUpdateMethod(oraList, false);  

  
  
  

