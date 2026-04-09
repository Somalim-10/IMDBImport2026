using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDBImport
{
    public class BulkInserter : IInserter
    {
        public void InsertTitles(List<Title_Model> titles, SqlConnection sqlConn)
        {
            DataTable titleTable = new DataTable();
            titleTable.Columns.Add("TConst", typeof(string));
            titleTable.Columns.Add("TitleType", typeof(string));
            titleTable.Columns.Add("PrimaryTitle", typeof(string));
            titleTable.Columns.Add("OriginalTitle", typeof(string));
            titleTable.Columns.Add("IsAdult", typeof(bool));
            titleTable.Columns.Add("StartYear", typeof(int));
            titleTable.Columns.Add("EndYear", typeof(int));
            titleTable.Columns.Add("RuntimeMinutes", typeof(int));

            foreach (Title_Model movie in titles)
            {
                titleTable.Rows.Add(movie.TConst,
                    movie.TitleType,
                    movie.PrimaryTitle,
                    movie.OriginalTitle,
                    movie.IsAdult,
                    movie.StartYear ?? (object)DBNull.Value,
                    movie.EndYear ?? (object)DBNull.Value,
                    movie.RuntimeMinutes ?? (object)DBNull.Value);
            }
            SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn)
            {
                DestinationTableName = "Titles"
            };

            bulkCopy.WriteToServer(titleTable);
        }
    }
}