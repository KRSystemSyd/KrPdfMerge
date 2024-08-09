using System;
using System.Windows.Forms;

namespace KrPDFmerge
{
    static class Program
    {
        //#240809 BJER:Flyttat till ny repository under KrSystemSyd istället
        [STAThread]
        static void Main(String[] args)
        {
            string sourcePdfPath = "";
            string outputFile= "";

            if (args.Length > 0)
            {

                sourcePdfPath = args[0] + "";
                Console.WriteLine("sourcePdfPath:" + sourcePdfPath);

                if (args.Length > 0)
                {
                    outputFile = args[1] + "";
                    Console.WriteLine("outputFile:" + outputFile);
                }
                else
                {
                    Console.WriteLine("No outputfile!");
                    outputFile = Application.StartupPath + "temp.pdf";
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmKrPDFmerge(sourcePdfPath,outputFile));
        }
    }
}
