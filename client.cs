using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace TcpClientImagePlayback
{
    /// <summary>
    /// A class to send messages by the client to the TCP server
    /// </summary>
    public class Client
    {
        public string serverAddress = "127.0.0.1";
        public int port = 5000;
        /// <summary>
        /// Sends Message from client to the server.
        /// </summary>
        /// <param name="message"></param>
        public void SendImageToServer()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (TcpClient tcpClient = new TcpClient(serverAddress, port))
                    {
                        using (NetworkStream stream = tcpClient.GetStream())
                        {
                            
                            string[] files = openFileDialog.FileNames;
                            byte[] fileCountBuffer = BitConverter.GetBytes      (files.Length);
                            Logger.log.Info($"Client fileCount:     {fileCountBuffer.Length}");

                            //filecount
                            stream.Write(fileCountBuffer, 0,    fileCountBuffer.Length);

                            foreach (string filePath in files)
                            {
                                string fileName = Path.GetFileName(filePath);
                                Logger.log.Info($"Client name: {fileName}");

                                byte[] fileNameBytes =  System.Text.Encoding.UTF8.GetBytes (fileName);

                                Logger.log.Info($"Client nameBytes: {fileNameBytes.Length}");

                                byte[] fileData = File.ReadAllBytes(filePath);

                                byte[] fileNameLengthBuffer = BitConverter.GetBytes (fileNameBytes.Length);

                                Logger.log.Info($"Client nameLength: {fileNameLengthBuffer.Length}");

                                byte[] fileLengthBuffer = BitConverter.GetBytes(fileData.Length);
                                Logger.log.Info($"Client fileLength: {fileLengthBuffer.Length}");

                                stream.Write(fileNameLengthBuffer, 0, fileNameLengthBuffer.Length);
                                stream.Write(fileNameBytes, 0, fileNameBytes.Length);
                                stream.Write(fileLengthBuffer, 0, fileLengthBuffer.Length);
                                stream.Write(fileData, 0, fileData.Length);
                                
                                stream.Flush();
                            }
                            
                        }
                    }
                    MessageBox.Show("Image Sent Successfully !", "Client", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                catch (Exception ex)
                {
                    Logger.log.Error("Error in sending message to server", ex);
                    MessageBox.Show($"Error in sending message to server: {ex.Message}", "Client", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}