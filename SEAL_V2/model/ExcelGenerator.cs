using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using SEAL_V2.db;
using io = System.IO;

namespace SEAL_V2.model
{
    class ExcelGenerator
    {
        private static DatabaseInterface db = DatabaseInterface.Instance;
        private static String excelOutputDir = io.Path.Combine(Environment.CurrentDirectory, @"output\");
        public static void createCaptureDocument(List<Software> list, Capture passedCapture)
        {
            List<Software> kbList = new List<Software>();
            list.Sort();

            //THIS SHOULD BE A SEPERATE CLASS!!!
            excel.Application app = new excel.Application();
            app.Visible = true;
            app.WindowState = excel.XlWindowState.xlMaximized;
            app.StandardFont = "Arial";

            excel.Workbook wb = app.Workbooks.Add(excel.XlWBATemplate.xlWBATWorksheet);
            excel.Worksheet ws = wb.Worksheets[1];


            ws.Range["1:1"].Cells.Orientation = excel.XlOrientation.xlUpward;
            ws.Rows["1"].Cells.Orientation = excel.XlOrientation.xlUpward;
            ws.Rows["1"].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
            ws.Rows["1"].Cells.VerticalAlignment = excel.XlHAlign.xlHAlignCenter;
            ws.Rows["1"].Cells.Font.Size = 9;
            ws.Rows["1"].Font.Bold = true;

            ws.Range["A1"].Value = "REFERENCE NUMBER";
            ws.Range["B1"].Value = "SOFTWARE NAME";
            ws.Range["C1"].Value = "ACRONYM";
            ws.Range["D1"].Value = "VERSION NUMBER";
            ws.Range["E1"].Value = "VENDOR/SUPPLIER";
            ws.Range["F1"].Value = "SOURCE(GOTS, COTS, OSS)";
            ws.Range["G1"].Value = "Non-VM Components\n(e.g., System Laptop)";
            ws.Range["H1"].Value = "Non-VM Components\n(e.g.,Cisco NEXUS)";
            ws.Range["I1"].Value = "CLASS / UNCLASS";
            ws.Range["J1"].Value = "DESCRIPTION / FUNCTION";
            ws.Range["K1"].Value = "COMMENTS";

            for (int i = 0; i < 11; i++)
            {
                ws.Range[ws.Cells[1, i + 1], ws.Cells[2, i + 1]].Merge();
                ws.Cells[1, i + 1].Borders.Weight = excel.XlBorderWeight.xlThin;
                ws.Cells[2, i + 1].Borders.Weight = excel.XlBorderWeight.xlThin;
                ws.Cells[1, i + 1].Interior.Color = excel.XlRgbColor.rgbDarkGray;
            }

            int kbCounter = 0;

            for (int i = 0; i < list.Count; i++)
            {
                Regex rgx = new Regex(@"KB\d{5}\d*", RegexOptions.IgnoreCase);

                MatchCollection matches = rgx.Matches(list[i].SoftwareVersion);

                if (matches.Count > 0)
                {
                    kbList.Add(list[i]);
                    kbCounter++;
                }
                else
                {
                    ws.Range["A" + (i + 3 - kbCounter).ToString() + ":K" + (i + 3 - kbCounter).ToString()].Cells.NumberFormat = "@";
                    ws.Range["A" + (i + 3 - kbCounter).ToString() + ":K" + (i + 3 - kbCounter).ToString()].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
                    ws.Range["A" + (i + 3 - kbCounter).ToString() + ":K" + (i + 3 - kbCounter).ToString()].Cells.VerticalAlignment = excel.XlHAlign.xlHAlignCenter;
                    ws.Rows[(i + 3 - kbCounter).ToString()].Cells.Font.Size = 10;
                    ws.Range["A" + (i + 3 - kbCounter).ToString()].Value = (i + 1 - kbCounter).ToString();
                    ws.Range["B" + (i + 3 - kbCounter).ToString()].Value = list[i].SoftwareName;
                    ws.Range["C" + (i + 3 - kbCounter).ToString()].Value = "N/A";

                    ws.Range["D" + (i + 3 - kbCounter).ToString()].Value = list[i].SoftwareVersion;
                    ws.Range["E" + (i + 3 - kbCounter).ToString()].Value = list[i].SoftwareVendor;
                    ws.Range["F" + (i + 3 - kbCounter).ToString()].Value = "N/A";
                    ws.Range["G" + (i + 3 - kbCounter).ToString()].Value = "N/A";
                    ws.Range["G" + (i + 3 - kbCounter).ToString()].Interior.Color = excel.XlRgbColor.rgbLightGray;

                    ws.Range["H" + (i + 3 - kbCounter).ToString()].Value = "N/A";

                    ws.Range["I" + (i + 3 - kbCounter).ToString()].Value = "N/A";
                    ws.Range["J" + (i + 3 - kbCounter).ToString()].Value = "N/A";
                    ws.Range["K" + (i + 3 - kbCounter).ToString()].Value = "Captured on " + db.getSystem(passedCapture.systemid).modelname;

                    ws.Range["A" + (i + 3 - kbCounter).ToString() + ":K" + (i + 3 - kbCounter).ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;
                    ws.Columns["A:I"].AutoFit();
                    ws.Columns["J:K"].ColumnWidth = 40;
                    ws.Columns["J:K"].WrapText = true;
                }
            }

            if (kbList.Count > 0)
            {
                int currentSpot = (list.Count - kbList.Count) + 3;

                ws.Range[currentSpot + ":" + currentSpot].Cells.Orientation = excel.XlOrientation.xlUpward;
                ws.Rows[currentSpot].Cells.Orientation = excel.XlOrientation.xlUpward;
                ws.Rows[currentSpot].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
                ws.Rows[currentSpot].Cells.VerticalAlignment = excel.XlHAlign.xlHAlignCenter;
                ws.Rows[currentSpot].Cells.Font.Size = 9;
                ws.Rows[currentSpot].Font.Bold = true;

                ws.Range["A" + currentSpot].Value = "REFERENCE NUMBER";
                ws.Range["B" + currentSpot].Value = "KB NAME";
                ws.Range["C" + currentSpot].Value = "ACRONYM";
                ws.Range["D" + currentSpot].Value = "VERSION NUMBER";
                ws.Range["E" + currentSpot].Value = "VENDOR/SUPPLIER";
                ws.Range["F" + currentSpot].Value = "SOURCE(GOTS, COTS, OSS)";
                ws.Range["G" + currentSpot].Value = "Non-VM Components\n(e.g., System Laptop)";
                ws.Range["H" + currentSpot].Value = "Non-VM Components\n(e.g.,Cisco NEXUS)";
                ws.Range["I" + currentSpot].Value = "CLASS / UNCLASS";
                ws.Range["J" + currentSpot].Value = "DESCRIPTION / FUNCTION";
                ws.Range["K" + currentSpot].Value = "COMMENTS";

                for (int i = 0; i < 11; i++)
                {
                    ws.Range[ws.Cells[currentSpot, i + 1], ws.Cells[currentSpot + 1, i + 1]].Merge();
                    ws.Cells[currentSpot, i + 1].Borders.Weight = excel.XlBorderWeight.xlThin;
                    ws.Cells[currentSpot + 1, i + 1].Borders.Weight = excel.XlBorderWeight.xlThin;
                    ws.Cells[currentSpot, i + 1].Interior.Color = excel.XlRgbColor.rgbDarkGray;
                }

                for (int i = 0; i < kbList.Count; i++)
                {
                    ws.Range["A" + (i + currentSpot + 2).ToString() + ":K" + (i + currentSpot + 2).ToString()].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
                    ws.Range["A" + (i + currentSpot + 2).ToString() + ":K" + (i + currentSpot + 2).ToString()].Cells.VerticalAlignment = excel.XlHAlign.xlHAlignCenter;
                    ws.Rows[(i + currentSpot + 2).ToString()].Cells.Font.Size = 10;
                    ws.Range["A" + (i + currentSpot + 2).ToString()].Value = (i + 1).ToString();
                    ws.Range["B" + (i + currentSpot + 2).ToString()].Value = kbList[i].SoftwareName;
                    ws.Range["C" + (i + currentSpot + 2).ToString()].Value = "N/A";
                    ws.Range["D" + (i + currentSpot + 2).ToString()].Value = kbList[i].SoftwareVersion;
                    ws.Range["E" + (i + currentSpot + 2).ToString()].Value = "Microsoft";
                    ws.Range["F" + (i + currentSpot + 2).ToString()].Value = "N/A";
                    ws.Range["G" + (i + currentSpot + 2).ToString()].Value = db.getSystem(passedCapture.systemid).modelname;
                    ws.Range["G" + (i + currentSpot + 2).ToString()].Interior.Color = excel.XlRgbColor.rgbLightGray;
                    ws.Range["H" + (i + currentSpot + 2).ToString()].Value = "N/A";
                    ws.Range["I" + (i + currentSpot + 2).ToString()].Value = "N/A";
                    ws.Range["J" + (i + currentSpot + 2).ToString()].Value = "N/A";
                    ws.Range["K" + (i + currentSpot + 2).ToString()].Value = "N/A";

                    ws.Range["A" + (i + currentSpot + 2).ToString() + ":K" + (i + currentSpot + 2).ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;
                    ws.Range["A:I"].Columns.AutoFit();
                    ws.Columns["J:J"].ColumnWidth = 40;
                }
            }

            //ws.Range["A" + (softwareList.Count + 5).ToString()].Value = "Generated by IDOPS AUTO TOOL Version: " + currentVersion.GetVersion();

            //ws.Range["A1:A3"].Value = "Who is number one? :)";
            //ws.Range["A4"].Value = "vitoshacademy.com";

            //ws.Range["B6"].Value = "Tommorow's date is: =>";
            //ws.Range["C6"].FormulaLocal = "= A5 + 1";
            //ws.Range["A7"].FormulaLocal = "=SUM(D1:D10)";
            //for (int i = 1; i <= 10; i++)
            //ws.Range["D" + i].Value = i * 2;

            //CHANGE THIS TO CURRENT DIRECTORY OUTPUT 
            //wb.SaveAs("C:\\Users\\arthur.gartner\\Software_Development\\test\\test.xlsx");

            makeOutputDir();


            String fileName = passedCapture.name + "_" + db.getSystem(passedCapture.systemid).modelname + "_FullSoftwareList" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            wb.SaveAs(excelOutputDir + fileName);
        }
        
        public static void createVerifyDocument(List<SoftwareCompare> list, Capture passedCapture)
        {
            list.Sort();

            List<SoftwareCompare> applications = new List<SoftwareCompare>();
            List<SoftwareCompare> drivers = new List<SoftwareCompare>();
            List<SoftwareCompare> security = new List<SoftwareCompare>();


            foreach (SoftwareCompare item in list)
            {
                if (item.Type.Equals("SECURITY"))
                {
                    security.Add(item);
                }
                else if (item.Type.Equals("DRIVER"))
                {
                    drivers.Add(item);
                }
                else
                {
                    applications.Add(item);
                }
            }


            excel.Application app = new excel.Application();
            app.Visible = true;
            app.WindowState = excel.XlWindowState.xlMaximized;
            app.StandardFont = "Calibri";

            excel.Workbook wb = app.Workbooks.Add(excel.XlWBATemplate.xlWBATWorksheet);
            excel.Worksheet ws = wb.Worksheets[1];

            ws.Rows["1"].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
            ws.Range["A1"].Value = "CM Audit Sheet";
            ws.Range["A1"].Font.Bold = true;
            ws.Range["A1"].Cells.Font.Size = 20;
            ws.Range[ws.Cells[1, 1], ws.Cells[1, 3]].Merge();
            ws.Range[ws.Cells[1, 1], ws.Cells[1, 3]].Interior.Color = System.Drawing.Color.FromArgb(237, 125, 49);
            ws.Columns["A"].ColumnWidth = 40;
            ws.Range["A2"].Value = "Software Version:";
            ws.Range["B2"].Value = CurrentSystem.getRegVersion();
            ws.Range["A3"].Value = "Asset Serial:";
            ws.Range["B3"].Value = CurrentSystem.serial;
            ws.Range["A4"].Value = "Date:";
            ws.Range["B4"].Value = DateTime.Now.ToString("d'-'MMM'-'y");

            for (int i = 0; i < 3; i++)
            {
                ws.Range[ws.Cells[(i + 2), 2], ws.Cells[(i + 2), 3]].Merge();
                ws.Range["A" + (i + 2).ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;
                ws.Range["B" + (i + 2).ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;
                ws.Range["C" + (i + 2).ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;
                ws.Range["A" + (i + 2).ToString()].Interior.Color = excel.XlRgbColor.rgbYellow;
                ws.Range["B" + (i + 2).ToString()].Interior.Color = excel.XlRgbColor.rgbYellow;
                ws.Rows[(i + 2).ToString()].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
            }

            ws.Rows[6].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
            ws.Range["A6:E6"].Font.Bold = true;
            ws.Range["A6:E6"].Font.Name = "Calibri";
            ws.Range["A6"].Value = "Software Name";
            ws.Range["B6"].Value = "Expected Version";
            ws.Range["C6"].Value = "Found Version";
            ws.Range["D6"].Value = "CM Comments";
            ws.Range["E6"].Value = "Integration Comments";
            ws.Range["B:E"].Columns.AutoFit();
            ws.Range["A6:E6"].Borders.Weight = excel.XlBorderWeight.xlThin;

            int currentRow = 6;

            foreach (SoftwareCompare softwareApplication in applications)
            {
                currentRow++;

                ws.Range["A" + currentRow.ToString() + ":E" + currentRow.ToString()].Cells.NumberFormat = "@";
                ws.Range["B" + currentRow.ToString() + ":C" + currentRow.ToString()].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
                ws.Range["A" + currentRow.ToString()].Value = softwareApplication.SoftwareName;
                ws.Range["B" + currentRow.ToString()].Value = softwareApplication.Version;
                ws.Range["C" + currentRow.ToString()].Value = softwareApplication.FoundVersion;
                ws.Range["B:E"].Columns.AutoFit();

                if (softwareApplication.Comparison == 0)
                {
                    ws.Range["C" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(169, 208, 142);
                    ws.Range["C" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(55, 86, 35);
                }
                else
                {
                    ws.Range["C" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(248, 203, 173);
                    ws.Range["C" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(255, 0, 0);
                }

                ws.Range["A" + currentRow.ToString() + ":E" + currentRow.ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;
            }

            if (drivers.Count > 0)
            {
                currentRow += 2;

                ws.Range["A" + currentRow.ToString()].Value = "Drivers";
                ws.Range["A" + currentRow.ToString()].Font.Bold = true;
                ws.Range["A" + currentRow.ToString()].Font.Underline = excel.XlUnderlineStyle.xlUnderlineStyleSingle;


                foreach (SoftwareCompare softwareDriver in drivers)
                {
                    currentRow++;

                    ws.Range["A" + currentRow.ToString() + ":E" + currentRow.ToString()].Cells.NumberFormat = "@";
                    ws.Range["B" + currentRow.ToString() + ":C" + currentRow.ToString()].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
                    ws.Range["A" + currentRow.ToString()].Value = softwareDriver.SoftwareName;
                    ws.Range["B" + currentRow.ToString()].Value = softwareDriver.Version;
                    ws.Range["C" + currentRow.ToString()].Value = softwareDriver.FoundVersion;
                    ws.Range["B:E"].Columns.AutoFit();

                    if (softwareDriver.Comparison == 0)
                    {
                        ws.Range["C" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(169, 208, 142);
                        ws.Range["C" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(55, 86, 35);
                    }
                    else
                    {
                        ws.Range["C" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(248, 203, 173);
                        ws.Range["C" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(255, 0, 0);
                    }

                    ws.Range["A" + currentRow.ToString() + ":E" + currentRow.ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;

                }
            }

            if (security.Count > 0)
            {
                currentRow += 2;

                ws.Range["A" + currentRow.ToString()].Value = "SECURITY PATCHES";
                ws.Range["A" + currentRow.ToString()].Font.Bold = true;
                ws.Range["A" + currentRow.ToString()].Font.Underline = excel.XlUnderlineStyle.xlUnderlineStyleSingle;


                foreach (SoftwareCompare softwareSecurity in security)
                {
                    currentRow++;

                    ws.Range["A" + currentRow.ToString() + ":E" + currentRow.ToString()].Cells.NumberFormat = "@";
                    ws.Range["B" + currentRow.ToString() + ":C" + currentRow.ToString()].Cells.HorizontalAlignment = excel.XlHAlign.xlHAlignCenter;
                    ws.Range["A" + currentRow.ToString()].Value = softwareSecurity.SoftwareName;
                    ws.Range["B" + currentRow.ToString()].Value = softwareSecurity.Version;
                    ws.Range["C" + currentRow.ToString()].Value = softwareSecurity.FoundVersion;
                    ws.Range["B:E"].Columns.AutoFit();

                    if (softwareSecurity.Comparison == 0)
                    {
                        ws.Range["B" + currentRow.ToString()].Value = softwareSecurity.FoundVersion;
                        ws.Range["C" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(169, 208, 142);
                        ws.Range["C" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(55, 86, 35);
                    }
                    else
                    {
                        ws.Range["C" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(248, 203, 173);
                        ws.Range["C" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(255, 0, 0);
                    }

                    ws.Range["A" + currentRow.ToString() + ":E" + currentRow.ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;

                }

            }

            currentRow += 3;

            ws.Range["A" + currentRow.ToString()].Value = "Passed Audit Item";
            ws.Range["A" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(169, 208, 142);
            ws.Range["A" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(55, 86, 35);
            ws.Range["A" + currentRow.ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;

            currentRow++;

            ws.Range["A" + currentRow.ToString()].Value = "Failed Audit Item";
            ws.Range["A" + currentRow.ToString()].Font.Bold = true;
            ws.Range["A" + currentRow.ToString()].Interior.Color = System.Drawing.Color.FromArgb(248, 203, 173);
            ws.Range["A" + currentRow.ToString()].Font.Color = System.Drawing.Color.FromArgb(255, 0, 0);
            ws.Range["A" + currentRow.ToString()].Borders.Weight = excel.XlBorderWeight.xlThin;


            //Capture ALL software on system
            //-Create fake ECPid (-1)
            //-Capture and save every software capture to tie with -1 (Maybe not possible if ecpid is a foreignID???)
            //Compare -1 ecp and selected ecp using softwarecomparison
            //Compare both from selected ECP and captured software
            makeOutputDir();


            String fileName = passedCapture.name + "_" + db.getSystem(passedCapture.systemid).modelname + "_SoftwareAuditList" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            wb.SaveAs(excelOutputDir + fileName);
        }
        private static void makeOutputDir()
        {
            if (!io.Directory.Exists(excelOutputDir))
            {
                io.Directory.CreateDirectory(excelOutputDir);
            }
        }

    }
}
