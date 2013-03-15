using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Diagnostics;
using System.Security.AccessControl;

namespace PipesClient
{
    public class PipeClient
    {
      public event EventHandler PipeFinished;
        public void Send(string SendStr, string PipeName, int TimeOut = 2000)
        {
            try
            {
                NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                // The connect function will indefinitely wait for the pipe to become available
                // If that is not acceptable specify a maximum waiting time (in ms)

                int i = 0;
                  while (!pipeStream.IsConnected) {
                    try {
                      pipeStream.Connect(100);
                      Debug.WriteLine("[Client] Pipe connection established");

                      byte[] _buffer = Encoding.Unicode.GetBytes(SendStr);
                      pipeStream.BeginWrite(_buffer, 0, _buffer.Length, AsyncSend, pipeStream);
                      break;
                    } catch (Exception) {

                    }
                    if (i >= 10)
                      break;
                    i++;
                  }
    
                 
               
            }
            catch (TimeoutException oEX)
            {
                Debug.WriteLine(oEX.Message);
            }
        }

        private void AsyncSend(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeClientStream pipeStream = (NamedPipeClientStream)iar.AsyncState;

                // End the write
                pipeStream.EndWrite(iar);
                pipeStream.Flush();
                pipeStream.Close();
                pipeStream.Dispose();
                //PipeFinished.Invoke(this, new EventArgs());
                
            }
            catch (Exception oEX)
            {
                Debug.WriteLine(oEX.Message);
            }
        }
    }
}
