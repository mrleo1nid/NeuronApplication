using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBotBase.Storage
{
    public class LearnDBRepository
    {
        private string StorageBdName = "C:/Users/Леонид/Desktop/Storage/Learn.db";

        public LearnRow GetStorageRowFromID(int id)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", StorageBdName));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM LearnTable  WHERE ID =@ID ", connection);
            command.Parameters.Add(new SQLiteParameter("@ID", id));
            var reader = command.ExecuteReader();
            LearnRow storage = new LearnRow();
            storage.ID = -1;
            while (reader.Read())
            {
                storage.ID = Convert.ToInt32(reader[0]);
                storage.Request = (string)reader[1];
                storage.Responce = (string)reader[2];
            }
            connection.Close();
            return storage;
        }
        public List<LearnRow> GetStorageRow()
        {
            List<LearnRow> list = new List<LearnRow>();
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", StorageBdName));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM LearnTable", connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                LearnRow storage = new LearnRow();
                storage.ID = -1;
                storage.ID = Convert.ToInt32(reader[0]);
                storage.Request = (string)reader[1];
                storage.Responce = (string)reader[2];
                list.Add(storage);
            }
            connection.Close();
            return list;
        }

        public void AddStorageRow(string request,string responce)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", StorageBdName));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("INSERT INTO LearnTable ('Request','Responce') VALUES (@Request,@Responce)", connection);
            command.Parameters.Add(new SQLiteParameter("@Request", request));
            command.Parameters.Add(new SQLiteParameter("@Responce", responce));
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
