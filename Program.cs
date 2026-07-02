using System;
using System;
using System.Windows.Forms;

namespace KrPDFmerge
{
    static class Program
    {
        //#240809 BJER:Flyttat till ny repository under KrSystemSyd istället
        /// <summary>
        /// Application entry point.
        /// Args: <c>sourceFolder</c>, <c>outputFile</c>, optional <c>sortMode</c>.
        /// </summary>
        /// <param name="args">Command-line arguments for source folder, output file and optional sorting mode.</param>
        [STAThread]
        static void Main(String[] args)
        {
            string sourcePdfPath = "";
            string outputFile= "";
            string sortMode = "";

            if (args.Length > 0)
            {

                sourcePdfPath = args[0] + "";
                Console.WriteLine("sourcePdfPath:" + sourcePdfPath);

                if (args.Length > 1)
                {
                    outputFile = args[1] + "";
                    Console.WriteLine("outputFile:" + outputFile);
                }
                else
                {
                    Console.WriteLine("No outputfile!");
                    outputFile = Application.StartupPath + "temp.pdf";
                }

                if (args.Length > 2)
                {
                    sortMode = args[2] + "";
                    Console.WriteLine("sortMode:" + sortMode);
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmKrPDFmerge(sourcePdfPath, outputFile, sortMode));
        }
    }
}
