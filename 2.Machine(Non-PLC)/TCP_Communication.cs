using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace service;
public class TCP_Communication
{
    public string TCP_IP;
    public int TCP_PORT;
    public int TCP_BUFFER_SIZE;
    public int check_connection;
    public TCP_Communication(string ip, int port, int buffer_size)
    {
        TCP_IP = ip;
        TCP_PORT = port;
        TCP_BUFFER_SIZE = buffer_size;
        check_connection = 0;

    }
    public bool Check()
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                var result = client.BeginConnect(TCP_IP, TCP_PORT, null, null);

                // ✅ รอแค่ 1000ms (1 วินาที) เท่านั้น ถ้าไม่ตอบ = ล้มเหลว
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(1000));

                if (!success)
                    return false;

                client.EndConnect(result); // ✅ ต้องปิดการเชื่อมต่อหลัง success
                return true;
            }
        }
        catch
        {
            //MessageBox.Show("failed");
            return false;
        }
    }
    public byte[][] Client(string message, int dataAmount)
    {
        TcpClient client = null;

        while (check_connection == 0)
        {
            try
            {
                client = new TcpClient();
                client.Connect(TCP_IP, TCP_PORT);
                check_connection = 1;
            }
            catch
            {
                MessageBox.Show($"Connection Failed !!!\nPlease TCP Connection again", "TCP Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        byte[][] data = new byte[dataAmount][];
        NetworkStream stream = client.GetStream();

        // ส่ง message
        byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
        stream.Write(sendBuffer, 0, sendBuffer.Length);

        // รับข้อมูลตามจำนวน dataAmount
        for (int i = 0; i < dataAmount; i++)
        {
            byte[] recvBuffer = new byte[TCP_BUFFER_SIZE];
            int bytesRead = stream.Read(recvBuffer, 0, TCP_BUFFER_SIZE);

            byte[] actualData = new byte[bytesRead];
            Array.Copy(recvBuffer, actualData, bytesRead);
            data[i] = actualData;
        }

        stream.Close();
        client.Close();
        check_connection = 0;

        return data;
    }
}