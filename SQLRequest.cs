#define Production

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;
using System.Configuration;
using System.Text.RegularExpressions;

/*  <summary>
// Summary description for SQLRequest  
// Author : Kanit P.  
// Created : 2015 09 07
// Edited : 2016 04 27
// Decription : For centralize code that using for connect with DB 
// need core method 
// Note : Legacy version 1.01
// </summary>   */

public static class SQLRequest
{
#if !Debugja
    private static String conn =  ConfigurationManager.ConnectionStrings["ConnectionString"].ToString(); // "Data Source=DBBKIINS11G;User ID=PCM;Password=PCMUSR08;Unicode=True";
#else
        private static String conn = "Data Source=DEV11G;User ID=account;Password=accdev11g;Unicode=True";
#endif

    #region Whitelist Filter Parameter Method
    private static void filterParameter(ref OracleCommand cmd)
    {
        String[] listToFilter = { "<script>" };//{ "alert", "<", ">", "'", "\"", "script", "\\", "/" };

        for (int i = 0; i < cmd.Parameters.Count; i++)
        {
            OracleParameter item = cmd.Parameters[i];
            if (item.OracleType == OracleType.NVarChar || item.OracleType == OracleType.NChar || item.OracleType == OracleType.LongVarChar)
            {
                if (item.Value != null && !String.IsNullOrEmpty(item.Value.ToString()))
                {
                    foreach (String omg in listToFilter)
                    {
                        item.Value = item.Value.ToString().Replace(omg, "");
                    }
                }
            }
        }

    }
    #endregion

    #region Automate insert parameter




    private static List<String> GetParameterName(String cmdtext)
    {
        List<String> result = new List<string>();
       // String[] splited = cmdtext.Split(' ', ',');
        String[] splited = cmdtext.Replace("HH24:MI:SS", "")
		.Replace("H:M", "")
		.Replace("I:S", "")
		.Replace("M:S", "")
		.Split(' ', ',');  //ใช้กันเคสพวก นาฬิกา  เช่น 'dd/mm/yyyy  HH24:MI:SS

        for (int i = 0; i < splited.Count(); i++)
        {
            if (splited[i].Contains(":"))
            {
                String paraName = splited[i];
                if (paraName.Length > 1 && !string.IsNullOrEmpty(paraName[(paraName.IndexOf(":") + 1)].ToString().Trim())) // is it really parameter? not just select ' : '
                {
                    //start with :
                    paraName = paraName.Substring(paraName.IndexOf(":") + 1).Replace(")", "").Trim(); // in case  where ( '555' = :wow) ...
                    //String[] dontComeNearMyParameter = { ",", ">", "<", "=", "!" };    paraName = paraName.Split(dontComeNearMyParameter)[0];  or just use  regex
                    paraName = Regex.Split(paraName, @"(?!_)\W+")[0];  // in case nval( :wow,'xxx')

                    paraName = paraName.Trim();

                    if (!String.IsNullOrEmpty(paraName))
                    //paraName = paraName.Substring( splited[i].IndexOf(":")+1 ).Replace(":", "").Trim();
                    result.Add(paraName);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// can use only text type เพราะว่า ถ้าเป็นพวก storeprocedure มันต้องเขียนเอง
    /// </summary>
    /// <param name="oracmd"></param>
    /// <param name="in_paras"></param>
    /// <returns></returns>
    public static OracleCommand AddCommandTxtParameter(String oracmd, params object[] in_paras)
    {
        OracleCommand cmd = new OracleCommand(oracmd);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Clear();
        AddCommandTxtParameter(ref cmd, in_paras);
        return cmd;

    }

    /// <summary>
    /// จะใส่ : paramter value ตามลำดับ in_paras ที่ add เข้ามา
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="in_paras"></param>
    public static void AddCommandTxtParameter(ref OracleCommand cmd, params object[] in_paras)
    {
        #region typeof
        //object:  System.Object
        //string:  System.String
        //bool:    System.Boolean
        //byte:    System.Byte
        //sbyte:   System.SByte
        //short:   System.Int16
        //ushort:  System.UInt16
        //int:     System.Int32
        //uint:    System.UInt32
        //long:    System.Int64
        //ulong:   System.UInt64
        //float:   System.Single
        //double:  System.Double
        //decimal: System.Decimal
        //char:    System.Char
        #endregion
        // get parameter string
        List<String> paraName = GetParameterName(cmd.CommandText);

        //add params
        for (int i = 0, end = paraName.Count(); i < end; i++)
        {
            String paramname = paraName[i];
            if (in_paras == null)
            {
                cmd.Parameters.Add(paramname, "NULL").Value = (object)DBNull.Value;
            }
            else if (in_paras[i] == null)
            {
                cmd.Parameters.Add(paramname, "NULL").Value = (object)DBNull.Value;
            }
            else
            {
                object paraVal = in_paras[i];

                if (paraVal != null)
                {
                    if (paraVal.GetType() == typeof(String))   // or  = typeof(String)
                    {
                        //ถ้าเกิด Error ORA-12704: character set mismatch 
                        //หมายความว่า  OracleType ในcolumn ของtable ใน  Database เป็น varchar ไม่ใช่ varchar2
                        //Solution มีสองแบบ 
                        // 1.1 แก้ที่ query ทำพารามิเตอร์ให้เป็น varchar เอา to_char(:parameter)
                        // 1.2 แก้ที่ query ทำตัวเทียบในwhere ให้เป็น nvarchar เช่น  cast( MYNVARCHAR2 as NVARCHAR2(length) )
                        // 2. add parameter เอง ใช้    OracleType.VarChar

                        cmd.Parameters.Add(paramname, OracleType.NVarChar).Value = paraVal.ToString();
                    }
                    else if (paraVal.GetType() == typeof(DateTime))
                    {
                        cmd.Parameters.Add(paramname, OracleType.DateTime).Value = (DateTime)paraVal;
                    }
                    else if (paraVal.GetType() == typeof(Decimal))
                    {
                        cmd.Parameters.Add(paramname, OracleType.Number).Value = (Decimal)paraVal;
                    }
                    else if (paraVal.GetType() == typeof(int))
                    {
                        cmd.Parameters.Add(paramname, OracleType.Int32).Value = (int)paraVal;
                    }
                    else if (paraVal.GetType() == typeof(float))
                    {
                        cmd.Parameters.Add(paramname, OracleType.Float).Value = (float)paraVal;
                    }
                    else if (paraVal.GetType() == typeof(double))
                    {
                        cmd.Parameters.Add(paramname, OracleType.Double).Value = (double)paraVal;
                    }

                    else { throw new Exception("ไม่รู้จัก type  มาเขียนเองในฟังก์ชัน  AddCommandTxtParameter"); }

                }
            }
        }


    }


    /// <summary>
    /// ใช้กับ กรณีเช่น set บอกว่า พารามิเตอร์เป็น วันที่ null
    /// remark ให้ใช้หลังจาก AddCommandTxtParameter
    /// 1. สร้าง oracle command จาก AddCommandTxtParameter ใส่พารามิเตอร์ทุกตัวให้เรียบร้อย
    /// 2. เรียก ManualSetParaType ทำเฉพาะ พารามิเตอร์ที่ น่าจะเกิดปัญา อย่างพวก null type ที่ไม่ใช่ string
    /// 3. เรียก insert update ฯลฯ  อย่าแอดพารามิเตอร์ตอนนี้ 
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="paraname"></param>
    /// <param name="type"></param>
    public static void ManualSetParaType(  ref OracleCommand cmd ,String paraname , OracleType type, Object Value)
    {
            cmd.Parameters.Remove(cmd.Parameters[paraname]); 
            if (Value == null)
                cmd.Parameters.Add(paraname, type).Value = DBNull.Value;
           else
                cmd.Parameters.Add(paraname, type).Value = Value;
    }

    #endregion

    #region CORE sql method !

    //Select DataTable

    /// <summary>
    /// CORE SELECT METHOD - always throw when error
    /// </summary>
    /// <param name="oraora"></param>
    /// <returns></returns>
    public static DataTable SelectMethod(OracleCommand oraora)
    {
        OracleDataAdapter adap = null;
        DataTable dt = new DataTable();
        filterParameter(ref oraora);
        using (OracleConnection connection = new OracleConnection(conn))
        {
            oraora.Connection = connection;
            using (adap = new OracleDataAdapter(oraora))
            {     // dt = new DataTable("BILLINGDS"); 
                adap.Fill(dt);
            }
        }
        return dt;
    }
    public static DataTable SelectMethod(string query)
    {
        OracleCommand cmd = new OracleCommand(query);
        cmd.CommandType = CommandType.Text;
        return SelectMethod(cmd);
    }
    public static DataTable SelectMethod(OracleCommand oraora, bool NotThrow)
    {
        DataTable dt = new DataTable();
        try
        {
            dt = SelectMethod(oraora);
        }
        catch (Exception ex)
        {
            if (!NotThrow)
            {
#if Production
                throw new Exception("Error : SelectMethod");
#else
                    throw new Exception(ex.ToString());
#endif
            }
        }
        return dt;
    }
    public static DataTable SelectMethod(string query, params object[] in_paraVal)
    {
        OracleCommand cmd = AddCommandTxtParameter(query, in_paraVal);
        return SelectMethod(cmd);
    }

    //Select one Data

    /// <summary>
    ///  number  in oracle = Decimal in c# not int!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static T GetOneScalarData<T>(OracleCommand cmd, bool NotThrow)  
    {

        T Data = default(T); try
        {
            using (OracleConnection connection = new OracleConnection(conn))
            {
                connection.Open();
                cmd.Connection = connection;
                var ob_ret= cmd.ExecuteScalar(); 
               // Data = (T)cmd.ExecuteScalar();
                if  ((ob_ret == null || ob_ret is System.DBNull)  && (typeof(T) == typeof(string)) )
                    Data = default(T);  
                else Data = (T)ob_ret;
            }
        }
        catch (Exception ex)
        {
            if (!NotThrow)
            {
                throw new Exception(ex.ToString());
            }
        }
        return Data;
    }
    public static T GetOneScalarData<T>(String sql, bool NotThrow)
    {
        T Data = default(T);
        OracleCommand cmd = new OracleCommand(sql);
        Data = GetOneScalarData<T>(cmd, NotThrow);
        //OracleConnection connection = new OracleConnection(conn);
        //try
        //{
        //    connection.Open();
        //    OracleCommand cmd = new OracleCommand(sql, connection);
        //    Data = (T)cmd.ExecuteScalar();
        //}
        //catch (Exception ex)
        //{
        //    if (!NotThrow)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //    //    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", "alert(" + ex.Message + ");", true);
        //}
        //finally
        //{
        //    connection.Close();
        //    connection.Dispose();
        //}
        return Data;
    }
    public static T GetOneScalarData<T>(String sql, bool NotThrow, params object[] in_paraVal)
    {
        T Data = default(T);
        OracleCommand cmd = AddCommandTxtParameter(sql, in_paraVal);
        Data = GetOneScalarData<T>(cmd, NotThrow);
        return Data;
    }


    // Insert Update Method
    /// <summary>
    /// send list of cmd in  will do by using transaction if fail - fail all!
    /// </summary>
    /// <param name="queryList"></param>
    /// <param name="NotThrow"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(List<OracleCommand> queryList, bool NotThrow)
    {
        using (OracleConnection connection = new OracleConnection(conn))
        {  OracleTransaction transaction = null;
            try
            {
                
                connection.Open();
                transaction = connection.BeginTransaction();
                foreach (OracleCommand cmd in queryList)
                {
                    OracleCommand dummycmd = cmd;
                    filterParameter(ref dummycmd);
                    ////  OracleCommand cmd = new OracleCommand();
                    //cmd.Connection = connection;
                    //cmd.Transaction = transaction;
                    //cmd.ExecuteNonQuery();

                    dummycmd.Connection = connection;
                    dummycmd.Transaction = transaction;
                    dummycmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                if (!NotThrow)
                {
#if Production
                    throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
                }
                //    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", "alert(" + ex.Message + ");", true);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// for execute non query  
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="NotThrow"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(ref OracleCommand cmd, bool NotThrow)
    {
        using (OracleConnection connection = new OracleConnection(conn))
        {
            OracleTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                filterParameter(ref cmd);
                //  OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                if (!NotThrow)
                {
#if Production
                    throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
                }
                //    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", "alert(" + ex.Message + ");", true);

                return false;
            }
        }
        return true;
    }

    // สำหรับเรียก storeprocedure returnเป็น Datatable
    public static DataTable InsertUpdateMethod(ref OracleCommand cmd,ref OracleParameter prc_out , bool NotThrow)
    {
        DataTable result = new DataTable();
        using (OracleConnection connection = new OracleConnection(conn))
        {
          
            OracleTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                filterParameter(ref cmd);
                //  OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
                transaction.Commit();
                result.Load((OracleDataReader)prc_out.Value);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                if (!NotThrow)
                {
#if Production
                    throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
                }
                //    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", "alert(" + ex.Message + ");", true);

               
            }
        }
        return result;
    }





    /// <summary>
    /// only for cmdText
    /// </summary>
    /// <param name="cmdText"></param>
    /// <param name="NotThrow"></param>
    /// <param name="in_paraVal"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(String cmdText, bool NotThrow, params object[] in_paraVal)
    {
        OracleCommand cmd = AddCommandTxtParameter(cmdText, in_paraVal);
        return InsertUpdateMethod(ref   cmd, NotThrow);
    }

	  
    /// <summary>
    /// สำหรับ cmd ที่ควบคุม transaction เอง ไป rollback commit เองข้างนอก
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="NotThrow"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(OracleCommand cmd, bool NotThrow, ref OracleTransaction transaction)
    { 
            try
            { 
                filterParameter(ref cmd); 
             //   cmd.Connection = connection; ส่งมาข้างนอก
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {  // transaction.Rollback(); 
                if (!NotThrow)
                {
#if Production
                    throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
                }
                return false;
            }
      
        return true;
    }
    /// <summary>
    /// สำหรับ cmd ที่ควบคุม transaction เอง ไป rollback commit เองข้างนอก
    /// </summary>
    /// <param name="cmdL"></param>
    /// <param name="NotThrow"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(List<OracleCommand> cmdL, bool NotThrow, ref OracleTransaction transaction)
    {
       
            try
            { 
                foreach (OracleCommand cmd in cmdL)
                {
                    OracleCommand dummycmd = cmd;
                    filterParameter(ref dummycmd); 

                   // dummycmd.Connection = connection; ส่งมาข้างนอก
                    dummycmd.Transaction = transaction;
                    dummycmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                if (!NotThrow)
                {
#if Production
                    throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
                }
                return false;
            }
       
        return true;
    }
    #endregion



		  #region For custom connection and Tx
    public static String GetconnString()
    {
        return conn;
    }
    /// <summary>
    /// สำหรับ cmd ที่ควบคุม transaction เอง ไป rollback commit เองข้างนอก
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="NotThrow"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(OracleCommand cmd, bool NotThrow,ref OracleConnection connection, ref OracleTransaction transaction)
    { 
            try
            { 
                filterParameter(ref cmd); 
                cmd.Connection = connection;// ส่งมาข้างนอก
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {  // transaction.Rollback(); 
                if (!NotThrow)
                {
#if Production
                    throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
                }
                return false;
            }
      
        return true;
    }
    /// <summary>
    /// สำหรับ cmd ที่ควบคุม transaction เอง ไป rollback commit เองข้างนอก
    /// </summary>
    /// <param name="cmdL"></param>
    /// <param name="NotThrow"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(List<OracleCommand> cmdL, bool NotThrow,ref OracleConnection connection, ref OracleTransaction transaction)
    {
       
            try
            { 
                foreach (OracleCommand cmd in cmdL)
                {
                    OracleCommand dummycmd = cmd;
                    filterParameter(ref dummycmd); 

                    dummycmd.Connection = connection; //ส่งมาข้างนอก
                    dummycmd.Transaction = transaction;
                    dummycmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                if (!NotThrow)
                {
#if Production
                    throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
                }
                return false;
            }
       
        return true;
    }
    /// <summary>
    /// สำหรับ cmd ที่ควบคุม connection คือพวกเรียก store แล้วเดี๋ยวไปดึงค่าจาก out ข้างนอกแล้วค่อยปิด
    /// </summary>
    /// <param name="cmdL"></param>
    /// <param name="NotThrow"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static bool InsertUpdateMethod(ref OracleCommand cmd, bool NotThrow, ref OracleConnection connection)
    {

        try
        { 
                OracleCommand dummycmd = cmd;
                filterParameter(ref dummycmd);

                dummycmd.Connection = connection; //ส่งมาข้างนอก 
                dummycmd.ExecuteNonQuery();
          
        }
        catch (Exception ex)
        {
            //transaction.Rollback();
            if (!NotThrow)
            {
#if Production
                throw new Exception("Error : InsertUpdateMethod");
#else
                    throw new Exception(ex.ToString());
#endif
               
            }
            return false;
        }

        return true;
    }
    public static T GetOneScalarData<T>(OracleCommand cmd, bool NotThrow, ref OracleConnection connection)
    {

        T Data = default(T); try
        { 
                cmd.Connection = connection;
                Data = (T)cmd.ExecuteScalar();
           
        }
        catch (Exception ex)
        {
            if (!NotThrow)
            {
                throw new Exception(ex.ToString());
            }
        }
        return Data;
    }

    #endregion

#region Debuging
    public static String _DebugQuery(OracleCommand cmd)
    {    // ตอน เกิดcatch ใน พวกquery ก็ลองมาเรียกตรงนี้ดู
        String result = cmd.CommandText.ToString();
        if (!String.IsNullOrEmpty(result))
        {
            var paras = cmd.Parameters;
            foreach (OracleParameter para in paras)
            {
                String paraname = ":" + para.ParameterName.Replace(":", "").ToString();
                object paravalue = para.Value;

                if (paravalue == null)
                {
                    result = result.Replace(paraname, "''");
                }
                else if (paravalue != null)
                {
                    if (para.OracleType == OracleType.NVarChar || para.OracleType == OracleType.VarChar)
                    {
                        result = result.Replace(paraname, "'" + para.Value.ToString() + "'");
                    }
                    else if (para.OracleType == OracleType.DateTime)
                    {  //  to_date() ควรไปใช้   varchar เพราะว่าฟอแมตมันแล้วแต่เครื่อง
                        result = result.Replace(paraname, "'" + para.Value.ToString() + "'");
                    }
                    else if (para.OracleType == OracleType.Number
                        || para.OracleType == OracleType.Int32
                         || para.OracleType == OracleType.Float
                         || para.OracleType == OracleType.Double
                        )
                    {
                        result = result.Replace(paraname, para.Value.ToString());
                    }

                    else { throw new Exception("ไม่รู้จัก type  มาเขียนเองในฟังก์ชัน  AddCommandTxtParameter"); }

                }

            }
        }
         return result;

    }

#endregion

}


