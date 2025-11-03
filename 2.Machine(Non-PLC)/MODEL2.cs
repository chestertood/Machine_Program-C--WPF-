using service;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using WpfApp1;
namespace Model;
using System.IO;
using System.Net.Http;


public class MODEL2
{
    public bool StopFlag = false;
    public Page3 _page3;
    public MODEL2(Page3 page3)
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

            //string configpath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\AlvinC#\\Config.ini";
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

            // MODEL2
            var RD_1 = config.Get("MODEL2", "RD_1");
            var RD_2 = config.Get("MODEL2", "RD_2");
            var RD_3 = config.Get("MODEL2", "RD_3");
            var RD_4 = config.Get("MODEL2", "RD_4");

            int number_of_PCB = 1;

            string[] SN = new string[number_of_PCB];
            string[] Data_Chip1 = new string[number_of_PCB];
            string[] Data_Chip2 = new string[number_of_PCB];
            string[] Data_Chip3 = new string[number_of_PCB];
            string[] Data_Chip4 = new string[number_of_PCB];
            string[] OCR_result = new string[number_of_PCB];


            Logfile logfile = new Logfile(path_log);
            Logfile MES_logfile = new Logfile(MES_path_log);

            TCP_Communication scanner = new TCP_Communication(scanner_ip, scanner_port, scanner_buffer);
            TCP_Communication camera = new TCP_Communication(camera_ip, camera_port, camera_buffer);

            byte[][] scanner_result = null;

            _page3.Status_box_MODEL2.Text = "Machine Initial Please wait...";
            await Task.Run(() =>
            {
                camera.Client("PW,1,2\r", 1);
                scanner.Client("OUTOFF,1\r", 1);

            });

            scanner_result = scanner.Client("INCHK,2\r", 1);
            string decoded = Encoding.UTF8.GetString(scanner_result[0]);
            string[] parts = decoded.Split(',');
            string Photo_sensor = parts[2].Replace("\r", "");

            while (Photo_sensor == "ON" && StopFlag)
            {
                MessageBox.Show("Remove PCBA on Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Warning);
                _page3.Status_box_MODEL2.Text = "Remove PCBA on Conveyor";
                scanner_result = scanner.Client("INCHK,2\r", 1);
                decoded = Encoding.UTF8.GetString(scanner_result[0]);
                parts = decoded.Split(',');
                Photo_sensor = parts[2].Replace("\r", "");

            }

            string Result_show = "";


            while (StopFlag)
            {
                _page3.Status_box_MODEL2.Text = "Waiting PCB!!!";
                await Task.Delay(100);
                _page3.Status_box_MODEL2.Text = null;
                await Task.Delay(100);
                scanner_result = scanner.Client("INCHK,2\r", 1);
                decoded = Encoding.UTF8.GetString(scanner_result[0]);
                parts = decoded.Split(',');
                Photo_sensor = parts[2].Replace("\r", "");

                if (Photo_sensor == "ON")
                {
                    _page3.Status_box_MODEL2.Text = "Running";
                    await Task.Delay(100);

                    string[][] Chip = new string[number_of_PCB][];
                    string[][][] data = new string[number_of_PCB][][];
                    string[] SN_new = new string[number_of_PCB];
                    string[][][] Data_final = new string[number_of_PCB][][];
                    byte[][] Camera_Result = null;
                    await Task.Run(() =>
                    {
                        Camera_Result = camera.Client("TA\r", 2);
                    });

                    string camera_decode = Encoding.UTF8.GetString(Camera_Result[1]).Replace("\r", "");
                    int scan_ok = 0;

                    try
                    {
                        var PCBA = camera_decode.Split("|");
                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            Chip[i] = PCBA[i].Split(':');
                            data[i] = new string[4][];
                            Data_final[i] = new string[4][];

                            for (int j = 0; j < 4; j++)
                            {
                                Data_final[i][j] = new string[2];
                                data[i][j] = Chip[i][j].Split(',');
                                SN_new[i] = data[i][j][1];
                                if ((SN_new[i] == "               ") | (SN_new[i] == "              "))
                                {
                                    SN_new[i] = "N/A";
                                }
                                Data_final[i][j][0] = data[i][j][0];
                                Data_final[i][j][1] = $"{data[i][j][2]},{data[i][j][3]},{data[i][j][4]},{data[i][j][5]}";

                            }
                        }

                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            SN[i] = SN_new[i];
                            Data_Chip1[i] = Data_final[i][0][1];
                            Data_Chip2[i] = Data_final[i][1][1];
                            Data_Chip3[i] = Data_final[i][2][1];
                            Data_Chip4[i] = Data_final[i][3][1];


                            var SN_text = _page3.FindName($"Serial_Number_PCB_{i + 1}") as TextBox;
                            var OCR1_text = _page3.FindName($"Data_RD1_PCB_{i + 1}") as TextBox;
                            var OCR2_text = _page3.FindName($"Data_RD2_PCB_{i + 1}") as TextBox;
                            var OCR3_text = _page3.FindName($"Data_RD3_PCB_{i + 1}") as TextBox;
                            var OCR4_text = _page3.FindName($"Data_RD4_PCB_{i + 1}") as TextBox;

                            if (SN_text != null) SN_text.Text = SN[i];
                            if (OCR1_text != null) OCR1_text.Text = Data_Chip1[i];
                            if (OCR2_text != null) OCR2_text.Text = Data_Chip2[i];
                            if (OCR3_text != null) OCR3_text.Text = Data_Chip3[i];
                            if (OCR4_text != null) OCR4_text.Text = Data_Chip4[i];
                            try
                            {
                                string[] Clean_data1 = Data_Chip1[i].Split(",");
                                string[] Clean_data2 = Data_Chip2[i].Split(",");
                                string[] Clean_data3 = Data_Chip3[i].Split(",");
                                string[] Clean_data4 = Data_Chip4[i].Split(",");


                                if (SN[i] != "N/A" && !Data_Chip1[i].Contains(" ") && !Data_Chip2[i].Contains(" ") && !Data_Chip3[i].Contains(" ") && !Data_Chip4[i].Contains(" ")
                                    && !string.IsNullOrWhiteSpace(Data_Chip1[i]) && !string.IsNullOrWhiteSpace(Data_Chip2[i]) && !string.IsNullOrWhiteSpace(Data_Chip3[i]) && !string.IsNullOrWhiteSpace(Data_Chip4[i]))
                                {
                                    scan_ok += 1;
                                }
                                else if (SN[i] == "N/A" && Data_Chip1[i] == "           ,    ,         ,      " && Data_Chip2[i] == "           ,    ,         ,      " && Data_Chip3[i] == "           ,    ,         ,      " && Data_Chip4[i] == "           ,    ,         ,      ")
                                {
                                    scan_ok += 1;
                                }
                            }
                            catch
                            {
                                scan_ok += 0;
                            }

                        }
                    }
                    catch
                    {

                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            SN[i] = "";
                            Data_Chip1[i] = "";
                            Data_Chip2[i] = "";
                            Data_Chip3[i] = "";
                            Data_Chip4[i] = "";
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

                                string resign_url_OCR1 = string.Format(URL_Regist, SN[i], RD_1, Data_Chip1[i]);
                                string resign_url_OCR2 = string.Format(URL_Regist, SN[i], RD_2, Data_Chip2[i]);
                                string resign_url_OCR3 = string.Format(URL_Regist, SN[i], RD_3, Data_Chip3[i]);
                                string resign_url_OCR4 = string.Format(URL_Regist, SN[i], RD_4, Data_Chip4[i]);

                                string delete_url_OCR1 = string.Format(URL_Delete, RD_1, SN[i]);
                                string delete_url_OCR2 = string.Format(URL_Delete, RD_2, SN[i]);
                                string delete_url_OCR3 = string.Format(URL_Delete, RD_3, SN[i]);
                                string delete_url_OCR4 = string.Format(URL_Delete, RD_4, SN[i]);


                                string getdata_url_OCR1 = string.Format(URL_Get, RD_1, SN[i]);
                                string getdata_url_OCR2 = string.Format(URL_Get, RD_2, SN[i]);
                                string getdata_url_OCR3 = string.Format(URL_Get, RD_3, SN[i]);
                                string getdata_url_OCR4 = string.Format(URL_Get, RD_4, SN[i]);

                                // Delete
                                var swDelete = Stopwatch.StartNew();
                                await MES.RequestDataAsync(delete_url_OCR1, HttpMethod.Get);
                                swDelete.Stop();
                                double durationDelete_data_1 = swDelete.Elapsed.TotalMilliseconds;
                                string Delete_respone_data_1 = MES.GetData();

                                swDelete = Stopwatch.StartNew();
                                await MES.RequestDataAsync(delete_url_OCR2, HttpMethod.Get);
                                swDelete.Stop();
                                double durationDelete_data_2 = swDelete.Elapsed.TotalMilliseconds;
                                string Delete_respone_data_2 = MES.GetData();

                                swDelete = Stopwatch.StartNew();
                                await MES.RequestDataAsync(delete_url_OCR3, HttpMethod.Get);
                                swDelete.Stop();
                                double durationDelete_data_3 = swDelete.Elapsed.TotalMilliseconds;
                                string Delete_respone_data_3 = MES.GetData();

                                swDelete = Stopwatch.StartNew();
                                await MES.RequestDataAsync(delete_url_OCR4, HttpMethod.Get);
                                swDelete.Stop();
                                double durationDelete_data_4 = swDelete.Elapsed.TotalMilliseconds;
                                string Delete_respone_data_4 = MES.GetData();

                                // Register
                                var swReg = Stopwatch.StartNew();
                                await MES.RequestDataAsync(resign_url_OCR1, HttpMethod.Get);
                                swReg.Stop();
                                double durationReg_data_1 = swReg.Elapsed.TotalMilliseconds;
                                string Registered_respone_data_1 = MES.GetData();
                                // GetData
                                var swGet = Stopwatch.StartNew();
                                await MES.RequestDataAsync(getdata_url_OCR1, HttpMethod.Get);
                                swGet.Stop();
                                double durationGet_data_1 = swGet.Elapsed.TotalMilliseconds;
                                string Check_OCR1 = MES.GetData();

                                swReg = Stopwatch.StartNew();
                                await MES.RequestDataAsync(resign_url_OCR2, HttpMethod.Get);
                                swReg.Stop();
                                double durationReg_data_2 = swReg.Elapsed.TotalMilliseconds;
                                string Registered_respone_data_2 = MES.GetData();
                                swGet = Stopwatch.StartNew();
                                await MES.RequestDataAsync(getdata_url_OCR2, HttpMethod.Get);
                                swGet.Stop();
                                double durationGet_data_2 = swGet.Elapsed.TotalMilliseconds;
                                string Check_OCR2 = MES.GetData();

                                swReg = Stopwatch.StartNew();
                                await MES.RequestDataAsync(resign_url_OCR3, HttpMethod.Get);
                                swReg.Stop();
                                double durationReg_data_3 = swReg.Elapsed.TotalMilliseconds;
                                string Registered_respone_data_3 = MES.GetData();
                                swGet = Stopwatch.StartNew();
                                await MES.RequestDataAsync(getdata_url_OCR3, HttpMethod.Get);
                                swGet.Stop();
                                double durationGet_data_3 = swGet.Elapsed.TotalMilliseconds;
                                string Check_OCR3 = MES.GetData();

                                swReg = Stopwatch.StartNew();
                                await MES.RequestDataAsync(resign_url_OCR4, HttpMethod.Get);
                                swReg.Stop();
                                double durationReg_data_4 = swReg.Elapsed.TotalMilliseconds;
                                string Registered_respone_data_4 = MES.GetData();
                                swGet = Stopwatch.StartNew();
                                await MES.RequestDataAsync(getdata_url_OCR4, HttpMethod.Get);
                                swGet.Stop();
                                double durationGet_data_4 = swGet.Elapsed.TotalMilliseconds;
                                string Check_OCR4 = MES.GetData();



                                if ((Check_OCR1.Contains(Data_Chip1[i])) && (Check_OCR2.Contains(Data_Chip2[i])) && (Check_OCR3.Contains(Data_Chip3[i])) && (Check_OCR4.Contains(Data_Chip4[i])))
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
                                Result_show = $"{SN[i]}, Data_1:{Data_Chip1[i]} , Register Result: {OCR_result[i]} \n"
                                            + $"{SN[i]}, Data_2:{Data_Chip2[i]} , Register Result: {OCR_result[i]} \n"
                                            + $"{SN[i]}, Data_3:{Data_Chip3[i]} , Register Result: {OCR_result[i]} \n"
                                            + $"{SN[i]}, Data_4:{Data_Chip4[i]} , Register Result: {OCR_result[i]} \n"
                                            + Result_show;
                                _page3.Result.Text = Result_show;
                                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                string logEntry_data1 = $"Registered>>>> {timestamp} {SN[i]}, Data_1:{Data_Chip1[i]} , Register Result: {OCR_result[i]}";
                                string logEntry_data2 = $"Registered>>>> {timestamp} {SN[i]}, Data_2:{Data_Chip2[i]} , Register Result: {OCR_result[i]}";
                                string logEntry_data3 = $"Registered>>>> {timestamp} {SN[i]}, Data_3:{Data_Chip3[i]} , Register Result: {OCR_result[i]}";
                                string logEntry_data4 = $"Registered>>>> {timestamp} {SN[i]}, Data_4:{Data_Chip4[i]} , Register Result: {OCR_result[i]}";
                                logfile.WriteLog(logEntry_data1);
                                logfile.WriteLog(logEntry_data2);
                                logfile.WriteLog(logEntry_data3);
                                logfile.WriteLog(logEntry_data4);

                                try
                                {
                                    MES_logfile.WriteMESLog(SN[i], RD_1, timestamp, delete_url_OCR1, Delete_respone_data_1, durationDelete_data_1.ToString(), "DeleteDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_1, timestamp, resign_url_OCR1, Registered_respone_data_1, durationReg_data_1.ToString(), "RegistDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_1, timestamp, getdata_url_OCR1, Check_OCR1, durationGet_data_1.ToString(), "GetDataMESLog");

                                    MES_logfile.WriteMESLog(SN[i], RD_2, timestamp, delete_url_OCR2, Delete_respone_data_2, durationDelete_data_2.ToString(), "DeleteDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_2, timestamp, resign_url_OCR2, Registered_respone_data_2, durationReg_data_2.ToString(), "RegistDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_2, timestamp, getdata_url_OCR2, Check_OCR2, durationGet_data_2.ToString(), "GetDataMESLog");

                                    MES_logfile.WriteMESLog(SN[i], RD_3, timestamp, delete_url_OCR3, Delete_respone_data_3, durationDelete_data_3.ToString(), "DeleteDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_3, timestamp, resign_url_OCR3, Registered_respone_data_3, durationReg_data_3.ToString(), "RegistDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_3, timestamp, getdata_url_OCR3, Check_OCR3, durationGet_data_3.ToString(), "GetDataMESLog");

                                    MES_logfile.WriteMESLog(SN[i], RD_4, timestamp, delete_url_OCR4, Delete_respone_data_4, durationDelete_data_4.ToString(), "DeleteDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_4, timestamp, resign_url_OCR4, Registered_respone_data_4, durationReg_data_4.ToString(), "RegistDataMESLog");
                                    MES_logfile.WriteMESLog(SN[i], RD_4, timestamp, getdata_url_OCR4, Check_OCR4, durationGet_data_4.ToString(), "GetDataMESLog");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error writing MES log: " + ex.Message);
                                }
                            }
                            else
                            {
                                Result_show = $"{SN[i]}, Data_1:{Data_Chip1[i]} \n"
                                            + $"{SN[i]}, Data_2:{Data_Chip2[i]} \n"
                                            + $"{SN[i]}, Data_3:{Data_Chip3[i]} \n"
                                            + $"{SN[i]}, Data_4:{Data_Chip4[i]} \n"
                                            + Result_show;
                                _page3.Result.Text = Result_show;
                                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                string logEntry = $"Not Registered>>>> {timestamp} PCBA_[{i}] is X-Out";
                                logfile.WriteLog(logEntry);

                                success += 1;
                            }

                        }
                        if (success == 1)
                        {
                            _page3.Status_box_MODEL2.Text = "Regist Data Success";
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
                                var SN_text = _page3.FindName($"Serial_Number_PCB_{i + 1}") as TextBox;
                                var OCR1_text = _page3.FindName($"Data_RD1_PCB_{i + 1}") as TextBox;
                                var OCR2_text = _page3.FindName($"Data_RD2_PCB_{i + 1}") as TextBox;
                                var OCR3_text = _page3.FindName($"Data_RD3_PCB_{i + 1}") as TextBox;
                                var OCR4_text = _page3.FindName($"Data_RD4_PCB_{i + 1}") as TextBox;

                                if (SN_text != null) SN_text.Text = "";
                                if (OCR1_text != null) OCR1_text.Text = "";
                                if (OCR2_text != null) OCR2_text.Text = "";
                                if (OCR3_text != null) OCR3_text.Text = "";
                                if (OCR4_text != null) OCR4_text.Text = "";

                            }
                            scanner.Client("OUTOFF,1\r", 1);
                            await Task.Delay(100);

                        }
                        else
                        {
                            _page3.Status_box_MODEL2.Text = "Regist Data FAILED";
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
                                var SN_text = _page3.FindName($"Serial_Number_PCB_{i + 1}") as TextBox;
                                var OCR1_text = _page3.FindName($"Data_RD1_PCB_{i + 1}") as TextBox;
                                var OCR2_text = _page3.FindName($"Data_RD2_PCB_{i + 1}") as TextBox;
                                var OCR3_text = _page3.FindName($"Data_RD3_PCB_{i + 1}") as TextBox;
                                var OCR4_text = _page3.FindName($"Data_RD4_PCB_{i + 1}") as TextBox;

                                if (SN_text != null) SN_text.Text = "";
                                if (OCR1_text != null) OCR1_text.Text = "";
                                if (OCR2_text != null) OCR2_text.Text = "";
                                if (OCR3_text != null) OCR3_text.Text = "";
                                if (OCR4_text != null) OCR4_text.Text = "";
                            }
                            await Task.Delay(100);
                        }

                    }

                    else
                    {

                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string logEntry_data1 = $"Not Registered>>>> {timestamp} {SN[i]}, Data_1:{Data_Chip1[i]}";
                            string logEntry_data2 = $"Not Registered>>>> {timestamp} {SN[i]}, Data_2:{Data_Chip2[i]}";
                            string logEntry_data3 = $"Not Registered>>>> {timestamp} {SN[i]}, Data_3:{Data_Chip3[i]}";
                            string logEntry_data4 = $"Not Registered>>>> {timestamp} {SN[i]}, Data_4:{Data_Chip4[i]}";
                            logfile.WriteLog(logEntry_data1);
                            logfile.WriteLog(logEntry_data2);
                            logfile.WriteLog(logEntry_data3);
                            logfile.WriteLog(logEntry_data4);
                            scanner_result = scanner.Client("INCHK,2\r", 1);
                            decoded = Encoding.UTF8.GetString(scanner_result[0]);
                            parts = decoded.Split(',');
                            Photo_sensor = parts[2].Replace("\r", "");

                        } while (Photo_sensor == "ON" && StopFlag)
                        {
                            MessageBox.Show($"Scan Error !!!\n{SN[0]}\nData_1:{Data_Chip1[0]}" +
                                $"\nData_2:{Data_Chip2[0]}" +
                                $"\nData_3:{Data_Chip3[0]}" +
                                $"\nData_4:{Data_Chip4[0]}" +
                                $"\nPlease take out of PCB from Conveyor", "Checking !!!", MessageBoxButton.OK, MessageBoxImage.Error);
                            scanner_result = scanner.Client("INCHK,2\r", 1);
                            decoded = Encoding.UTF8.GetString(scanner_result[0]);
                            parts = decoded.Split(',');
                            Photo_sensor = parts[2].Replace("\r", "");
                            await Task.Delay(100);
                        }
                        for (int i = 0; i < number_of_PCB; i++)
                        {
                            var SN_text = _page3.FindName($"Serial_Number_PCB_{i + 1}") as TextBox;
                            var OCR1_text = _page3.FindName($"Data_RD1_PCB_{i + 1}") as TextBox;
                            var OCR2_text = _page3.FindName($"Data_RD2_PCB_{i + 1}") as TextBox;
                            var OCR3_text = _page3.FindName($"Data_RD3_PCB_{i + 1}") as TextBox;
                            var OCR4_text = _page3.FindName($"Data_RD4_PCB_{i + 1}") as TextBox;

                            if (SN_text != null) SN_text.Text = "";
                            if (OCR1_text != null) OCR1_text.Text = "";
                            if (OCR2_text != null) OCR2_text.Text = "";
                            if (OCR3_text != null) OCR3_text.Text = "";
                            if (OCR4_text != null) OCR4_text.Text = "";

                            Result_show = $"{SN[i]}, Data_1:{Data_Chip1[i]} , ERROR SCANNING  \n"
                                            + $"{SN[i]}, Data_2:{Data_Chip2[i]} , ERROR SCANNING  \n"
                                            + $"{SN[i]}, Data_3:{Data_Chip3[i]} , ERROR SCANNING  \n"
                                            + $"{SN[i]}, Data_4:{Data_Chip4[i]} , ERROR SCANNING  \n"
                                            + Result_show;
                        }
                        _page3.Result.Text = Result_show;
                        await Task.Delay(100);
                    }
                }
            }
        }
    }
}


