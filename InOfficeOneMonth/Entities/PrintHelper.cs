using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace InOfficeOneMonth.Entities
{
    /// <summary>
    /// Вспомогательный класс для создания отчётного документа
    /// </summary>
    public class PrintHelper
    {
        Office office;
        public PrintHelper(Office of)
        {
            office = of;
        }

        //создаёт начальную HTML страницу
        public void CreateHtml()
        {
            string filename = @"Output\Employment.html";
            StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd \">");
                    sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                    sb.AppendLine("<head>");
                    sb.AppendLine("<meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" />");
                    sb.AppendLine("<title>Employment Report</title>");
                    sb.AppendLine("<LINK href='table.css' title='Table.CSS' rel='stylesheet' type='text/css'>");
                    sb.AppendLine("<script type='text/javascript' src='jquery-2.1.4.min.js'></script>");
                    sb.AppendLine("<script type='text/javascript' src='EmploymentData.js'></script>");
                    sb.AppendLine("<script type='text/javascript' src='table_script.js'></script>");
                    sb.AppendLine("<script type='text/javascript' src='createTable.js'></script>");
                    sb.AppendLine("</head>");
                    sb.AppendLine("<body>");
                    sb.AppendLine("<div id='cont'>");
                    sb.AppendLine("</body>");
                    sb.AppendLine("</html>");
                    File.WriteAllText(filename,sb.ToString(),Encoding.UTF8);
        }

        //создаёт Javascript файл и записывает в него данные Office.Employment в формате JSON
        public void CreateDataScript()
        {
            string filename = @"Output\EmploymentData.js";
            FileStream fstream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
            StringBuilder sb = new StringBuilder();
            using (StreamWriter sw = new StreamWriter(fstream))
            {
                Encoding enc = sw.Encoding;
                sb.Append(" var jsonData = {'Employment':{'Weeks':[");
                for (int i = 0; i < 4; i++)
                {
                    sb.Append("{ 'wn':" + i + ",'Employees':[");
                    foreach (KeyValuePair<string, Report> pair in office.Employment)
                    {
                        sb.Append("{'Name':'" + pair.Key + "','Responsibility':[");
                        for (int j = 0; j < 5; j++)
                        {
                            sb.Append("[");
                            for (int k = 0; k < 8; k++)
                            {
                                sb.Append((int)pair.Value.DischargeDuties[i, j, k]);
                                sb.Append(",");
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append("],");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append("],'Weeksalary':" + pair.Value.WeekSalary[i] + "},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(" ]},");
                }
                sb.Append(" ]}}");
                sw.Write(sb.ToString());
            }
            fstream.Close();
        }
    }
}
