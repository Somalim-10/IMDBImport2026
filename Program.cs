using IMDBImport;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

Console.WriteLine("IMDB Import");

Stopwatch stopwatch = Stopwatch.StartNew();
List<Title_Model> movies = new List<Title_Model>();

Console.WriteLine("Reading titles from file...");
foreach (string movie in File.ReadLines(@"C:\Temp\title.basics.tsv\title.basics.tsv").Skip(1).Take(100000))
{
    string[] parts = movie.Split('\t');
    if (parts.Length == 9)
    {
        movies.Add(new Title_Model(parts));
    }
    else
    {
        Console.WriteLine("Invalid line: " + movie);
    }
}

foreach (Title_Model movie in movies)
{
    Console.WriteLine(movie);
}

stopwatch.Stop();
Console.WriteLine("Elapsed milliseconds to read from file: " + stopwatch.ElapsedMilliseconds); 

stopwatch.Start();

//NormalInserter inserter = new NormalInserter();
BulkInserter inserter = new BulkInserter();
SqlConnection sqlConn = new SqlConnection(
    "Server=localhost;Database=IMDB;Integrated security=True;" +
    "Trusted_Connection=True;TrustServerCertificate=True;");
sqlConn.Open(); 
inserter.InsertTitles(movies, sqlConn);
//inserter.InsertGenres(movies, sqlConn); 

sqlConn.Close();
stopwatch.Stop();
Console.WriteLine("Elapsed milliseconds to Insert Data: " + stopwatch.ElapsedMilliseconds); 