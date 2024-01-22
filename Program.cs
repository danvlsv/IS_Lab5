using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;


namespace GPU_DB
{
    class Program
    {

        static SQLiteConnection connection;
        static SQLiteCommand command;

        static string entString = "";

        static void Main(string[] args)
        {
            if (Connect("gpu.db"))
            {
                //Console.WriteLine("Connected");
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT * FROM entities";
                DataTable entities = new DataTable();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                adapter.Fill(entities);
                foreach (DataRow row in entities.Rows)
                {
                    entString += row.Field<long>("id") + ". " + row.Field<string>("name") + "\n";
                }
                while (MenuChoice() != 0) { }
            }
        }



        static public bool Connect(string fileName)
        {
            try
            {
                connection = new SQLiteConnection("Data Source=" + fileName + ";Version=3; FailIfMissing=False");
                connection.Open();
                
                return true;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Ошибка доступа к базе данных. Исключение: {ex.Message}");
                return false;
            }
        }

        static private int MenuChoice()
        {
            Console.WriteLine("\n--------------------------\n");
            Console.WriteLine("Список понятий:\n");
            Console.WriteLine(entString);

            Console.WriteLine("Список команд:\n\n* relations <id> - список отношений субъекта\n* change <id сущности> <id свойства> <новое значение> - список отношений субъекта\n* exit \n");

            Console.Write("Ваша команда: ");
            string[] str = Console.ReadLine().Split();

            switch (str[0])
            {
                case "relations":
                    Console.WriteLine();
                    PrintRelations(str[1]);
                    return 1;
                case "change":
                    Console.WriteLine();
                    ChangeProperty(str[1], str[2], str[3]);
                    return 2;
                case "exit":
                    return 0;
            }

            Console.WriteLine("\nОшибки в запросе\n");
            return -1;
            
        }

        static private void PrintRelations(string id)
        {
            command.CommandText = "SELECT * FROM structure where entity = " + id;
            DataTable structure = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(structure);

            foreach (DataRow row in structure.Rows)
            {
                Console.WriteLine(PrintRow(row));
            }
            if (structure.Rows.Count == 0)
            {
                DataTable tempTable = new DataTable();
                command.CommandText = "SELECT * FROM structure where entity_2 = " + id;
                SQLiteDataAdapter adapter1 = new SQLiteDataAdapter(command);
                adapter1.Fill(tempTable);
                if (tempTable.Rows.Count > 0)
                {
                    Console.WriteLine("Данная сущность не имеет отношений или является свойством");
                }
                else { Console.WriteLine("Сущность с таким id не существует"); }
            }
            Console.WriteLine("\n\nНажмите любую клавишу, чтобы продолжить.");
            Console.ReadKey();
            Console.WriteLine("\n");
        }

        static private string PrintRow(DataRow row)
        {
            DataTable entity1 = new DataTable();
            DataTable relations = new DataTable();
            DataTable entity2 = new DataTable();
            DataTable property = new DataTable();
            string rowStr = "";


            command.CommandText = $"SELECT name FROM entities where id = {row.Field<long>("entity")}";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(entity1);

            foreach (DataRow i in entity1.Rows)
            {
                rowStr += "'" + i.Field<string>("name") + "' ";
            }
            
            
            command.CommandText = $"SELECT rel_name FROM relations where id_rel = {row.Field<long>("relation_id")}";
            adapter = new SQLiteDataAdapter(command);
            adapter.Fill(relations);

            foreach (DataRow i in relations.Rows)
            {
                rowStr += i.Field<string>("rel_name") + " ";
            }

            

            command.CommandText = $"SELECT name FROM entities where id = {row.Field<long>("entity_2")}";
            adapter = new SQLiteDataAdapter(command);
            adapter.Fill(entity2);

            foreach (DataRow i in entity2.Rows)
            {
                rowStr += "'" + i.Field<string>("name") + "' ";
            }
            
            if (row.Field<long>("relation_id")==1)
            {
                rowStr += "имеет значение: ";
                command.CommandText = $"SELECT name FROM entities where id = {row.Field<long>("value")}";
                adapter = new SQLiteDataAdapter(command);
                adapter.Fill(property);

                foreach (DataRow i in property.Rows)
                {
                    rowStr += "'" + i.Field<string>("name") + "' ";
                }
            }
            return rowStr;
        }


        static private void ChangeProperty(string id, string id_prop, string value)
        {
            command.CommandText = $"SELECT * FROM structure where entity = {id} and relation_id=1";

            DataTable structure = new DataTable();

            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(structure);


            long newValue = long.Parse(value);


            foreach (DataRow row in structure.Rows)
            {
                if (row.Field<long>("entity_2") == long.Parse(id_prop))
                {
                    long temp = row.Field<long>("value");
                    int flag = 0;
                    if (new long [] { 13, 14 }.Contains(temp) && new long[] { 13, 14 }.Contains(newValue))
                    {
                        flag++;
                    }
                    if (new long[] { 11,12 }.Contains(temp) && new long[] { 11,12 }.Contains(newValue))
                    {
                        flag++;
                    }
                    if (new long[] { 15,16 }.Contains(temp) && new long[] { 15,16 }.Contains(newValue))
                    {
                        flag++;
                    }
                    if (flag>0)
                    {
                        command.CommandText = $"Update structure Set value = $value where entity_2 = {row.Field<long>("entity_2")}";
                        command.Parameters.AddWithValue("$value", newValue);
                        command.ExecuteNonQuery();
                        Console.WriteLine("Значение изменено");
                    }
                    else
                    {
                        Console.WriteLine("Нельзя задать такое значение ");
                    }
                }

            }

            if (structure.Rows.Count == 0)
            {
                Console.WriteLine("Данный элемент сети не имеет такого свойства");
            }
            Console.WriteLine("\n\nНажмите любую клавишу, чтобы продолжить.");
            Console.ReadKey();
            Console.WriteLine("\n");
        }
    }
}