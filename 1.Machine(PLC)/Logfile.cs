using System.IO;      
using System.Text;
using System.Windows;
using System.Xml.Linq;
namespace service;

public class Logfile
{
    public string _logPath;
    public string logEntry;
    public bool Check_scan;
    public Logfile(string logPath)
    {
        _logPath = logPath;
    }

    public void WriteLog(string logEntry = "")
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string dateOnly = DateTime.Now.ToString("yyyy-MM-dd");
        string choose_path = $"{_logPath}\\{dateOnly}.txt";

        try
        {
            File.AppendAllText(choose_path, logEntry + Environment.NewLine, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to write log: " + ex.Message +"\n[!! Please recheck path of logfolder !!]","Error log path",MessageBoxButton.OK,MessageBoxImage.Error);
        }
    }
    public void WriteMESLog(string SN, string RD, string Timestamp, string URL, string Response_result, string Response_Times, string METHOD)
    {
        string file_name = DateTime.Now.ToString("yyyy-MM-dd");
        string logPath = $"{_logPath}\\{file_name}.xml";


        XDocument doc;
        if (File.Exists(logPath))
        {
            doc = XDocument.Load(logPath);
        }
        else
        {
            // สร้างไฟล์ใหม่พร้อม namespace
            doc = new XDocument(
                new XElement("MesLogData",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XElement("RegistDataMESLog"),
                    new XElement("DeleteDataMESLog"),
                    new XElement("GetDataMESLog")
                )
            );
        }

        // เพิ่มข้อมูลเข้าไปใน productIDMesLog
        XElement entry = new XElement("Product",
            new XAttribute("SN", SN),
            new XAttribute("RD", RD),
            new XElement("Timestamp", Timestamp),
            new XElement("URL_Request", URL),
            new XElement("Response", Response_result),
            new XElement("Response_Times", Response_Times,
                new XAttribute("unit", "ms"))            
        );

        doc.Root.Element($"{METHOD}").Add(entry);
        doc.Save(logPath);

        Console.WriteLine("Log saved!");
    }

}

