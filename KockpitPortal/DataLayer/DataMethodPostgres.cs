using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace KockpitPortal.DataAccessLayer
{
    public class DataMethodPostgres
    {
        NpgsqlConnection _sqlCon;
        DataSet _localDS;
        string _strCon;

        public NpgsqlConnection sqlCon { get { return _sqlCon; } }

        public DataMethodPostgres()
        {

        }

        public DataMethodPostgres(string strCon)
        {
            _sqlCon = new NpgsqlConnection(strCon);
            _strCon = strCon;
        }

        public DataMethodPostgres(string strCon, DataSet localDS)
        {
            _sqlCon = new NpgsqlConnection(strCon);
            _localDS = localDS;
            _strCon = strCon;
        }

        public bool ExecuteNonQueryCommand(string strCommandText, NpgsqlParameter[] sqlpara, bool IsCommand = true)
        {
            int nretval = 0;

            using (NpgsqlCommand localCmd = _sqlCon.CreateCommand())
            {
                localCmd.CommandType = !IsCommand ? CommandType.StoredProcedure : CommandType.Text;
                localCmd.CommandText = strCommandText;
                localCmd.CommandTimeout = 60 * 60;

                if (sqlpara != null)
                    localCmd.Parameters.AddRange(sqlpara);

                try
                {
                    if (_sqlCon.State == ConnectionState.Closed)
                        _sqlCon.Open();

                    nretval = localCmd.ExecuteNonQuery();
                }
                catch (Exception ee)
                {
                    throw new Exception("Execute nonQuery Exception" + ee.Message);
                }
                finally
                {
                    _sqlCon.Close();
                }


                return (nretval >= 0 || nretval == -1);
            }

        }

        public bool ExecuteSelectCommand(string strCommandText, NpgsqlParameter[] sqlpara, string strTableAlias, bool isText = true)
        {
            bool lRetval = true;

            if (_localDS.Tables.Contains(strTableAlias))
                _localDS.Tables.Remove(strTableAlias);

            using (NpgsqlCommand localCmd = _sqlCon.CreateCommand())
            {
                localCmd.CommandType = isText ? CommandType.Text : CommandType.StoredProcedure;
                localCmd.CommandText = strCommandText;

                if (sqlpara != null)
                    localCmd.Parameters.AddRange(sqlpara);

                try
                {
                    if (_sqlCon.State == ConnectionState.Closed)
                        _sqlCon.Open();

                    using (NpgsqlDataAdapter localAdapter = new NpgsqlDataAdapter(localCmd))
                    {
                        localAdapter.Fill(_localDS, strTableAlias);
                    }
                }
                catch (Exception ee)
                {
                    lRetval = false;
                    throw new Exception("Selection Exception" + ee.Message);
                }
                finally
                {
                    _sqlCon.Close();
                }

                return lRetval;
            }
        }


        public bool ExecuteSelectReaderCommand(string strCommandText, NpgsqlParameter[] sqlpara, string strTableAlias, bool isText = true)
        {
            bool lRetval = true;

            if (_localDS.Tables.Contains(strTableAlias))
                _localDS.Tables.Remove(strTableAlias);

            using (NpgsqlCommand localCmd = _sqlCon.CreateCommand())
            {
                localCmd.CommandType = isText ? CommandType.Text : CommandType.StoredProcedure;
                localCmd.CommandText = strCommandText;

                if (sqlpara != null)
                    localCmd.Parameters.AddRange(sqlpara);

                try
                {
                    if (_sqlCon.State == ConnectionState.Closed)
                        _sqlCon.Open();

                    using (NpgsqlDataReader localReader = localCmd.ExecuteReader())
                    {
                        //var columns = new List<string>();
                        DataTable dt = new DataTable(strTableAlias);
                        for (int i = 0; i < localReader.FieldCount; i++)
                        {
                            //columns.Add(localReader.GetName(i));
                            dt.Columns.Add(localReader.GetName(i));
                        }
                        //if(columns.Distinct().Count() != columns.Count())
                        //{
                        //    throw new Exception("Duplicate column names in resultset.");
                        //}
                        while (localReader.Read())
                        {
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                dr[i] = localReader[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        _localDS.Tables.Add(dt);
                        //localAdapter.Fill(_localDS, strTableAlias);
                    }
                }
                catch (Exception ee)
                {
                    lRetval = false;
                    throw new Exception("Selection Exception: " + ee.Message);
                }
                finally
                {
                    _sqlCon.Close();
                }

                return lRetval;
            }
        }



        public bool ExecuteSelectCommand(string strCommandText, string strTableAlias)
        {
            return ExecuteSelectCommand(strCommandText, null, strTableAlias);
        }

        public bool ExecuteSelectQuery(string strCommandText, string strTableAlias)
        {
            return ExecuteSelectCommand(strCommandText, null, strTableAlias, true);
        }

        public bool ExecuteNonQueryCommand(string strCommandText)
        {
            return ExecuteNonQueryCommand(strCommandText, null, true);
        }
    }
}
