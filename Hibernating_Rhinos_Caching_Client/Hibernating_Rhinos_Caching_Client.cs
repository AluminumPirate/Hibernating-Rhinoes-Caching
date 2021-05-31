using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;

namespace Hibernating_Rhinos_Caching_Client
{ 
  public class Hibernating_Rhinos_Caching_Client
  {
    public const int PORT = 10011;
    public static int Main(String[] args)
    {
      StartClient();
      return 0;
    }


    public static void StartClient()
    {
      byte[] bytes = new byte[1024];

      try
      {
        // Connect to a Remote server  
        // Get Host IP Address that is used to establish a connection  
        // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
        // If a host has multiple addresses, you will get a list of addresses  
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

        // Create a TCP/IP  socket.    
        Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Connect to Remote EndPoint  
        sender.Connect(remoteEP);

        Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
        // Connect the socket to the remote endpoint. Catch any errors.    

        while (true)
        {
          try
          {
            byte[] msg = Encoding.ASCII.GetBytes(Console.ReadLine()); ;
            while (msg == null || msg.Length <= 0)
            {
              // Encode the data string into a byte array.    
              msg = Encoding.ASCII.GetBytes(Console.ReadLine());
            }

            // Send the data through the socket.    
            int bytesSent = sender.Send(msg);

            // Receive the response from the remote device.    
            int bytesRec = sender.Receive(bytes);

            string recieved_data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            //Console.WriteLine("Echoed test = {0}", recieved_data);

            if (recieved_data == "Continue set parameter")
            {
              msg = Encoding.ASCII.GetBytes(Console.ReadLine());
              bytesSent = sender.Send(msg);

              // RECIEVE ERROR OR OK BACK
              bytesRec = sender.Receive(bytes);
              recieved_data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
              Console.WriteLine(recieved_data);
            }
            else if (recieved_data == "get parameter")
            {
              bytesRec = sender.Receive(bytes);
              recieved_data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
              Console.WriteLine(recieved_data);
            }
            else if (recieved_data == "quit")
            {
              // Release the socket.    
              sender.Shutdown(SocketShutdown.Both);
              sender.Close();
              return;
            }
            else
            {
              Console.WriteLine(recieved_data);
            }
          }
          catch (Exception e)
          {
            Console.WriteLine(e.ToString());
          }
        }
      }
      catch (ArgumentNullException ane)
      {
        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
      }
      catch (SocketException se)
      {
        Console.WriteLine("SocketException : {0}", se.ToString());
      }
      catch (Exception e)
      {
        Console.WriteLine("Unexpected exception : {0}", e.ToString());
      }
    }
  }

}

