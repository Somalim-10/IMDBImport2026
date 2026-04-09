using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDBImport
{
    public class NormalInserter
    {

        public void InsertTitles(List<Title_Model> titles, SqlConnection sqlConn)
        {
            foreach (Title_Model movie in titles)
            {
                string query = "INSERT INTO Titles (" +
                    "TConst, " +
                    "TitleType, " +
                    "PrimaryTitle, " +
                    "OriginalTitle, " +
                    "IsAdult, " +
                    "StartYear, " +
                    "EndYear, " +
                    "RuntimeMinutes) " +
                               "VALUES (" +
                               movie.TConst + ", " +
                               "'" + movie.TitleType + "', " +
                               "'" + movie.PrimaryTitle.Replace("'", "''") + "', " +
                               "'" + movie.OriginalTitle.Replace("'", "''") + "', " +
                               "'" + movie.IsAdult + "', " +
                               ConvertIntToString(movie.StartYear) + ", " +
                               ConvertIntToString(movie.EndYear) + ", " +
                               ConvertIntToString(movie.RuntimeMinutes) + ")";

                try
                {
                    SqlCommand sqlComm = new SqlCommand(query, sqlConn);
                    sqlComm.ExecuteNonQuery();
                }
                catch (SqlException sqlex)
                {
                    Console.WriteLine("Error inserting query:\r\n" + query);
                }
            }


        }
        private string ConvertIntToString(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "NULL";
        }
    }
}