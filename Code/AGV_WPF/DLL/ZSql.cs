using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections;
using System.Linq;
using System.Configuration;
using System.Windows;

/************************************************************************/
/* 空间名：DAL 类名：ZSql 
 函数                                                                     功能                  
 * public int Multi_Update(string sql1, string sql2, string sql3, string sql4, string sql5)
 * public ZSql()
 * public ZSql(IDbConnection conn)
 * public ZSql(string connstr)
 * public void Close()
 * public void config(string sqlstr, DataTable dt)
 * public void Delete()
 * public DataSet DSet(string sqlstr)
 * public bool Find(string sname, object val)
 * public object GetScalar(string sqlstr)
 * public SqlDataReader GetSqlDataReader(string sqlstr)
 * private void init()
 * public DataRow NewRow()
 * public bool NextRow()
 * public int Open(string sqlstr)
 * public int Open(string sqlstr, DataTable dt)
 * public int Open_StorePro(string spstr, string peo_id, string acc, string p_date_from, string p_date_to)//执行存储过程
 * public IDataReader Open_dr(string sqlstr)
 * public void RemoveAt(int i)
 * public int Update()
 * public int Update(string idname)
 * public object this[string sname]
 * public int rowcount
 * public int rowindex
 * public DataRowCollection Rows
/************************************************************************/

namespace DAL
{
    public class ZSql
    {
        //严格顺序执行更新语句的函数
        public int Multi_Update(string sql1, string sql2, string sql3, string sql4, string sql5)
        {
            {
                //连接数据库的字符串

                string ConnectionString =  ConfigurationManager.AppSettings["ConnString"];

                //创建数据库连接类实例

                SqlConnection conn = new SqlConnection(ConnectionString);

                //创建三个数据库SqlCommand命令类实例

                SqlCommand command1 = new SqlCommand(sql1, conn);
                SqlCommand command2 = new SqlCommand(sql2, conn);
                SqlCommand command3 = new SqlCommand(sql3, conn);
                SqlCommand command4 = new SqlCommand(sql4, conn);
                SqlCommand command5 = new SqlCommand(sql5, conn);

                try
                {
                    conn.Open();//打开数据库
                    command1.ExecuteNonQuery();//执行查询
                    command2.ExecuteNonQuery();
                    if (sql3 != "")
                    {
                        command3.ExecuteNonQuery();
                    }
                    if (sql4 != "")
                    {
                        command4.ExecuteNonQuery();
                    }
                    if (sql5 != "")
                    {
                        command5.ExecuteNonQuery();
                    }

                    return 1;
                }
                catch (Exception)
                {
                    return 0;//即使return了也会执行finally里面的内容
                }

                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();//关闭数据库
                    }
                }
            }
        }

        private SqlCommandBuilder m_cb;
        private SqlCommand m_cmd;
        public SqlConnection m_conn;
        private SqlDataAdapter m_da;
        public SqlDataReader m_dr;
        public DataSet m_ds;
        private string m_idname;
        private int m_rowindex;
        public DataTable m_table;

        /// <summary>
        /// 是否已释放
        /// </summary>
        private bool disposed;

        public ZSql()
        {

            this.m_conn = new SqlConnection();
            //SqlConnection的成员变量ConnectionString
            this.m_conn.ConnectionString = ConfigurationManager.AppSettings["ConnString"]; 
               // ConfigurationManager.AppSettings["ConnString"];
            this.init();
        }

        public ZSql(IDbConnection conn)
        {
            this.m_conn = (SqlConnection)conn;
            this.init();
        }

        public ZSql(string connstr)
        {
            this.m_conn = new SqlConnection();
            this.m_conn.ConnectionString = connstr;
            this.init();
        }

        public void Close()
        {
            if (this.m_dr != null)
            {
                this.m_dr.Close();
            }
            this.m_conn.Close();
        }

        public void config(string sqlstr, DataTable dt)
        {
            this.m_cmd.CommandText = sqlstr;
            this.m_table = dt;
        }

        public void Delete()
        {
            if (this.rowindex >= 0)
            {
                this.m_table.Rows[this.rowindex].Delete();
            }
        }

        public DataSet DSet(string sqlstr)
        {
            this.m_cmd.CommandText = sqlstr;
            this.m_da.Fill(this.m_ds);
            return this.m_ds;
        }

        public DataSet DSet(string sqlstr, SqlParameter[] parameters)
        {
            this.m_cmd.Parameters.Clear();
            this.m_cmd.CommandText = sqlstr;
            foreach (SqlParameter parameter in parameters)
            {
                this.m_cmd.Parameters.Add(parameter);
            }
            this.m_cmd.CommandText = sqlstr;
            this.m_da.Fill(this.m_ds);
            return this.m_ds;
        }

        public bool Find(string sname, object val)
        {
            this.rowindex = -1;
            while (this.NextRow())
            {
                if (this[sname].Equals(val))
                {
                    break;
                }
            }
            if (this.rowindex == -1)
            {
                return false;
            }
            return true;
        }

        public object GetScalar(string sqlstr)
        {
            ConnectionState state = this.m_conn.State;
            if (this.m_conn.State == ConnectionState.Closed)
            {
                this.m_conn.Open();
            }
            object obj2 = new SqlCommand(sqlstr, this.m_conn).ExecuteScalar();
            if (state == ConnectionState.Closed)
            {
                this.m_conn.Close();
            }
            return obj2;
        }

        public SqlDataReader GetSqlDataReader(string sqlstr)
        {
            this.m_cmd.CommandText = sqlstr;
            this.m_dr = this.m_cmd.ExecuteReader();
            return this.m_dr;
        }

        private void init()
        {
            this.m_cmd = new SqlCommand();
            this.m_da = new SqlDataAdapter();
            this.m_cb = new SqlCommandBuilder(this.m_da);
            this.m_cmd.Connection = this.m_conn;
            this.m_da.SelectCommand = this.m_cmd;
            this.m_cmd.CommandType = CommandType.Text;
            this.m_table = new DataTable();
            this.m_ds = new DataSet();
            this.m_idname = "";
            this.m_dr = null;
        }

        public DataRow NewRow()
        {
            DataRow row = this.m_table.NewRow();
            this.m_table.Rows.Add(row);
            this.rowindex = this.m_table.Rows.Count - 1;
            return row;
        }

        public bool NextRow()
        {
            this.rowindex++;
            return (this.rowindex >= 0);
        }

        public int Open(string sqlstr, SqlParameter[] parameters)
        {
            try
            {
                this.m_cmd.Parameters.Clear();
                this.m_cmd.CommandText = sqlstr;
                foreach (SqlParameter parameter in parameters)
                {
                    this.m_cmd.Parameters.Add(parameter);
                }
                this.m_da = new SqlDataAdapter();
                this.m_cb = new SqlCommandBuilder(this.m_da);
                this.m_da.SelectCommand = this.m_cmd;
                this.m_table.Reset();
                this.m_da.Fill(this.m_table);
                this.rowindex = -1;
                return this.rowcount;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("数据库操作失败！");
                return -1;
            }
        }

        public int Open(string sqlstr)
        {
            try
            {
                this.m_cmd.CommandText = sqlstr;
                this.m_da = new SqlDataAdapter();
                this.m_cb = new SqlCommandBuilder(this.m_da);
                this.m_da.SelectCommand = this.m_cmd;
                this.m_table.Reset();
                this.m_da.Fill(this.m_table);
                this.rowindex = -1;
                return this.rowcount;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("数据库操作失败！");
                return -1;
            }
        }

        public int Open(string sqlstr, DataTable dt)
        {
            try
            {
                this.m_cmd.CommandText = sqlstr;
                this.m_table.Reset();
                this.m_da.Fill(dt);
                this.m_table = dt;
                this.rowindex = -1;
                return this.rowcount;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("数据库操作失败！");
                return -1;
            }
        }

        public int Open_StorePro(string spstr, string peo_id, string acc, string p_date_from, string p_date_to)//执行存储过程
        {
            this.m_cmd.CommandText = spstr;//spstr为存储过程名称
            this.m_cmd.CommandType = CommandType.StoredProcedure;
            IDataParameter[] parameters = {
                new SqlParameter("@peo_id", SqlDbType.NVarChar,30) ,
                new SqlParameter("@acc", SqlDbType.NVarChar,6) ,
                new SqlParameter("@p_date_from", SqlDbType.NVarChar,30) ,
                new SqlParameter("@p_date_to", SqlDbType.NVarChar,30)// 返回值
            };
            // 设置参数类型
            parameters[0].Value = peo_id;        // 设置为输出参数
            parameters[1].Value = acc;                   // 给输入参数赋值
            parameters[2].Value = p_date_from;
            parameters[3].Value = p_date_to;// 设置为返回值
            // 添加参数
            this.m_cmd.Parameters.Add(parameters[0]);
            this.m_cmd.Parameters.Add(parameters[1]);
            this.m_cmd.Parameters.Add(parameters[2]);
            this.m_cmd.Parameters.Add(parameters[3]);

            this.m_da = new SqlDataAdapter();
            this.m_cb = new SqlCommandBuilder(this.m_da);
            this.m_da.SelectCommand = this.m_cmd;
            this.m_table.Reset();
            this.m_da.Fill(this.m_table);
            this.rowindex = -1;
            return this.rowcount;
        }
        public IDataReader Open_dr(string sqlstr)
        {
            if (this.m_conn.State == ConnectionState.Closed)
            {
                this.m_conn.Open();
            }
            if ((this.m_dr != null) && !this.m_dr.IsClosed)
            {
                this.m_dr.Close();
            }
            this.m_cmd.CommandText = sqlstr;
            this.m_dr = this.m_cmd.ExecuteReader();
            return this.m_dr;
        }

        public void RemoveAt(int i)
        {
            if ((i >= 0) && (i < this.m_table.Rows.Count))
            {
                this.m_table.Rows.RemoveAt(i);
                this.rowindex = -1;
            }
        }

        public int Update()
        {
            this.m_idname = "";
            return this.m_da.Update(this.m_table);
        }

        public int Update(string idname)
        {
            this.m_idname = idname;
            int num = this.m_da.Update(this.m_table);
            this.m_table.AcceptChanges();
            return num;
        }


        public object this[string sname]
        {
            get
            {
                return this.m_table.Rows[this.rowindex][sname];
            }
            set
            {
                this.m_table.Rows[this.rowindex][sname] = value;
                object obj2 = new object();
            }
        }

        public int rowcount
        {
            get
            {
                return this.m_table.Rows.Count;
            }
        }

        public int rowindex
        {
            get
            {
                return this.m_rowindex;
            }
            set
            {
                if ((value < this.m_table.Rows.Count) && (value >= 0))
                {
                    this.m_rowindex = value;
                }
                else
                {
                    this.m_rowindex = -1;
                }
            }
        }

        public DataRowCollection Rows
        {
            get
            {
                return this.m_table.Rows;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">显示释放(手动指定释放资源)</param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.Close();
                    if (null != m_conn)
                    {
                        m_conn.Dispose();
                    }
                    if (null != m_da)
                    {
                        m_da.Dispose();
                    }
                    if (null != m_dr)
                    {
                        m_dr.Dispose();
                    }
                    if (null != m_ds)
                    {
                        m_ds.Dispose();
                    }
                    if (null != m_table)
                    {
                        m_table.Dispose();
                    }
                    if (null != m_cb)
                    {
                        m_cb.Dispose();
                    }
                    if (null != m_cmd)
                    {
                        m_cmd.Dispose();
                    }
                }
                disposed = true;
            }
        }

    }
}


