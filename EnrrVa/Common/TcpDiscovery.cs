using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace EnrrVa.Common
{
    public class TcpDiscovery
    {
        public static void tcpListen()
        {

            int port = 1300;

            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            byte[] bytes = new byte[1024];

            string data = "";
            string output = "";

            while (true)
            {
                output += "Waiting for a connection... \n";

                TcpClient client = server.AcceptTcpClient();

                output += "Connected... \n";

                NetworkStream stream = client.GetStream();

                int i;

                i = stream.Read(bytes, 0, bytes.Length);

                while (i != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    output += String.Format("Received: {0}", data);

                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    output += String.Format("Sent: {0}", data);

                    i = stream.Read(bytes, 0, bytes.Length);

                }

                // Shutdown and end connection
                client.Close();


            }

            File.WriteAllText(@"C:\dev\tcp.txt", data);
        }


    }
}
