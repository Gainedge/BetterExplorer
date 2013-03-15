using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading;

namespace PipesServer
{
    // Delegate for passing received message back to caller
    public delegate void DelegateMessage(string Reply);

    public class PipeServer
    {
        public event DelegateMessage PipeMessage;
      public event EventHandler PipeFinished;
        string _pipeName;

        public void Listen(string PipeName)
        {
            try
            {
                // Set to class level var so we can re-use in the async callback method
                _pipeName = PipeName;
                PipeSecurity ps = new PipeSecurity();
                ps.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, AccessControlType.Allow));
                // Create the new async pipe 
                NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 65535, 65535, ps);

                // Wait for a connection
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            }
            catch (Exception oEX)
            {
                Debug.WriteLine(oEX.Message);
            }
        }

        private void StartNewPipe(object param) {
          try {
            // Get the pipe
            NamedPipeServerStream pipeServer = (NamedPipeServerStream)param;
            // End waiting for the connection
            pipeServer.WaitForConnection();

            byte[] buffer = new byte[65535];

            // Read the incoming message
            pipeServer.Read(buffer, 0, 65535);
            // Convert byte buffer to string
            string stringData = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
            Debug.WriteLine(stringData + Environment.NewLine);

            // Pass message back to calling form
            PipeMessage.Invoke(stringData);

            // Kill original sever and create new wait server
            pipeServer.Close();
            pipeServer = null;
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 65535, 65535, ps);
            
            // Recursively wait for the connection again and again....
            pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            
          } catch {
            return;
          }
        }

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState;
                // End waiting for the connection
                pipeServer.EndWaitForConnection(iar);

                //Thread t = new Thread(new ParameterizedThreadStart(StartNewPipe));
                //t.Start(pipeServer);

                byte[] buffer = new byte[65535];

                // Read the incoming message
                pipeServer.Read(buffer, 0, 65535);
                // Convert byte buffer to string
                string stringData = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                Debug.WriteLine(stringData + Environment.NewLine);

                // Pass message back to calling form
                PipeMessage.Invoke(stringData);

                // Kill original sever and create new wait server
                pipeServer.Close();
                pipeServer = null;
                PipeSecurity ps = new PipeSecurity();
                ps.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, AccessControlType.Allow));
                pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 65535, 65535, ps);

                // Recursively wait for the connection again and again....
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
                if (PipeFinished != null)
                  PipeFinished.Invoke(this, new EventArgs());
            }
            catch
            {
                return;
            }
        }
    }
}
