using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows;
using WpfApp1;


namespace service;
public class PLC_comunication
{

    public string _Comport;
    public int _Buadrate;
    public int _Datasize;
    public string _Device;
    public PLC_comunication(string Comport,int Buadrate,int Datasize,string Device)
    {
        _Comport = Comport;
        _Buadrate = Buadrate;
        _Datasize = Datasize;
        _Device = Device;
    }

    public bool Check()
    {
        string command = $"%{_Device}#RCSX0000**\r";

        SerialPort serialPort = new SerialPort();

        serialPort.PortName = _Comport;
        serialPort.BaudRate = _Buadrate;
        serialPort.DataBits = _Datasize;
        serialPort.Encoding = Encoding.UTF8;
        serialPort.Parity = Parity.Odd;
        serialPort.StopBits = StopBits.One;
        serialPort.ReadTimeout = 300;
        serialPort.WriteTimeout = 300;

        try
            {
                serialPort.Open();
                serialPort.DiscardInBuffer(); // สำคัญมาก ล้างข้อมูลค้าง
                Thread.Sleep(50);
                serialPort.Write(command);
                Thread.Sleep(100);
                // ✅ ใช้ ReadExisting เหมือน Python readline
                string result = serialPort.ReadExisting();  
                serialPort.Close();
                //MessageBox.Show($"Received: {result}");
                return result.Contains("%EE");
            }
        catch (Exception ex)
            {
                //MessageBox.Show("Error: " + ex.Message);
                return false;
            }
        }

    public bool Readbit(string type, string memory_area)
    {
        int Read_Error = 0;
        string result = "";
        SerialPort serialPort = new SerialPort();

        while (Read_Error == 0)
        {
            try
            {
                string command = $"%{_Device}#RCS{type}{memory_area}**\r";

                serialPort.PortName = _Comport;
                serialPort.BaudRate = _Buadrate;
                serialPort.DataBits = _Datasize;
                serialPort.Encoding = Encoding.UTF8;
                serialPort.Parity = Parity.Odd;
                serialPort.StopBits = StopBits.One;
                serialPort.ReadTimeout = 300;
                serialPort.WriteTimeout = 300;
                serialPort.Open();
                serialPort.DiscardInBuffer(); // สำคัญมาก ล้างข้อมูลค้าง
                Thread.Sleep(50);
                serialPort.Write(command);
                Thread.Sleep(100);
                result = serialPort.ReadExisting();
                serialPort.Close();
                //MessageBox.Show(result);
                if (!string.IsNullOrEmpty(result))
                {
                    if (result.Contains("%EE"))
                    {
                        Read_Error = 1;
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
                MessageBox.Show("Please Release E-stop", "Push and Release E-stop", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please Release E-stop", "Push and Release E-stop", MessageBoxButton.OK, MessageBoxImage.Warning);
                //MessageBox.Show(ex.Message);
            }

        }

        // ตัดเอาเฉพาะตำแหน่งที่ 6 ถึง 6+1 (index 6)
        // %EE$RC020 ตัวอย่าง
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

    public void Writebit(string type,string memory_area,int data)
    {
        int Write_Error = 0;       
        string result = "";
        SerialPort serialPort = new SerialPort();
        while (Write_Error == 0)
            try
            {
                string command = $"%{_Device}#WCS{type}{memory_area}{data}**\r";

                serialPort.PortName = _Comport;
                serialPort.BaudRate = _Buadrate;
                serialPort.DataBits = _Datasize;
                serialPort.Encoding = Encoding.UTF8;
                serialPort.Parity = Parity.Odd;
                serialPort.StopBits = StopBits.One;
                serialPort.ReadTimeout = 300;
                serialPort.WriteTimeout = 300;
                serialPort.Open();
                serialPort.DiscardInBuffer(); // สำคัญมาก ล้างข้อมูลค้าง
                Thread.Sleep(50);
                serialPort.Write(command);
                Thread.Sleep(100);
                result = serialPort.ReadExisting();
                serialPort.Close();
                if (!string.IsNullOrEmpty(result))
                {
                    if (result.Contains("%EE"))
                    {
                        Write_Error = 1;
                        break;
                    }
                    else
                    {
                        return;
                    }
                }
                MessageBox.Show("Please Release E-stop", "Push and Release E-stop", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex) {
                MessageBox.Show("Please Release E-stop", "Push and Release E-stop", MessageBoxButton.OK, MessageBoxImage.Warning);
                //MessageBox.Show(ex.Message);
            }
    }
}
