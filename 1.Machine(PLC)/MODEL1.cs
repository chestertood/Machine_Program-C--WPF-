using service;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WpfApp1;
namespace Model;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

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

            // Loading parameter from Config.ini //
            /////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////
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

            int Number_of_PCB = 7; // จำนวน PCB ที่จะสแกนในแต่ละรอบ

            string[] SN = new string[Number_of_PCB];
            string[] OCR = new string[Number_of_PCB];
            string[] OCR_result = new string[Number_of_PCB];

            Logfile logfile = new Logfile(path_log);
            Logfile MES_logfile = new Logfile(MES_path_log);

            TCP_Communication scanner = new TCP_Communication(scanner_ip, scanner_port, scanner_buffer);
            TCP_Communication camera = new TCP_Communication(camera_ip, camera_port, camera_buffer);
            PLC_comunication PLC = new PLC_comunication(PLC_comport, PLC_buadrate, 8, PLC_device);
            /////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////

            _page3.Status_box_MODEL1.Text = "Machine Initial Please wait...";
            await Task.Run(() =>
            {
                camera.Client("PW,1,1\r", 1);   // Camera Mode example  PW,1,1/r  ,  PW,1,2/r
            });


            bool Photo_sensor = PLC.Readbit("X", "0002"); // Read Photo Sensor from PLC

            while (Photo_sensor == true && StopFlag)
            {
                PLC.Writebit("Y", "0002", 0); // Stop Conveyor
                PLC.Writebit("R", "0001", 1); // Set Conveyor to Run
                PLC.Writebit("R", "0060", 0); // Reset Register
                Photo_sensor = PLC.Readbit("X", "0002"); // Check Photo Sensor
                MessageBox.Show("Remove PCBA on Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Warning);
                _page3.Status_box_MODEL1.Text = "Remove PCBA on Conveyor";

            }

            string Result_show = "";
            while (StopFlag)
            {
                _page3.Status_box_MODEL1.Text = "Waiting PCB!!!";
                await Task.Delay(100);
                _page3.Status_box_MODEL1.Text = null;
                await Task.Delay(100);
                PLC.Writebit("Y", "0002", 1); // Start Conveyor
                Photo_sensor = PLC.Readbit("X", "0002"); // Check Photo Sensor
                if (Photo_sensor == true)
                {
                    await Task.Delay(400);
                    _page3.Status_box_MODEL1.Text = "Running";
                    PLC.Writebit("R", "0060", 1); // Set Register to 1

                    byte[][] Scanner_Result = null;
                    await Task.Run(() =>
                    {
                        Scanner_Result = scanner.Client("LON\r", 1); // Send command to Scanner to get data
                    });
                    string scanner_decode = Encoding.UTF8.GetString(Scanner_Result[0]).Replace("\r", ""); // Decode Scanner Result  
                                                                                                          //Example = "1,01,MES1|1,02,MES2|1,03,MES3|1,04,MES4|2,05,MES5|2,06,MES6|2,07,MES7";

                    string[] parts = scanner_decode.Split('|');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        string[] subParts = parts[i].Split(',');                   /////////////////////            Cleaning Data               //////////////////////////////
                        if (subParts.Length == 3)
                        {
                            SN[i] = subParts[2];
                        }
                    }


                    byte[][] Camera_Result = null;
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < Number_of_PCB; i++)
                        {
                            camera.Client($"STW,{i},\"{SN[i]}\r", 1); // Send command to Camera to set SN for each PCBA
                        }
                        Camera_Result = camera.Client("TA\r", 2); // Send command to Camera to get data
                    });
                    string camera_decode = Encoding.UTF8.GetString(Camera_Result[1]).Replace("\r", "");

                    parts = camera_decode.Split('|');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        string[] subParts = parts[i].Split(',');

                        if (subParts.Length == 5)
                        {
                            OCR[i] = $"{subParts[1]},{subParts[2]},{subParts[3]},{subParts[4]}";                              /////////////////////            Cleaning Data               //////////////////////////////
                        }
                    }

                    int scan_ok = 0;
                    for (int i = 0; i < Number_of_PCB; i++)
                    {
                        if (SN[i] == "ERROR")
                        {
                            SN[i] = "N/A";
                        }

                        var SN_text = _page3.FindName($"serial_number_{i + 1}") as TextBox;                                  ////////////////            Show SN and OCR on UI               //////////////////////////////
                        var OCR_text = _page3.FindName($"OCR_{i + 1}") as TextBox;

                        if (SN_text != null) SN_text.Text = $"{SN[i]}";
                        if (OCR_text != null) OCR_text.Text = $"{OCR[i]}";

                        try
                        {
                            string[] Clean_OCR = OCR[i].Split(",");
                            if (SN[i] != "N/A" && !OCR[i].Contains(" ") && !string.IsNullOrWhiteSpace(OCR[i]))   ////////////////            Check SN and OCR Format               //////////////////////////////
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

                    if (scan_ok == SN.Length)
                    {

                        int success = 0;
                        MES MES = new MES();
                        MES.Connect(MES_ip); // Connect to MES Tracker server


                        for (int i = 0; i < Number_of_PCB; i++)
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
                                logfile.WriteLog(logEntry); // Write log entry for registered data

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
                                logfile.WriteLog(logEntry); // Write log entry for not registered data
                                success += 1;
                            }

                        }
                        if (success == SN.Length)
                        {
                            _page3.Status_box_MODEL1.Text = "Regist Data Success";
                            PLC.Writebit("Y", "0002", 0); // Stop Conveyor
                            PLC.Writebit("R", "0060", 0); // Reset Register
                            PLC.Writebit("R", "0001", 1); // Set Conveyor to Run
                            Photo_sensor = PLC.Readbit("X", "0002"); // Check Photo Sensor

                            while (Photo_sensor == true && StopFlag)
                            {

                                PLC.Writebit("Y", "0002", 0); // Stop Conveyor
                                Photo_sensor = PLC.Readbit("X", "0002"); // Check Photo Sensor
                                await Task.Delay(100);

                            }
                            await Task.Delay(500);
                            PLC.Writebit("Y", "0002", 1); // Start Conveyor
                            bool End_Sensor = PLC.Readbit("X", "0001"); // Check End Sensor
                            while (End_Sensor == true && StopFlag)
                            {
                                End_Sensor = PLC.Readbit("X", "0001"); //Check End Sensor
                                await Task.Delay(100);
                            }
                            await Task.Delay(500);
                            for (int i = 0; i < Number_of_PCB; i++)
                            {
                                var SN_text = _page3.FindName($"serial_number_{i + 1}") as TextBox;
                                var OCR_text = _page3.FindName($"OCR_{i + 1}") as TextBox;

                                if (SN_text != null) SN_text.Text = "";
                                if (OCR_text != null) OCR_text.Text = "";
                            }
                            await Task.Delay(100);

                        }
                        else
                        {
                            _page3.Status_box_MODEL1.Text = "Regist Data FAILED";
                            MessageBox.Show("Please Check \"MES Connection\" Again!!!", "Regist Data FAILED", MessageBoxButton.OK, MessageBoxImage.Error);
                            Photo_sensor = PLC.Readbit("X", "0002");
                            while (Photo_sensor == true && StopFlag)
                            {
                                PLC.Writebit("Y", "0002", 0);
                                MessageBox.Show("Please take out of PCB from Conveyor", "Regist error !!!", MessageBoxButton.OK, MessageBoxImage.Error);
                                Photo_sensor = PLC.Readbit("X", "0002");
                                await Task.Delay(100);
                            }
                            PLC.Writebit("Y", "0002", 1);
                            PLC.Writebit("R", "0060", 0);
                            PLC.Writebit("R", "0001", 1);
                            for (int i = 0; i < Number_of_PCB; i++)
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

                        for (int i = 0; i < Number_of_PCB; i++)
                        {
                            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string logEntry = $"Not Registered>>>> {timestamp} {SN[i]}, Data:{OCR[i]}";
                            logfile.WriteLog(logEntry);

                        } while (Photo_sensor == true && StopFlag)
                        {
                            MessageBox.Show($"Scan Error !!!\n{SN[0]}, Data:{OCR[0]}" +
                                $"\n{SN[1]}, Data:{OCR[1]}" +
                                $"\n{SN[2]}, Data:{OCR[2]}" +
                                $"\n{SN[3]}, Data:{OCR[3]}" +
                                $"\n{SN[4]}, Data:{OCR[4]}" +
                                $"\n{SN[5]}, Data:{OCR[5]}" +
                                $"\n{SN[6]}, Data:{OCR[6]}" +
                                $"\nPlease take out of PCB from Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Error);
                            PLC.Writebit("Y", "0002", 0);
                            Photo_sensor = PLC.Readbit("X", "0002");
                            await Task.Delay(300);
                        }
                        await Task.Delay(1000);
                        for (int i = 0; i < Number_of_PCB; i++)
                        {
                            var SN_text = _page3.FindName($"serial_number_{i + 1}") as TextBox;
                            var OCR_text = _page3.FindName($"OCR_{i + 1}") as TextBox;

                            if (SN_text != null) SN_text.Text = "";
                            if (OCR_text != null) OCR_text.Text = "";
                            Result_show = $"{SN[i]}, Data:{OCR[i]}, PCBA[{i + 1}] ERROR SCANNING \n" + Result_show;
                        }
                        _page3.Result.Text = Result_show;
                        PLC.Writebit("Y", "0002", 1); // Start Conveyor
                        PLC.Writebit("R", "000A", 1); // Set Register to 1 for Error
                        PLC.Writebit("R", "0060", 0); // Reset Register
                        await Task.Delay(100);
                    }
                }
            }
        }
    }
}