using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBotBase.Storage
{
    public class StorageDBRepository
    {
        private string StorageBdName = "C:/Users/Леонид/Desktop/Storage/Storage.db";

        public StorageRow GetStorageRowFromID(int id)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", StorageBdName));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM StorageTable  WHERE ID =@ID ", connection);
            command.Parameters.Add(new SQLiteParameter("@ID", id));
            var reader = command.ExecuteReader();
            StorageRow storage = new StorageRow();
            storage.ID = -1;
            while (reader.Read())
            {
                storage.ID = Convert.ToInt32(reader[0]);
                storage.Text = (string)reader[1];
            }
            connection.Close();
            return storage;
        }
        public StorageRow GetStorageRowFromText(string text)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", StorageBdName));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM StorageTable WHERE Text =@Text ", connection);
            command.Parameters.Add(new SQLiteParameter("@Text", text));
            var reader = command.ExecuteReader();
            StorageRow storage = new StorageRow();
            storage.ID = -1;
            while (reader.Read())
            {
                storage.ID =   Convert.ToInt32(reader[0]);
                storage.Text = (string)reader[1];
            }
            connection.Close();
            return storage;
        }

        public void AddStorageRow(string text)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", StorageBdName));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("INSERT OR IGNORE INTO StorageTable ('Text') VALUES (@Text)", connection);
            command.Parameters.Add(new SQLiteParameter("@Text", text));
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
