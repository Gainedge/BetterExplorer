/*
 *  Copyright 2008 Alessandro Pilotti
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation; either version 2.1 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA 
 */

using System.IO;

namespace AlexPilotti.FTPS.Common
{
    internal delegate void FTPStreamCallback();

    /// <summary>
    /// Incapsulates a Stream used during FTP get and put commands.
    /// </summary>
    public class FTPStream : Stream
    {
        public enum EAllowedOperation { Read = 1, Write = 2 }

        Stream innerStream;
        FTPStreamCallback streamClosedCallback;
        EAllowedOperation allowedOp;

        internal FTPStream(Stream innerStream, EAllowedOperation allowedOp, FTPStreamCallback streamClosedCallback)
        {
            this.innerStream = innerStream;
            this.streamClosedCallback = streamClosedCallback;
            this.allowedOp = allowedOp;
        }

        public override bool CanRead
        {
            get { return innerStream.CanRead && (allowedOp & EAllowedOperation.Read) == EAllowedOperation.Read; }
        }

        public override bool CanSeek
        {
            get { return innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return innerStream.CanWrite && (allowedOp & EAllowedOperation.Write) == EAllowedOperation.Write; }
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override long Length
        {
            get { return innerStream.Length; }
        }

        public override long Position
        {
            get
            {
                return innerStream.Position;
            }
            set
            {
                innerStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new FTPException("Operation not allowed");

            return innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new FTPException("Operation not allowed");

            innerStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            base.Close();
            streamClosedCallback();
        }
    }
}
