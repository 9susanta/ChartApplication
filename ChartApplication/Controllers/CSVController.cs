using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace ChartApplication.Controllers
{
    public class CSVController : Controller
    {
        // GET: CSV
        private static double lastValue;
        public ActionResult Index()
        {
            try
            {
                var directory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var filePath = directory + "\\dataset.csv";

                var lines = System.IO.File.ReadAllLines(filePath);
                var firstLine = lines.First().Split(',');
                var dataLines = lines.Skip(1).ToArray();
                DataTable dt = new DataTable();
                int i = 0;
                foreach (var item in firstLine)
                {
                    if (i == 0)
                    {
                        dt.Columns.Add(item.ToLower());
                    }
                    else if (i > 0)
                    {
                        for (int k = 1; k <= 12; k++)
                        {
                            dt.Columns.Add(item + '.' + k.ToString("00"));
                        }
                    }
                    i++;
                }

                foreach (var line in dataLines)
                {
                    int colNo = 1;
                    DataRow nRow = dt.NewRow();
                    int m = 0;
                    var lvline = line.Split(',');
                    double prval = 0;
                    foreach (var item in lvline)
                    {
                        if (m == 0)
                        {
                            nRow[0] = item;
                        }
                        else if (m > 0)
                        {
                            for (int k = 1; k <= 12; k++)
                            {
                                prval = monthlyValue(item, lvline, prval, m, k);
                                nRow[firstLine[m] + "." + k.ToString("00")] = Math.Round(prval, 2);
                                colNo++;
                            }
                        }
                        m++;
                    }
                    dt.Rows.Add(nRow);
                }
                ToCSV(dt, directory + "\\output.csv");
            }
            catch (Exception ex)
            {
            }
            return View();
        }
        public void ToCSV(DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i].ToString());
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
        private double monthlyValue(string yearlyValue, string[] lvline, double prevsval, int rowNo, int curCol)
        {
            double currentVal = 0;
            double NxtVal = 0;

            double res = 0;

            double.TryParse(yearlyValue, out currentVal);

            int arrLen = lvline.Length;
            if (curCol == 1 && rowNo != arrLen - 1)
            {
                return currentVal;
            }
            else
            {
                if (rowNo == arrLen - 1)
                {
                    double.TryParse(lvline[rowNo], out NxtVal);

                    if (lastValue < NxtVal)
                    {
                        res = (NxtVal - lastValue) / 12;
                    }
                    else if (lastValue > NxtVal)
                    {
                        res = (lastValue - NxtVal) / 12;
                    }
                    if (curCol == 12)
                    {
                        return NxtVal;
                    }

                }
                else
                {
                    double.TryParse(lvline[rowNo + 1], out NxtVal);

                    if (currentVal < NxtVal)
                    {
                        res = (NxtVal - currentVal) / 12;
                    }
                    else if (currentVal > NxtVal)
                    {
                        res = (currentVal - NxtVal) / 12;
                    }
                }
                if (rowNo == arrLen - 2 && curCol == 12)
                {
                    lastValue = prevsval + res;
                }

                return (prevsval + res);
            }
        }
    }
}