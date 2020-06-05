using System;
using System.Data;
using System.Linq;

using CirnoLib.Jass;

namespace CirnoLib.Format
{
    public sealed class SLK : DataTable, IArrayable
    {
        public DataRow this[string Column, string Key] {
            get {
                return (from DataRow Row in Rows
                        where Row[Column].ToString() == Key
                        select Row)?.First();
            }
        }

        public DataRow this[string ID] {
            get {
                return (from DataRow Row in Rows
                        where (Row["itemID"] as int[])?[0] == ID.Numberize()
                        select Row)?.First();
            }
        }

        public DataRow this[byte[] ID] {
            get {
                return (from DataRow Row in Rows
                        where (Row["itemID"] as int[])?[0] == ID.Numberize()
                        select Row)?.First();
            }
        }

        public static SLK Parse(byte[] data)
        {
            SLK slk = new SLK();
            DataRow row = null;
            string[] lines = data.GetString().Replace('\r','\n').Replace("\n\n", "\n").Split('\n');
            bool isColumnsLine = false;
            Type type = typeof(object);
            int index = 0;
            foreach (var line in lines)
            {
                string[] parts = line.Split(';');
                
                switch (parts[0])
                {
                    default: continue;
                    case "C": break;
                    case "E": return slk;
                }

                for (int i = 1; i < parts.Length; i++)
                {
                    switch (parts[i][0])
                    {
                        case 'Y':
                            isColumnsLine = Convert.ToInt32(parts[i].Substring(1)) == 1;
                            row = slk.Rows.Add();
                            break;
                        case 'X':
                            index = Convert.ToInt32(parts[i].Substring(1));
                            break;
                        case 'K':
                            string value = parts[i].Substring(1);
                            int? intValue = null;
                            double? doubleValue = null;
                            if (value[0] == '"') value = value.Substring(1, value.Length - 2);
                            else if (value.IndexOf('.') != -1) doubleValue = Convert.ToDouble(value);
                            else intValue = Convert.ToInt32(value);

                            if (isColumnsLine)
                            {
                                slk.Columns.Add(value, type);
                                row[index - 1] = value;
                            }
                            else if (intValue != null)
                                row[index - 1] = intValue;
                            else if (doubleValue != null)
                                row[index - 1] = doubleValue;
                            else
                            {
                                int[] list = value.GetNumberizeList();
                                if (list == null) row[index - 1] = value;
                                else row[index - 1] = list;
                            }                                
                            break;
                        default: throw new Exception("읽을 수 없는 데이터 입니다.");
                    }
                }
            }
            return slk;
        }

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write($"ID;PWXL;N;E\nB;X{Columns.Count};Y{Rows.Count};D0\n", false);
                for (int i = 0; i < Rows.Count; i++)
                {
                    bool isFirstColumn = true;
                    for (int j = 0; j < Columns.Count; j++)
                    {
                        object cell = Rows[i][j];
                        if (cell is DBNull) continue;
                        bs.Write("C;", false);
                        if (isFirstColumn)
                        {
                            isFirstColumn = false;
                            bs.Write($"Y{i + 1};", false);
                        }
                        bs.Write($"X{j + 1};K", false);
                        switch (cell.GetType().ToString())
                        {
                            case "System.Byte[]":
                                bs.Write('"');
                                bs.Write(cell as byte[]);
                                bs.Write('"');
                                break;
                            case "System.String":
                                bs.Write($"\"{cell as string}\"", false); 
                                break;
                            case "System.Int32":
                            case "System.Double":
                                bs.Write(cell.ToString(), false);
                                break;
                            case "System.Int32[]":
                                bs.Write('"');
                                bs.Write((cell as int[]).GetRawCodeList());
                                bs.Write('"');
                                break;
                        }
                        bs.Write('\n');
                    }
                }
                bs.Write('E');
                return bs.ToArray();
            }
        }
    }
}
