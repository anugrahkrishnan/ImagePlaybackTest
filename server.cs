using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.IO;

namespace Tasktest
{
    /// <summary>
    /// this class act as Server
    /// </summary>
    public class TcpServer
    {
        private TcpListener tcpListener;
        private Thread listenerThread;
        private bool isRunning;
       

        /// <summary>
        /// Tcp server listens for incoming client connection on port 5000.
        /// </summary>
        public void Start()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 5000);
                tcpListener.Start();
                isRunning = true;
                listenerThread = new Thread(ListenForClients);
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                Logger.log.Error("Error in starting the TCP server", ex);
            }
        }
        /// <summary>
        /// Listens for incoming TCP client connection.
        /// </summary>
        private void ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(HandleClient);
                    clientThread.Start(tcpClient);

                }
                catch (Exception ex)
                {
                    Logger.log.Error("Error in connecting the client", ex);
                    MessageBox.Show($"Server error: {ex.Message}", "Server", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Handle the communication between server and the connected client.
        /// </summary>
        /// <param name="obj"></param>
        private void HandleClient(object obj)
        {
            try
            {
                TcpClient tcpClient = obj as TcpClient;

                using (NetworkStream stream = tcpClient.GetStream())
                {
                    
                        string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string newFolderPath = Path.Combine(projectDirectory, "ReceivedImages");

                        if (Directory.Exists(newFolderPath))
                        {
                            Directory.Delete(newFolderPath, true);
                        }
                        Directory.CreateDirectory(newFolderPath);


                        byte[] fileCountBuffer = new byte[1024];
                        stream.Read(fileCountBuffer, 0, fileCountBuffer.Length);
                        int fileCount = BitConverter.ToInt32(fileCountBuffer,0);
                        Logger.log.Info($"Server fileCount: {fileCount}");
                   


                    Application.Current.Dispatcher.Invoke(() => Config.viewModel.ImagePaths.Clear());


                        for (int i = 0; i < fileCount; i++)
                        {
                            byte[] fileNameLengthBuffer = new byte[1024];
                            stream.Read(fileNameLengthBuffer, 0, fileNameLengthBuffer.Length);
                            int fileNameLength = BitConverter.ToInt32(fileNameLengthBuffer,0);
                        

                            Logger.log.Info($"Server nameLength: {fileNameLength}");


                            byte[] fileNameBuffer = new byte[1024];
                            stream.Read(fileNameBuffer, 0, fileNameBuffer.Length);
                            string fileName = System.Text.Encoding.UTF8.GetString(fileNameBuffer);
                            
                            Logger.log.Info($"Server name: {fileName}");

                            //fileName = SantizeFileName(fileName);

                            byte[] fileLengthBuffer = new byte[1024];
                            stream.Read(fileLengthBuffer, 0, fileLengthBuffer.Length);
                            long fileSize = BitConverter.ToInt64(fileLengthBuffer,0);
                            Logger.log.Info($"Server fileSize: {fileSize}");


                            byte[] fileData = new byte[1024];
                            int bytesRead = 0;

                            while(bytesRead < fileSize)
                            {
                                bytesRead += stream.Read(fileData, bytesRead, (int)(fileSize - bytesRead));
                            }

                                //string fileName = reader.ReadString();
                                //Logger.log.Info($"name: {fileName}");

                                //long fileSize = reader.ReadInt64();
                                //byte[] fileData = reader.ReadBytes((int)fileSize);
                                string filePath = Path.Combine(newFolderPath, fileName);

                                Logger.log.Info($"path: {filePath}");

                                File.WriteAllBytes(filePath, fileData);

                                Application.Current.Dispatcher.Invoke(() =>         Config.viewModel.ImagePaths.Add(filePath));
                            
                        }
                                          
                }
                Logger.log.Info("Images Received Successfully");
                MessageBox.Show($"Images Received Successfully", "Server", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.log.Error("Error handling client", ex);
                MessageBox.Show($"Error handling client: {ex.Message}", "Server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        private string SantizeFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }
            return fileName;
        }
        /// <summary>
        /// Stop the server.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            tcpListener.Stop();
            listenerThread?.Join();
        }
    }
}