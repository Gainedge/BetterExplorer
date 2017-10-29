using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace wyDay.Controls
{
    /// <summary>Allow pipe communication between a server and a client.</summary>
    public class PipeClient : IDisposable
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern SafeFileHandle CreateFile(
           string pipeName,
           uint dwDesiredAccess,
           uint dwShareMode,
           IntPtr lpSecurityAttributes,
           uint dwCreationDisposition,
           uint dwFlagsAndAttributes,
           IntPtr hTemplate);

        /// <summary>Handles messages received from a server pipe</summary>
        /// <param name="message">The byte message received</param>
        public delegate void MessageReceivedHandler(byte[] message);

        /// <summary>Event is called whenever a message is received from the server pipe</summary>
        public event MessageReceivedHandler MessageReceived;

        /// <summary>Handles server disconnected messages</summary>
        public delegate void ServerDisconnectedHandler();

        /// <summary>Event is called when the server pipe is severed.</summary>
        public event ServerDisconnectedHandler ServerDisconnected;

        const int BUFFER_SIZE = 4096;

        FileStream stream;
        SafeFileHandle handle;
        Thread readThread;

        /// <summary>Gets if this client connected to a server pipe.</summary>
        public bool Connected { get; private set; }

        /// <summary>The pipe this client is connected to.</summary>
        public string PipeName { get; private set; }

        #region Dispose

        /// <summary>Indicates whether this instance is disposed.</summary>
        bool isDisposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="PipeClient"/> class.
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PipeClient"/> is reclaimed by garbage collection.
        /// </summary>
        ~PipeClient()
        {
            Dispose(false);
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">Result: <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                // Not already disposed ?
                if (disposing)
                {
                    // dispose managed resources
                    Disconnect();
                }

                // free unmanaged resources
                // Set large fields to null.

                // Instance is disposed
                isDisposed = true;
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>Connects to the server with a pipename.</summary>
        /// <param name="pipename">The name of the pipe to connect to.</param>
        public void Connect(string pipename)
        {
            if (Connected)
                throw new Exception("Already connected to pipe server.");

            PipeName = pipename;

            handle =
               CreateFile(
                  PipeName,
                  0xC0000000, // GENERIC_READ | GENERIC_WRITE = 0x80000000 | 0x40000000
                  0,
                  IntPtr.Zero,
                  3, // OPEN_EXISTING
                  0x40000000, // FILE_FLAG_OVERLAPPED
                  IntPtr.Zero);

            // could not create handle - server probably not running
            if (handle.IsInvalid)
            {
                handle = null;
                return;
            }

            Connected = true;

            // start listening for messages
            readThread = new Thread(Read) { IsBackground = true };
            readThread.Start();
        }

        /// <summary>Disconnects from the server.</summary>
        public void Disconnect()
        {
            if (!Connected)
                return;

            // we're no longer connected to the server
            Connected = false;
            PipeName = null;

            // clean up resources
            if (stream != null)
                stream.Dispose();

            // If Connected == true then handle is non-null.
            // Thus, just dispose it.
            handle.Dispose();

            stream = null;
            handle = null;
        }

        void Read()
        {
            stream = new FileStream(handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] readBuffer = new byte[BUFFER_SIZE];

            while (true)
            {
                int bytesRead = 0;

                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        // read the total stream length
                        int totalSize = stream.Read(readBuffer, 0, 4);

                        // client has disconnected
                        if (totalSize == 0)
                            break;

                        totalSize = BitConverter.ToInt32(readBuffer, 0);

                        do
                        {
                            int numBytes = stream.Read(readBuffer, 0, Math.Min(totalSize - bytesRead, BUFFER_SIZE));

                            ms.Write(readBuffer, 0, numBytes);

                            bytesRead += numBytes;

                        } while (bytesRead < totalSize);
                    }
                    catch
                    {
                        //read error has occurred
                        break;
                    }

                    //client has disconnected
                    if (bytesRead == 0)
                        break;

                    //fire message received event
                    if (MessageReceived != null)
                        MessageReceived(ms.ToArray());
                }
            }

            // if connected, then the disconnection was
            // caused by a server terminating, otherwise it was from
            // a call to Disconnect()
            if (Connected)
            {
                //clean up resource
                stream.Dispose();
                handle.Dispose();

                stream = null;
                handle = null;

                // we're no longer connected to the server
                Connected = false;
                PipeName = null;

                if (ServerDisconnected != null)
                    ServerDisconnected();
            }
        }

        /// <summary>Sends a message to the server.</summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the message is sent successfully - false otherwise.</returns>
        public bool SendMessage(byte[] message)
        {
            try
            {
              if (stream != null) {
                // write the entire stream length
                stream.Write(BitConverter.GetBytes(message.Length), 0, 4);

                stream.Write(message, 0, message.Length);
                stream.Flush();
                return true;
              } else {
                return false;
              }
            }
            catch
            {
                return false;
            }
        }
    }
}