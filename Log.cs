using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Data.SQLite;

namespace log
{

    class Log
    {
        public class Record
        {
            [JsonProperty(PropertyName = "created_at")]
            public DateTime сreated_at { get; set; }
            [JsonProperty(PropertyName = "first_name")]
            public string first_name { get; set; }
            [JsonProperty(PropertyName = "message")]
            public string message { get; set; }
            [JsonProperty(PropertyName = "second_name")]
            public string second_name { get; set; }
            [JsonProperty(PropertyName = "user_id")]
            public Int32 user_id { get; set; }
        }
        public class ImportData
        {
            [JsonProperty(PropertyName = "error")]
            public string error { get; set; }

            [JsonProperty(PropertyName = "logs")]
            public List<Record> logs { get; set; }
        }
        //быстрая сортировка (Хоара)
        private void qs(List<Record> items, int left, int right)
        {
            int i, j;
            Record x, y;
            i = left; j = right;
            x = items[(left + right) / 2];
            do
            {
                while ((items[i].сreated_at < x.сreated_at) && (i < right)) i++;
                while ((x.сreated_at < items[j].сreated_at) && (j > left)) j--;
                if (i <= j)
                {
                    y = items[i];
                    items[i] = items[j];
                    items[j] = y;
                    i++; j--;
                }
            } while (i <= j);
            if (left < j) qs(items, left, j);
            if (i < right) qs(items, i, right);
        }

        public bool ReadLog(int y, int m,int d)
        {
            string link = String.Format("http://www.dsdev.tech/logs/{0:D4}{1:D2}{2:D2}", y, m, d);  //подготовить запрос
            string response;
            try
            {
                using (var client = new WebClient())
                {
                    response = client.DownloadString(link); //получить логи с сайта
                }
                ImportData imp = JsonConvert.DeserializeObject<ImportData>(response);   //десериализация json
                qs(imp.logs, 0, imp.logs.Count - 1);    //сортировка

                if (imp.error != "")    //проверка ошибки
                    return false;
                string dbname = String.Format("{0:D4}{1:D2}{2:D2}.db", y, m, d);    //сформировать имя базы данных
                if (File.Exists(dbname))    //если она существует, удалить
                    File.Delete(dbname);
                SQLiteConnection.CreateFile(dbname); // создать базу данных, по указанному пути содаётся пустой файл базы данных
                using (SQLiteConnection Connect = new SQLiteConnection(String.Format("Data Source={0}; Version=3;", dbname))) // в строке указывается к какой базе подключаемся
                {
                    // строка запроса, который надо будет выполнить
                    //Создание таблицы
                    string commandText = "CREATE TABLE IF NOT EXISTS [dbLog] ( [id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, [created_at] DATETIME, [first_name] TEXT,[message] TEXT,[second_name] TEXT,[user_id] INTEGER)"; // создать таблицу, если её нет
                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                    Connect.Open(); // открыть соединение
                    Command.ExecuteNonQuery(); // выполнить запрос
                    foreach (var item in imp.logs)  //Заполнение БД
                    {
                        string msg = item.message.Replace("\"", "\"\"");    //Экранирование кавычек в сообщении
                        //сформировать команду на добавление записи
                        string cmd = string.Format("INSERT INTO 'dbLog' ('created_at', 'first_name', 'message', 'second_name', 'user_id') VALUES (\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\");", item.сreated_at.ToString("yyyy-MM-dd HH:mm:ss"),item.first_name,msg,item.second_name,item.user_id);
                        SQLiteCommand command = new SQLiteCommand(cmd, Connect);    //добавить дапись
                        command.ExecuteNonQuery();  // выполнить запрос
                    }
                    Connect.Close(); // закрыть соединение
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
