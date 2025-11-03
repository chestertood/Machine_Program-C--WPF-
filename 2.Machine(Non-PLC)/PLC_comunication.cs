using System.IO.Ports;
using System.Windows;
using WpfApp1;
using System.IO;

namespace service;
public class PLC_comunication
{
    public string configpath;
    public FileConfig config;

    // PLC Config
    public string PLC_comport;
    public int PLC_buadrate;
    public string PLC_device;

    // ODC Config
    public string ODC_ip;
    public int ODC_Current_Station;

    // Scanner Config
    public string scanner_ip;
    public int scanner_port;
    public int scanner_buffer;

    // Camera Config
    public string camera_ip;
    public int camera_port;
    public int camera_buffer;

    // Path Log
    public string path_log;

    // URL Config
    public string URL_Regist;
    public string URL_Delete;
    public string URL_Get;
    public string URL_getstation;

    // Alvin2
    public string RD_1;

    public int Data_size = 8;

    
    public PLC_comunication()
    {
        configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini");
        config = new FileConfig(configpath);

        // PLC Config
        var PLC_comport = config.Get("PLC", "comport");
        var PLC_buadrate = int.Parse(config.Get("PLC", "buadrate"));
        var PLC_device = config.Get("PLC", "device");

        // ODC Config
        var ODC_ip = config.Get("ODC", "ip");
        var ODC_Current_Station = int.Parse(config.Get("ODC", "Current_Station"));

        // Scanner Config
        var scanner_ip = config.Get("SR_X300W_1", "ip");
        var scanner_port = int.Parse(config.Get("SR_X300W_1", "Port"));
        var scanner_buffer = int.Parse(config.Get("SR_X300W_1", "buffer_size"));

        // Camera Config
        var camera_ip = config.Get("CV-X490F_1", "ip");
        var camera_port = int.Parse(config.Get("CV-X490F_1", "Port"));
        var camera_buffer = int.Parse(config.Get("CV-X490F_1", "buffer_size"));

        // Path Log
        var path_log = config.Get("Path", "Log");

        // URL Config
        var URL_Regist = config.Get("URL", "Regist_Data");
        var URL_Delete = config.Get("URL", "Delete_data");
        var URL_Get = config.Get("URL", "Get_Data");
        var URL_getstation = config.Get("URL", "getstation");

        // Alvin2
        var RD_1 = config.Get("ALVIN2", "RD");

    }

    public bool Check(){
        string command = $"%{PLC_device}#RCSX0000**\r" ;
        SerialPort serialPort = new SerialPort();
        serialPort.PortName = PLC_comport;
        serialPort.BaudRate = PLC_buadrate;
        serialPort.DataBits = Data_size;
        serialPort.Encoding = System.Text.Encoding.UTF8;
        serialPort.Parity = Parity.Odd;
        serialPort.StopBits = StopBits.One;
        serialPort.ReadTimeout = 300;
        serialPort.WriteTimeout = 300;

        try
        {
            serialPort.Open();
            serialPort.Write(command);

            string result = serialPort.ReadLine(); 
            Console.WriteLine("Response: " + result);

            if (result.Contains("%EE"))
            {
                serialPort.Close();
                return true;
            }
            else
            {
                serialPort.Close();
                return false;
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine("Serial Error: " + ex.Message);
            return false;
        }

    }
    
    public bool Readbit(string type, string memory_area)
    {
        Int128 Read_Error = 0;
        string result = "";
        SerialPort serialPort = new SerialPort();

        while (Read_Error == 0)

            try
            {
                string command = $"%{PLC_device}#RCS{type}{memory_area}**\r";
                
                serialPort.PortName = PLC_comport;
                serialPort.BaudRate = PLC_buadrate;
                serialPort.DataBits = Data_size;
                serialPort.Encoding = System.Text.Encoding.UTF8;
                serialPort.Parity = Parity.Odd;
                serialPort.StopBits = StopBits.One;
                serialPort.ReadTimeout = 300;
                serialPort.WriteTimeout = 300;
                serialPort.Open();
                serialPort.Write(command);

                // อ่าน 10 bytes
                char[] buffer = new char[10];
                int bytesRead = serialPort.Read(buffer, 0, buffer.Length);
                result = new string(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {

                Thread.Sleep(100);
                Console.WriteLine("Push and Release E-stoppp");
            }

            finally
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                if (result.Contains("%EE"))
                {
                    Read_Error = 1;
                }
                else
                {
                    Console.WriteLine("Push and Release E-stop");
                    return false;
                }
            }

            // ตัดเอาเฉพาะตำแหน่งที่ 6 ถึง 6+1 (index 6)
            if (result.Length >= 7)
            {
                string bit = result.Substring(6, 1);
                return bit == "1";
            }
            else
            {
                return false;
            }


    } 
    
    public void Writebit(string type,string memory_area,string data)
    {
        Int128 Write_Error = 0;       
        string result = "";
        SerialPort serialPort = new SerialPort();
        while (Write_Error == 0)

            try
            {
                string command = $"%{PLC_device}#WCS{type}{memory_area}{data}**\r";

                serialPort.PortName = PLC_comport;
                serialPort.BaudRate = PLC_buadrate;
                serialPort.DataBits = Data_size;
                serialPort.Encoding = System.Text.Encoding.UTF8;
                serialPort.Parity = Parity.Odd;
                serialPort.StopBits = StopBits.One;
                serialPort.ReadTimeout = 300;
                serialPort.WriteTimeout = 300;
                serialPort.Open();
                serialPort.Write(command);

                // อ่าน 9 bytes
                char[] buffer = new char[9];
                int bytesRead = serialPort.Read(buffer, 0, buffer.Length);
                result = new string(buffer, 0, bytesRead);
            }
            catch (Exception e) {

                Thread.Sleep(100);
                Console.WriteLine("Push and Release E-stop");
            }
            finally
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                if (result.Contains("%EE"))
                {
                    Write_Error = 1;
                }
                else
                {
                    Console.WriteLine("Push and Release E-stop");
                    return;
                }
            }
    }
}
