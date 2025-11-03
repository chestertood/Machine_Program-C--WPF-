using service;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WpfApp1;
namespace Model;


public class MODEL1
{
    public bool StopFlag = false;
    public Page3 _page3;
    public MODEL1(Page3 page3)
    {
        _page3 = page3;  // ✅ เก็บอ้างอิง
    }

    public void stop()
    {
        StopFlag = false;
    }
    public void start()
    {
        StopFlag = true;
    }
    public async void run()
    {
        while (StopFlag)
        {

            string configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini");

            var config = new FileConfig(configpath);

            // PLC Config
            var PLC_comport = config.Get("PLC", "comport");
            var PLC_buadrate = int.Parse(config.Get("PLC", "buadrate"));
            var PLC_device = config.Get("PLC", "device");

            // MES Config
            var MES_ip = config.Get("MES", "ip");
            var MES_Current_Station = int.Parse(config.Get("MES", "Current_Station"));

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
            var MES_path_log = config.Get("Path", "MES_Log");

            // URL Config
            var URL_Regist = config.Get("URL", "Regist_Data");
            var URL_Delete = config.Get("URL", "Delete_data");
            var URL_Get = config.Get("URL", "Get_Data");
            var URL_getstation = config.Get("URL", "getstation");

            // MODEL1
            var RD_1 = config.Get("MODEL1", "RD");

            int number_of_PCB = 1;

            string[] SN = new string[number_of_PCB];
            string[] OCR = new string[number_of_PCB];
            string[] OCR_result = new string[number_of_PCB];

            Logfile logfile = new Logfile(path_log);
            Logfile MES_logfile = new Logfile(MES_path_log);

            TCP_Communication scanner = new TCP_Communication(scanner_ip, scanner_port, scanner_buffer);
            TCP_Communication camera = new TCP_Communication(camera_ip, camera_port, camera_buffer);

            byte[][] scanner_result = null;

            _page3.Status_box_MODEL1.Text = "Machine Initial Please wait...";
            await Task.Run(() =>
            {
                camera.Client("PW,1,1\r", 1);
                scanner.Client("OUTOFF,1\r", 1); // Turn off output

            });

            scanner_result = scanner.Client("INCHK,2\r", 1); // Check photo sensor status
            string decoded = Encoding.UTF8.GetString(scanner_result[0]);
            string[] parts = decoded.Split(',');
            string Photo_sensor = parts[2].Replace("\r", "");

            while (Photo_sensor == "ON" && StopFlag)
            {
                _page3.Status_box_MODEL1.Text = "Remove PCBA on Conveyor";
                MessageBox.Show("Remove PCBA on Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Warning);
                scanner_result = scanner.Client("INCHK,2\r", 1);
                decoded = Encoding.UTF8.GetString(scanner_result[0]);
                parts = decoded.Split(',');
                Photo_sensor = parts[2].Replace("\r", "");

            }

            string Result_show = "";
            await Task.Delay(100);

            while (StopFlag)
            {
                _page3.Status_box_MODEL1.Text = "Waiting PCB!!!";
                await Task.Delay(100);
                _page3.Status_box_MODEL1.Text = null;
                await Task.Delay(100);
                scanner_result = scanner.Client("INCHK,2\r", 1);
                decoded = Encoding.UTF8.GetString(scanner_result[0]);
                parts = decoded.Split(',');
                Photo_sensor = parts[2].Replace("\r", "");
                if (Photo_sensor == "ON")
                {
                    _page3.Status_box_MODEL1.Text = "Running";
                    await Task.Delay(100);
                    byte[][] Scanner_Result = null;
                    byte[][] Camera_Result = null;
                    await Task.Run(() =>
                    {
                        Scanner_Result = scanner.Client("LON\r", 1); // Turn on scanner
                        Camera_Result = camera.Client("TA\r", 2); // Take a picture with camera
                    });

                    string scanner_decode = Encoding.UTF8.GetString(Scanner_Result[0]).Replace("\r", "");
                    string camera_decode = Encoding.UTF8.GetString(Camera_Result[1]).Replace("\r", "");
                    SN[0] = scanner_decode;
                    OCR[0] = camera_decode;

                    int scan_ok = 0;
                    for (int i = 0; i < number_of_PCB; i++)
                    {

                        if (SN[i] == "ERROR")
                        {
                            SN[i] = "N/A";
                        }

                        var SN_text = _page3.FindName($"serial_number_{i + 1}") as TextBox;
                        var OCR_text = _page3.FindName($"OCR_{i + 1}") as TextBox;

                        if (SN_text != null) SN_text.Text = $"{SN[i]}";
                        if (OCR_text != null) OCR_text.Text = $"{OCR[i]}";

                        try
                        {
                            string[] Clean_OCR = OCR[i].Split(",");
                            if (SN[i] != "N/A" && !OCR[i].Contains(" ") && !string.IsNullOrWhiteSpace(OCR[i]))
                            {
                                scan_ok += 1;
                            }
                            else if (SN[i] == "N/A" && OCR[i] == "           ,    ,         ,      ")
                            {
                                scan_ok += 1;
                            }
                        }
                        catch
                        {
                            scan_ok += 0;
                        }

                    }

                    if (scan_ok == 1)
                    {

                        int success = 0;
                        MES MES = new MES();
                        MES.Connect(MES_ip);

                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            if (SN[i] != "N/A")
                            {
                                string resign_url = string.Format(URL_Regist, SN[i], RD_1, OCR[i]);
                                string delete_url = string.Format(URL_Delete, RD_1, SN[i]);
                                string getdata_url = string.Format(URL_Get, RD_1, SN[i]);

                                // Delete
                                var swDelete = Stopwatch.StartNew();
                                await MES.RequestDataAsync(delete_url, HttpMethod.Get);
                                swDelete.Stop();
                                double durationDelete = swDelete.Elapsed.TotalMilliseconds;
                                string Delete_respone = MES.GetData();

                                // Register
                                var swReg = Stopwatch.StartNew();
                                await MES.RequestDataAsync(resign_url, HttpMethod.Get);
                                swReg.Stop();
                                double durationReg = swReg.Elapsed.TotalMilliseconds;
                                string Registered_respone = MES.GetData();

                                // GetData
                                var swGet = Stopwatch.StartNew();
                                await MES.RequestDataAsync(getdata_url, HttpMethod.Get);
                                swGet.Stop();
                                double durationGet = swGet.Elapsed.TotalMilliseconds;
                                string Check_OCR = MES.GetData();


                                if (Check_OCR.Contains(OCR[i]))
                                {
                                    OCR_result[i] = "OK";

                                }
                                else
                                {
                                    OCR_result[i] = "NOK";
                                }

                                if (OCR_result[i] == "OK")
                                {
                                    success += 1;
                                }

                                Result_show = $"{SN[i]}, Data:{OCR[i]} , Register Result: {OCR_result[i]} \n" + Result_show;
                                _page3.Result.Text = Result_show;

                                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                string logEntry = $"Registered>>>> {timestamp} {SN[i]}, Data:{OCR[i]} , Register Result: {OCR_result[i]}";
                                logfile.WriteLog(logEntry);


                                try
                                {
                                    MES_logfile.WriteMESLog(SN[i], RD_1, timestamp, delete_url, Delete_respone, durationDelete.ToString(), "DeleteDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_1, timestamp, resign_url, Registered_respone, durationReg.ToString(), "RegistDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_1, timestamp, getdata_url, Check_OCR, durationGet.ToString(), "GetDataMESLog");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error writing MES log: " + ex.Message);
                                }

                            }
                            else
                            {
                                Result_show = $"{SN[i]}, Data:{OCR[i]} \n" + Result_show;
                                _page3.Result.Text = Result_show;
                                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                string logEntry = $"Not Registered>>>> {timestamp} PCBA_[{i + 1}] is X-Out";
                                logfile.WriteLog(logEntry);
                                success += 1;
                            }

                        }
                        if (success == 1)
                        {
                            _page3.Status_box_MODEL1.Text = "Regist Data Success";
                            scanner_result = scanner.Client("INCHK,2\r", 1);
                            decoded = Encoding.UTF8.GetString(scanner_result[0]);
                            parts = decoded.Split(',');
                            Photo_sensor = parts[2].Replace("\r", "");


                            while (Photo_sensor == "ON" && StopFlag)
                            {
                                scanner.Client("OUYON,1\r", 1);
                                MessageBox.Show("Please take out of PCB from Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Information);
                                scanner_result = scanner.Client("INCHK,2\r", 1);
                                decoded = Encoding.UTF8.GetString(scanner_result[0]);
                                parts = decoded.Split(',');
                                Photo_sensor = parts[2].Replace("\r", "");
                                await Task.Delay(100);

                            }
                            for (int i = 0; i < number_of_PCB; i++)
                            {
                                var SN_text = _page3.FindName($"serial_number_{i + 1}") as TextBox;
                                var OCR_text = _page3.FindName($"OCR_{i + 1}") as TextBox;

                                if (SN_text != null) SN_text.Text = "";
                                if (OCR_text != null) OCR_text.Text = "";
                            }
                            await Task.Delay(100);
                            scanner.Client("OUTOFF,1\r", 1);

                        }
                        else
                        {
                            _page3.Status_box_MODEL1.Text = "Regist Data FAILED";
                            MessageBox.Show("Please Check \"MES Connection\" Again!!!", "Regist Data FAILED", MessageBoxButton.OK, MessageBoxImage.Error);
                            scanner.Client("OUYON,1\r", 1);
                            scanner_result = scanner.Client("INCHK,2\r", 1);
                            decoded = Encoding.UTF8.GetString(scanner_result[0]);
                            parts = decoded.Split(',');
                            Photo_sensor = parts[2].Replace("\r", "");
                            while (Photo_sensor == "ON" && StopFlag)
                            {
                                MessageBox.Show("Please take out of PCB from Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Information);
                                scanner_result = scanner.Client("INCHK,2\r", 1);
                                decoded = Encoding.UTF8.GetString(scanner_result[0]);
                                parts = decoded.Split(',');
                                Photo_sensor = parts[2].Replace("\r", "");
                                await Task.Delay(100);

                            }
                            for (int i = 0; i < number_of_PCB; i++)
                            {
                                var SN_text = _page3.FindName($"serial_number_{i + 1}") as TextBox;
                                var OCR_text = _page3.FindName($"OCR_{i + 1}") as TextBox;

                                if (SN_text != null) SN_text.Text = "";
                                if (OCR_text != null) OCR_text.Text = "";
                            }
                            await Task.Delay(100);
                        }

                    }
                    else
                    {

                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string logEntry = $"Not Registered>>>> {timestamp} {SN[i]}, Data:{OCR[i]}";
                            logfile.WriteLog(logEntry);
                            scanner_result = scanner.Client("INCHK,2\r", 1);
                            decoded = Encoding.UTF8.GetString(scanner_result[0]);
                            parts = decoded.Split(',');
                            Photo_sensor = parts[2].Replace("\r", "");

                        } while (Photo_sensor == "ON" && StopFlag)
                        {
                            MessageBox.Show($"Scan Error !!!\n{SN[0]}\nData:{OCR[0]}\nPlease take out of PCB from Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Error);
                            scanner_result = scanner.Client("INCHK,2\r", 1);
                            decoded = Encoding.UTF8.GetString(scanner_result[0]);
                            parts = decoded.Split(',');
                            Photo_sensor = parts[2].Replace("\r", "");
                            await Task.Delay(100);
                        }
                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            var SN_text = _page3.FindName($"serial_number_{i + 1}") as TextBox;
                            var OCR_text = _page3.FindName($"OCR_{i + 1}") as TextBox;

                            if (SN_text != null) SN_text.Text = "";
                            if (OCR_text != null) OCR_text.Text = "";
                            Result_show = $"{SN[i]}, Data:{OCR[i]}, PCBA[{i + 1}] ERROR SCANNING \n" + Result_show;
                        }
                        _page3.Result.Text = Result_show;
                        await Task.Delay(100);
                    }

                }
            }
        }
    }
}
