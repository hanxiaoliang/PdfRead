using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PdfReadTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "F:\\工作记录\\数据管理\\文档";
            openFileDialog1.Filter = "pdf file(*.pdf)|*pdf";

            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                txtFile.Text = openFileDialog1.FileName;
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            string path = txtFile.Text.Trim();
            if (!string.IsNullOrEmpty(path))
            {
                StringBuilder text = new StringBuilder();
                if (File.Exists(path))
                {
                    PdfReader pdfReader = new PdfReader(path);
                    List<AirportPoint> Points = new List<AirportPoint>();
                    Dictionary<int, string> dicColName = null;

                    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                    {
                        //ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        //ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                        CustomStrategy strategy = new CustomStrategy();
                        string val = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                        //currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                        //text.Append(currentText);

                        if (null != dicColName)
                            strategy.dicBlockColName = dicColName;

                        string msg = strategy.InitPoint();
                        if (strategy.dicBlockColName != null)
                            dicColName = strategy.dicBlockColName;

                        text.AppendLine(string.Format("{0}页，解析结果：{1} ", page, msg));

                        if (null != strategy.Points && strategy.Points.Any())
                        {
                            Points = Points.Concat(strategy.Points).ToList();
                        }
                    }
                    ShowPoint(Points);
                    txtMsg.Text = text.ToString();
                    pdfReader.Close();
                }
            }
        }

        private void ShowPoint(List<AirportPoint> points)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (var item in points)
            {
                count++;
                sb.AppendFormat("{0}:{1}  {2}", count, item.PointNo, item.LatLong);
                sb.AppendLine();
            }

            txtResult.Text = sb.ToString();
        }
    }
}