using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace KrPDFmerge
{
    public partial class frmKrPDFmerge : Form
    {
        public const string SORT_BY_DATE = "SORT_BY_DATE";
        public const string SORT_ALPHABETIC = "SORT_ALPHABETIC";
        public const string SORT_PREFIX = "SORT_PREFIX";
        public const string SORT_BY_DATE_DESC = "SORT_BY_DATE_DESC";
        public const string SORT_BY_DATE_ASC = "SORT_BY_DATE_ASC";
        public const string SORT_INVOICE_FIRST = "SORT_INVOICE_FIRST";

        public string gsSourcePdfPath;
        public string gsOutputFile;
        public string gsSortMode;
        public string gsLog;

        /// <summary>
        /// Initializes the form with source folder, output file and optional sorting mode.
        /// </summary>
        /// <param name="sourcePdfPath">Folder that contains source PDF files.</param>
        /// <param name="outputFile">Output PDF file path.</param>
        /// <param name="sortMode">Sorting mode for file ordering before merge.</param>
        public frmKrPDFmerge(string sourcePdfPath, string outputFile, string sortMode)
        {
            gsSourcePdfPath = sourcePdfPath;
            gsOutputFile = outputFile;
            gsSortMode = NormalizeSortMode(sortMode);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the form using default sorting mode.
        /// </summary>
        /// <param name="sourcePdfPath">Folder that contains source PDF files.</param>
        /// <param name="outputFile">Output PDF file path.</param>
        public frmKrPDFmerge(string sourcePdfPath, string outputFile)
            : this(sourcePdfPath, outputFile, SORT_INVOICE_FIRST)
        {
        }

        /// <summary>
        /// Adds a log row to console and in-memory log buffer.
        /// </summary>
        /// <param name="sMessage">Message to append to the log.</param>
        private void LogAdd(string sMessage)
        {
            Console.WriteLine(sMessage);
            gsLog += DateTime.Now + ":" + sMessage + System.Environment.NewLine;
        }

        //Logg för felsökning av programmerare
        /// <summary>
        /// Writes the current log buffer to monthly logfile in startup path.
        /// </summary>
        private void LogWrite()
        {
            string yyyyMM = DateTime.Now.ToString("yyyyMM");
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            gsLog = "EXE-sökväg: " + exePath + System.Environment.NewLine
                  + "StartupPath: " + System.Windows.Forms.Application.StartupPath + System.Environment.NewLine
                  + "****************************" + System.Environment.NewLine + gsLog + System.Environment.NewLine + "****************************";
            Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\log\\");
            File.AppendAllText(System.Windows.Forms.Application.StartupPath + "\\log\\" + yyyyMM + ".txt", gsLog, Encoding.UTF8);
        }

        // Normaliserar UNC-sökvägar med dubblerade bakåtsnedstreck (\\\\server\\share -> \\server\share)
        /// <summary>
        /// Normalizes file paths by removing duplicate slashes while preserving UNC start.
        /// </summary>
        /// <param name="path">Path to normalize.</param>
        /// <returns>Normalized path.</returns>
        private string NormalizePath(string path)
        {
            while (path.Contains(@"\\"))
                path = path.Replace(@"\\", @"\");
            if (path.StartsWith(@"\"))
                path = @"\" + path;
            return path;
        }

        /// <summary>
        /// Normalizes and validates sort mode, including legacy aliases.
        /// </summary>
        /// <param name="sortMode">Incoming sort mode.</param>
        /// <returns>A supported sort mode, otherwise default <c>SORT_INVOICE_FIRST</c>.</returns>
        private string NormalizeSortMode(string sortMode)
        {
            if (string.IsNullOrWhiteSpace(sortMode))
                return SORT_INVOICE_FIRST;

            string mode = sortMode.Trim().ToUpperInvariant();
            if (mode == SORT_ALPHABETIC || mode == SORT_PREFIX || mode == SORT_BY_DATE_ASC || mode == SORT_BY_DATE_DESC || mode == SORT_INVOICE_FIRST)
                return mode;

            if (mode == SORT_BY_DATE)
                return SORT_BY_DATE_DESC;

            return SORT_INVOICE_FIRST;
        }

        /// <summary>
        /// Checks if filename appears to represent an invoice.
        /// </summary>
        /// <param name="filename">Filename to evaluate.</param>
        /// <returns><c>true</c> when filename indicates invoice; otherwise <c>false</c>.</returns>
        private bool IsInvoiceFile(string filename)
        {
            return filename.IndexOf("invoice", StringComparison.OrdinalIgnoreCase) >= 0
                || filename.IndexOf("faktura", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Reads a leading numeric prefix from filename (without extension).
        /// </summary>
        /// <param name="fileNameWithoutExtension">Filename without extension.</param>
        /// <returns>Parsed numeric prefix or <c>null</c> if missing/invalid.</returns>
        private int? GetNumericPrefix(string fileNameWithoutExtension)
        {
            int i = 0;
            while (i < fileNameWithoutExtension.Length && char.IsDigit(fileNameWithoutExtension[i]))
            {
                i++;
            }

            if (i == 0)
                return null;

            int number;
            if (int.TryParse(fileNameWithoutExtension.Substring(0, i), out number))
                return number;

            return null;
        }

        /// <summary>
        /// Returns PDF files sorted according to selected sort mode.
        /// Excludes files containing <c>merged</c> in filename.
        /// </summary>
        /// <param name="sourcePdfPath">Source folder path.</param>
        /// <param name="sortMode">Selected sort mode.</param>
        /// <returns>Sorted list of PDF file paths.</returns>
        private string[] GetSortedPdfFiles(string sourcePdfPath, string sortMode)
        {
            var files = Directory.GetFiles(sourcePdfPath, "*.pdf")
                .Where(f => Path.GetFileName(f).IndexOf("merged", StringComparison.OrdinalIgnoreCase) < 0)
                .ToList();

            if (sortMode == SORT_ALPHABETIC)
            {
                return files
                    .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            if (sortMode == SORT_PREFIX)
            {
                return files
                    .Select(f => new
                    {
                        FilePath = f,
                        FileName = Path.GetFileName(f),
                        Prefix = GetNumericPrefix(Path.GetFileNameWithoutExtension(f))
                    })
                    .OrderBy(x => x.Prefix.HasValue ? 0 : 1)
                    .ThenBy(x => x.Prefix ?? int.MaxValue)
                    .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.FilePath)
                    .ToArray();
            }

            if (sortMode == SORT_BY_DATE_DESC)
            {
                return files
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ThenBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            if (sortMode == SORT_BY_DATE_ASC)
            {
                return files
                    .OrderBy(f => File.GetLastWriteTime(f))
                    .ThenBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            return files
                .OrderBy(f => IsInvoiceFile(Path.GetFileName(f)) ? 0 : 1)
                .ThenBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        //sourcePdfPath = mapp med pdf:er som ska sammanslås
        //outputPdf = sammanslagen pdf med sökväg
        /// <summary>
        /// Merges PDF files from source folder into one output file using selected sort mode.
        /// </summary>
        /// <param name="sourcePdfPath">Folder containing source PDF files.</param>
        /// <param name="outputFile">Output PDF file path.</param>
        public void MergePDFs(string sourcePdfPath, string outputFile)
        {
            try
            {
                sourcePdfPath = NormalizePath(sourcePdfPath);
                outputFile = NormalizePath(outputFile);
                gsSortMode = NormalizeSortMode(gsSortMode);
                LogAdd("Normalized source folder:" + sourcePdfPath + ", output file:" + outputFile + ", sort mode:" + gsSortMode);
                string[] filenames = GetSortedPdfFiles(sourcePdfPath, gsSortMode);
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
                    PdfReader reader = new PdfReader(filename);
                    reader.ConsolidateNamedDestinations();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        int rotation = reader.GetPageRotation(i);
                        if (rotation == 180)
                        {
                            reader.GetPageN(i).Put(PdfName.ROTATE, new PdfNumber(0));
                            LogAdd("Rotation fix applied. File:" + filename + ", page:" + i + ", from:180 to:0");
                        }

                        PdfImportedPage page = writer.GetImportedPage(reader, i);
                        writer.AddPage(page);
                    }
                    reader.Close();
                }
                writer.Close();
                doc.Close();
                LogWrite();
            }
            catch (Exception ex)
            {
                LogAdd("FEL: " + ex.Message);
                LogAdd("Typ: " + ex.GetType().FullName);
                LogAdd("StackTrace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    LogAdd("InnerException: " + ex.InnerException.Message);
                    LogAdd("InnerException StackTrace: " + ex.InnerException.StackTrace);
                }
                LogWrite();
                MessageBox.Show("Fel vid sammanslagning:\n" + ex.Message, "KrPDFmerge - Fel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles form load and performs direct merge when command-line values were provided.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void FrmKrPDFmerge_Load(object sender, EventArgs e)
        {
            if (gsSourcePdfPath != "" && gsOutputFile != "")
            {
                MergePDFs(gsSourcePdfPath, gsOutputFile);
                System.Windows.Forms.Application.Exit();
            }
        }

        /// <summary>
        /// Handles merge button click in GUI mode.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void BtnMerge_Click(object sender, EventArgs e)
        {
            MergePDFs(edSourceFolder.Text, edOutputFile.Text);
        }
    }
}
