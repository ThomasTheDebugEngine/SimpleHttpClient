using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleHttpClient
{
    class Server
    {
        private static IPAddress IpAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
        private static int Port = 8080;
        
        private static ManualResetEvent AllDone = new(false);

        private IPEndPoint ServerEndPoint = new IPEndPoint(IpAddress, Port);
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private IHypertext _HtmlEngine;


        public Server(IHypertext htmlengine)
        {
            _HtmlEngine = htmlengine;
        }



        public void StartServer()
        {
            
            Console.WriteLine("Starting server @ " + IpAddress + ": " + Port);

            try
            {
                ServerSocket.Bind(ServerEndPoint);
                ServerSocket.Listen(69);

                Console.WriteLine("server started");

                ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception ex)
            {
                if(ex is ObjectDisposedException || ex is SocketException)
                {
                    Console.WriteLine("WARN: sockets were not bound, restarting server");
                    ServerSocket.Close();
                    StartServer();
                }
            }

            while(true)
            {
                AllDone.Reset();

                try
                {
                    Console.WriteLine("server listening");
                    ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), ServerSocket);

                    if(ServerSocket == null)
                    {
                        throw new ArgumentNullException("WARN: ServerSocket is null, this will cause exceptions in callback");
                    }
                }
                catch(Exception ex)
                {
                    if(ex is ObjectDisposedException || ex is SocketException || ex is ArgumentNullException)
                    {
                        Console.WriteLine("WARN: sockets have been corrupted or disconnected, restarting server");
                        ServerSocket.Close();
                        StartServer();
                    }
                }
                
                AllDone.WaitOne();
            }
        }



        private void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();

            Socket ListenerSocket = (Socket)ar.AsyncState;

            try
            {
                Socket soc = ServerSocket.EndAccept(ar);

                StateObject state = new StateObject(); // don't dependency inject, causes socket handler to throw exception on 2nd request
                state.ClientSocket = soc;


                soc.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
            }
            catch(Exception ex)
            {
                if(ex is ObjectDisposedException || ex is SocketException)
                {
                    Console.WriteLine("WARN: request data is disposed or sockets are unreachable, restarting server");
                    ServerSocket.Close();
                    StartServer();
                }
            }
        }



        private void ReadCallback(IAsyncResult ar)
        {
            string request = null;

            StateObject state = (StateObject)ar.AsyncState;
            Socket soc = state.ClientSocket;

            int BytesLenght = soc.EndReceive(ar);

            if(BytesLenght > 0)
            {
                state.sbuild.Append(Encoding.ASCII.GetString(state.buffer, 0, BytesLenght));
                request = state.sbuild.ToString();

                Console.WriteLine("Read " + request.Length + " bytes from socket. \n Data : " + request);
                
                ServeIndex(_HtmlEngine.CompileWebsite(), soc); 
            }
        }



        private void ServeIndex(string body, Socket socket)
        {
            string Headers = "Content-Type: text/html\n";
            string HttpResponse = $"HTTP/1.1 200 OK\n{Headers}\n{body}\n";

            byte[] data = Encoding.ASCII.GetBytes(HttpResponse);
            try
            {
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
            catch(Exception ex)
            {
                if(ex is ObjectDisposedException || ex is SocketException)
                {
                    Console.WriteLine("WARN: sockets are not able to send data, restarting server");
                    socket.Close();
                    StartServer();
                }
            }
        }



        private void SendCallback(IAsyncResult ar)
        {

            Socket soc = (Socket)ar.AsyncState;

            int sentByteLength = soc.EndSend(ar);

            Console.WriteLine("sent bytes: " + sentByteLength);

            try
            { 
                soc.Shutdown(SocketShutdown.Both);
            }
            catch(Exception ex)
            {
                if(ex is ObjectDisposedException || ex is SocketException)
                {
                    Console.WriteLine("WARN: Socket connection is already shut down, skipping to next step of graceful shutdown");
                    ServerSocket.Close();
                }
            }
            soc.Close();
        }
    }
}
