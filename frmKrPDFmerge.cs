using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;


namespace KrPDFmerge
{
    public partial class frmKrPDFmerge : Form
    {
        public string gsSourcePdfPath;
        public string gsOutputFile;
        public string gsLog;
        public frmKrPDFmerge(string sourcePdfPath, string outputFile)
        {
            gsSourcePdfPath = sourcePdfPath;
            gsOutputFile = outputFile;
            InitializeComponent();
        }

        private void LogAdd(string sMessage)
        {
            Console.WriteLine(sMessage);
            gsLog += DateTime.Now + ":" + sMessage + System.Environment.NewLine;
        }

        //Logg för felsökning av programmerare
        private void LogWrite()
        {
            string yyyyMM = DateTime.Now.ToString("yyyyMM");
            gsLog = "****************************" + System.Environment.NewLine + gsLog + System.Environment.NewLine + "****************************";
            Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\log\\");
            File.AppendAllText(System.Windows.Forms.Application.StartupPath + "\\log\\" + yyyyMM + ".txt", gsLog, Encoding.UTF8);
        }

        //sourcePdfPath = mapp med pdf:er som ska sammanslås
        //outputPdf = sammanslagen pdf med sökväg
        public void MergePDFs(string sourcePdfPath,string outputFile)
        {
            string[] filenames = Directory.GetFiles(sourcePdfPath);
            LogAdd("-----------------------------------------------------");
            LogAdd("Source folder:" + sourcePdfPath + ", output file:" + outputFile);
            Document doc = new Document();
            PdfCopy writer = new PdfCopy(doc, new FileStream(outputFile, FileMode.Create));
            if (writer == null)
            {
                return;
            }
            doc.Open();
            foreach (string filename in filenames)
            {
                LogAdd("Filename:" + filename);
                if (filename.ToLower().IndexOf("merged") > 0)
                {
                    Console.WriteLine("Hoppa över filer som har 'merged' i filnamnet");
                }
                else
                {
                    PdfReader reader = new PdfReader(filename);
                    reader.ConsolidateNamedDestinations();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        PdfImportedPage page = writer.GetImportedPage(reader, i);
                        writer.AddPage(page);
                    }
                    reader.Close();
                }
            }
            writer.Close();
            doc.Close();
            LogWrite();
        }

        private void FrmKrPDFmerge_Load(object sender, EventArgs e)
        {
            if (gsSourcePdfPath != "" && gsOutputFile != "")
            {
                MergePDFs(gsSourcePdfPath, gsOutputFile);
                System.Windows.Forms.Application.Exit();
            }
        }

        private void BtnMerge_Click(object sender, EventArgs e)
        {
            MergePDFs(edSourceFolder.Text, edOutputFile.Text);
        }
    }
}
