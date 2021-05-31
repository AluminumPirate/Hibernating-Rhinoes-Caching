using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading;

namespace Hibernating_Rhinos_Caching_Server
{
  // Socket Listener acts as a server and listens to the incoming   
  // messages on the specified port and protocol.  
  public class Hibernating_Rhinos_Caching_Client
  {
    // DataHolder is a data structure for the server cache purpose
    public static DataHolder ServerCachedData = new DataHolder();
    public const int PORT = 10011;

    public static int Main(String[] args)
    {
      CacheServer();
      return 0;
    }


    public static void CacheServer()
    {
      // Get Host IP Address that is used to establish a connection  
      // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
      // If a host has multiple addresses, you will get a list of addresses  
      IPHostEntry host = Dns.GetHostEntry("localhost");
      IPAddress ipAddress = host.AddressList[0];
      IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

      // Create a Socket that will use Tcp protocol      
      Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      
      // A Socket must be associated with an endpoint using the Bind method  
      listener.Bind(localEndPoint);
      
      // Specify how many requests a Socket can listen before it gives Server busy response.  
      // We will listen 100 requests at a time  
      listener.Listen(100);

      List<Thread> client_threads = new List<Thread>();
      
      while (true)
      {
        try
        {
          Console.WriteLine("Waiting for a connection...");

          // Accespt client socket
          Socket client_socket = listener.Accept();
          Console.WriteLine("Client Connected");
         
          // Create thread and set background true
          Thread client_thread = new Thread(() => ClientHandler(client_socket))
          {
            IsBackground = true
          };
          // Start thread
          client_thread.Start();

          client_threads.Add(client_thread);
        }
        catch (Exception e)
        {
          Console.WriteLine(e.ToString());
        }
      }
    }


    public static void ClientHandler(Socket client_socket)
    {
      while (true)
      {

        string data = null;
        byte[] bytes = new byte[1024];


        // Get data from client
        int bytesRec = client_socket.Receive(bytes);
        while (bytesRec <= 0)
        {
          bytesRec = client_socket.Receive(bytes);
        }

        // Get data as string
        data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

        // Check string isn't empty
        if (data.Length < 1)
        {
          return;
        }
        // If startst with set 
        else if (data.StartsWith("set "))
        {
          Console.WriteLine("set found: " + data);
          string[] set_command = data.Split(" ");

          // If length is right -> 3
          if (set_command.Length == 3)
          {
            // Notify user set arrived
            client_socket.Send(Encoding.ASCII.GetBytes("Continue set parameter")); // Just send something back


            // Get client set string
            bytes = new byte[1024];
            bytesRec = client_socket.Receive(bytes);
            string suggested_value_data = Encoding.ASCII.GetString(bytes, 0, bytesRec); ;
           
            try
            {
              // Check length is right
              int data_suggested_size = Convert.ToInt32(set_command[2]);

              // Return no valid size if length > prompted length
              if (suggested_value_data.Length > data_suggested_size)
              {
                client_socket.Send(Encoding.ASCII.GetBytes("not a valid size"));
              }
              else
              {
                // Check if Name already exists and notify client
                bool already_exists = false;
                foreach (Entry entry in ServerCachedData.Enrties)
                {
                  if (set_command[1] == entry.Name)
                  {
                    already_exists = true;
                    client_socket.Send(Encoding.ASCII.GetBytes("Already Exists"));
                    break;
                  }
                }
                // If not exists already, sets data and send OK
                if (!already_exists)
                {
                  ServerCachedData.Enrties.Add(new Entry(set_command[1], suggested_value_data, Convert.ToInt32(data_suggested_size)));
                  client_socket.Send(Encoding.ASCII.GetBytes("OK"));
                }
              }
            }
            catch (Exception)
            {
              client_socket.Send(Encoding.ASCII.GetBytes("error parsing int"));
              throw;
            }
          }
          else
          {
            // Notify client for not valid command
            client_socket.Send(Encoding.ASCII.GetBytes("not a valid command"));
          }
        }
        else if (data.StartsWith("get "))
        {
          // If data starts with get, Notify client get parameter arrived

          Console.WriteLine("get found: " + data);

          string[] get_command = data.Split(" ");
          client_socket.Send(Encoding.ASCII.GetBytes("get parameter"));

          // If get length 2 
          if (get_command.Length == 2)
          {
            try
            {
              // Check if entry found
              bool enrty_found = false;
              foreach (Entry entry in ServerCachedData.Enrties)
              {
                // If found send to client  "OK" and value. and set entry found to true 
                if (entry.Name == get_command[1])
                {
                  enrty_found = true;
                  client_socket.Send(Encoding.ASCII.GetBytes($"OK {entry.Size}\n{entry.Value}"));
                  break;
                }
              }

              // If entry not found, send "MISSING"
              if (!enrty_found)
              {
                client_socket.Send(Encoding.ASCII.GetBytes("MISSING"));
              }
            }
            catch (Exception)
            {

              throw;
            }
          }
        }
        // If "quit" arrived, shutdown and close socket, also returns "quit" to client
        else if (data == "quit")
        {
          client_socket.Send(Encoding.ASCII.GetBytes(data));
          client_socket.Shutdown(SocketShutdown.Both);
          client_socket.Close();
          Console.WriteLine("Clinet Disconnected");
          return;
        }
      }
    }
  
  }
}