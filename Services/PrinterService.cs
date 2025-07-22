using POS_ModernUI.Services.Contracts;
using System.IO;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace POS_ModernUI.Services;

public class PrinterService : IPrinterService
{
    
    public void PrintReciept(FrameworkElement element)
    {
        string sPrintFileName = "print_preview.xps";

        try
        {
            if (File.Exists(sPrintFileName))
                File.Delete(sPrintFileName);
        }
        catch { }

        XpsDocument doc = new(sPrintFileName, FileAccess.ReadWrite);

        XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);

        SerializerWriterCollator output_Doc = writer.CreateVisualsCollator();
        output_Doc.BeginBatchWrite();
        output_Doc.Write(element);
        output_Doc.EndBatchWrite();

        FixedDocumentSequence preview = doc.GetFixedDocumentSequence();

        PrintDocument(preview);

        doc.Close();
        writer = null;
        output_Doc = null;
        doc = null;
    }


    private void PrintDocument(FixedDocumentSequence document)
    {
        PrintDialog printDialog = new PrintDialog();
        printDialog.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
        printDialog.PrintTicket = printDialog.PrintQueue.DefaultPrintTicket;

        printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(80, 20);
        printDialog.PrintTicket.PageBorderless = PageBorderless.None;

        document.PrintTicket = printDialog.PrintTicket;
        XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printDialog.PrintQueue);

        writer.WriteAsync(document, printDialog.PrintTicket);
    }
}
