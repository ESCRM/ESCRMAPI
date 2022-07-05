using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// SQL Communications Layer
    /// Used as a base for SQLDB class in IDS.EBSTCRM.Base project
    /// </summary>
    public class SQLBase : IDisposable {

        #region Declarations

        SqlCommand cmd = new SqlCommand();
        SqlConnection conn;
        string conStr;
        internal System.Data.SqlClient.SqlErrorCollection SqlErrorMessages;
        internal string errorMessage;

        #endregion

        #region Methods

        private void init(string sqlConnectionString)
        {
            try
            {
                conStr = sqlConnectionString;
                conn = new SqlConnection(sqlConnectionString);
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandTimeout = 600;
                reset();
            }
            catch (SqlException sqle)
            {
                SqlErrorMessages = sqle.Errors;
                throw sqle;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                throw e;
            }
        }

        /// <summary>
        /// Dispose of the Connection
        /// </summary>
        public void Dispose() {
            try {
                cmd.Dispose();
                cmd = null;
                conn.Close();
                conn.Dispose();
            } catch (SqlException sqle) {
                SqlErrorMessages = sqle.Errors;
                throw sqle;
            } catch (Exception e) {
                errorMessage = e.Message;
                throw e;
            } finally {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Resets the connection, making it ready for a new Transaction or Query
        /// </summary>
        public void reset() {
            cmd.Parameters.Clear();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "";
        }

        /// <summary>
        /// Connection String
        /// </summary>
        public string ConnectionString {
            get { return conStr; }
        }

        /// <summary>
        /// CommandText to Execute - can be SQL or Stored Procedure
        /// </summary>
        public string commandText {
            get { return cmd.CommandText; }
            set { cmd.CommandText = value; }
        }

        /// <summary>
        /// Sets the CommandType (SQL or Stored Procedure)
        /// </summary>
        public System.Data.CommandType commandType {
            get { return cmd.CommandType; }
            set { cmd.CommandType = value; }
        }

        /// <summary>
        /// Paramaters to Add, when executing a Stored Procedure
        /// </summary>
        public System.Data.SqlClient.SqlParameterCollection parameters {
            get { return cmd.Parameters; }
        }

        /// <summary>
        /// Executes the SQL or Stored Procedure and returns a DataReader
        /// </summary>
        public System.Data.SqlClient.SqlDataReader executeReader {
            get { return cmd.ExecuteReader(); }
        }

        /// <summary>
        /// Executes the SQL / Stored Procedure
        /// </summary>
        public void execute() {
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL / Stored Procedure, ESCRM-163
        /// </summary>
        public int _execute()
        {
            int result = cmd.ExecuteNonQuery();

            return result;
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new SQL connection
        /// </summary>
        /// <param name="server">Host or IP address to server</param>
        /// <param name="initialDatabase">Database name</param>
        /// <param name="username">Username to database</param>
        /// <param name="password">Password to database</param>
        /// <param name="pooling">Use Pooling with connection</param>
        public SQLBase(string server, string initialDatabase, string username, string password, bool pooling) {
            init("Data Source=" + server + ";Initial Catalog=" + initialDatabase + ";Uid=" + username + ";pwd=" + password + ";pooling=" + pooling.ToString() + ";");
        }

        /// <summary>
        /// Constructs a new SQL connection
        /// </summary>
        /// <param name="server">Host or IP address to server</param>
        /// <param name="initialDatabase">Database name</param>
        /// <param name="username">Username to database</param>
        /// <param name="password">Password to database</param>
        public SQLBase(string server, string initialDatabase, string username, string password) {
            init("Data Source=" + server + ";Initial Catalog=" + initialDatabase + ";Uid=" + username + ";pwd=" + password + ";pooling=False;");
        }

        /// <summary>
        /// Constructs a new SQL connection
        /// </summary>
        /// <param name="sqlConnectionString">ConnectionString, i.e. from Web.Config or App.Config</param>
        public SQLBase(string sqlConnectionString) {
            init(sqlConnectionString);
        }
        #endregion

    }
}