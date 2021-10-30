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
               // var filePath = Server.MapPath("~/Data/populationdata1950-2020.csv");

                
            }
            catch (Exception ex)
            {
            }
            return View();
        }


        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Data"), _FileName);
                    file.SaveAs(filePath);


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
                                dt.Columns.Add(item + "." + k.ToString("00"));
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
                    ToCSV(dt, Server.MapPath("~/OutputCSV/"+_FileName));

                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch(Exception ex)
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }

        public void ToCSV(DataTable dtDataTable, string strFilePath)
        {
            System.IO.File.Create(strFilePath).Close();
            

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

        public ActionResult Transpose()
        {
            //var filePath = Server.MapPath("~/Data/outputses.csv");
            //DataTable dt = new DataTable();
            //dt = ReadCsvFile(filePath);

            //DataTable trsnpose = new DataTable();
            //trsnpose=GenerateTransposedTable(dt);
            //Random _random = new Random();
            //ToCSV(trsnpose, Server.MapPath("~/Data/outputses.csv"));

            return View();

        }
        [HttpPost]
        public ActionResult Transpose(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Data"), _FileName);
                    file.SaveAs(filePath);

                    DataTable dt = new DataTable();
                    dt = ReadCsvFile(filePath);

                    DataTable trsnpose = new DataTable();
                    trsnpose = GenerateTransposedTable(dt);
                    Random _random = new Random();
                    ToCSV(trsnpose, Server.MapPath("~/TransposeCSV/transpose_"+_FileName));

                }

            }
            catch (Exception ex)
            {

            }
            return View();
        }

        public DataTable ReadCsvFile(string FileSaveWithPath)
        {

            DataTable dtCsv = new DataTable();
            string Fulltext;
            {
                using (StreamReader sr = new StreamReader(FileSaveWithPath))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString(); //read full file text  
                        string[] rows = Fulltext.Split('\n'); //split full file text into rows  
                        for (int i = 0; i < rows.Count() - 1; i++)
                        {
                            string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values  
                            {
                                if (i == 0)
                                {
                                    for (int j = 0; j < rowValues.Count(); j++)
                                    {
                                        dtCsv.Columns.Add(rowValues[j]); //add headers  
                                    }
                                }
                                else
                                {
                                    DataRow dr = dtCsv.NewRow();
                                    for (int k = 0; k < rowValues.Count(); k++)
                                    {
                                        dr[k] = rowValues[k].ToString();
                                    }
                                    dtCsv.Rows.Add(dr); //add other rows  
                                }
                            }
                        }
                    }
                }
            }
            return dtCsv;
        }

        private DataTable GenerateTransposedTable(DataTable inputTable)
        {
            DataTable outputTable = new DataTable();

            // Add columns by looping rows

            // Header row's first column is same as in inputTable
            outputTable.Columns.Add(inputTable.Columns[0].ColumnName.ToString());

            // Header row's second column onwards, 'inputTable's first column taken
            foreach (DataRow inRow in inputTable.Rows)
            {
                string newColName = inRow[0].ToString();
                outputTable.Columns.Add(newColName);
            }

            // Add rows by looping columns        
            for (int rCount = 1; rCount <= inputTable.Columns.Count - 1; rCount++)
            {
                DataRow newRow = outputTable.NewRow();

                // First column is inputTable's Header row's second column
                newRow[0] = inputTable.Columns[rCount].ColumnName.ToString();
                for (int cCount = 0; cCount <= inputTable.Rows.Count - 1; cCount++)
                {
                    string colValue = inputTable.Rows[cCount][rCount].ToString();
                    newRow[cCount + 1] = colValue;
                }
                outputTable.Rows.Add(newRow);
            }

            return outputTable;
        }
        public static bool IsDoubleRealNumber(string valueToTest)
        {
            if (double.TryParse(valueToTest, out double d) && !Double.IsNaN(d) && !Double.IsInfinity(d))
            {
                return true;
            }

            return false;
        }

        public ActionResult ConvertMonthlyTranspose()
        {

            return View();
        }
        [HttpPost]
        public ActionResult ConvertMonthlyTranspose(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Data"), _FileName);
                    file.SaveAs(filePath);


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
                                dt.Columns.Add(item + "." + k.ToString("00")+".01");
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
                                    nRow[firstLine[m] + "." + k.ToString("00") + ".01"] = Math.Round(prval, 2);
                                    colNo++;
                                }
                            }
                            m++;
                        }
                        dt.Rows.Add(nRow);
                    }
                    DataTable trsnpose = new DataTable();
                    trsnpose = GenerateTransposedTable(dt);
                    Random _random = new Random();
                    ToCSV(trsnpose, Server.MapPath("~/TransposeCSV/transpose_" + _FileName));

                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }
    }
}