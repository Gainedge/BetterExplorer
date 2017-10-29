using System;
using System.Collections.Generic;
using System.IO;

namespace wyUpdate.Common
{
    internal class UpdateHelperData
    {
        public UpdateAction Action;
        public UpdateStep UpdateStep;


        public List<string> ExtraData = new List<string>();
        public List<bool> ExtraDataIsRTF = new List<bool>();

        public Response ResponseType = Response.Nothing;
        public int Progress = -1;


        public int ProcessID;


        public UpdateHelperData() { }


        public UpdateHelperData(UpdateAction action)
        {
            Action = action;
        }

        public UpdateHelperData(UpdateStep step)
        {
            Action = UpdateAction.UpdateStep;
            UpdateStep = step;
        }

        public UpdateHelperData(Response responseType, UpdateStep step)
            : this(step)
        {
            ResponseType = responseType;
        }

#if CLIENT
        public UpdateHelperData(Response responseType, UpdateStep step, int progress)
            : this(responseType, step)
        {
            Progress = progress;
        }
#endif

        public UpdateHelperData(Response responseType, UpdateStep step, string messageTitle, string messageBody)
            : this(responseType, step)
        {
            ExtraData.Add(messageTitle);
            ExtraData.Add(messageBody);

            ExtraDataIsRTF.Add(false);
            ExtraDataIsRTF.Add(false);
        }

        public byte[] GetByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriteFiles.WriteInt(ms, 0x01, (int)Action);

                // what update step are we on?
                WriteFiles.WriteInt(ms, 0x02, (int)UpdateStep);

                // write extra string data
                for (int i = 0; i < ExtraData.Count; i++)
                {
                    if (!string.IsNullOrEmpty(ExtraData[i]))
                    {
                        if (ExtraDataIsRTF[i])
                            ms.WriteByte(0x80);

                        WriteFiles.WriteString(ms, 0x03, ExtraData[i]);
                    }
                }

                // 0x07, 0x08, and 0x09 used to be links data - obsolete

                if (ProcessID != 0)
                    WriteFiles.WriteInt(ms, 0x04, ProcessID);

                if (Progress > -1 && Progress <= 100)
                    WriteFiles.WriteInt(ms, 0x05, Progress);

                if (ResponseType != Response.Nothing)
                    WriteFiles.WriteInt(ms, 0x06, (int)ResponseType);

                ms.WriteByte(0xFF);

                return ms.ToArray();
            }
        }

        public static UpdateHelperData FromByteArray(byte[] data)
        {
            UpdateHelperData uhData = new UpdateHelperData();

            using (MemoryStream ms = new MemoryStream(data))
            {
                byte bType = (byte)ms.ReadByte();

                //read until the end byte is detected
                while (!ReadFiles.ReachedEndByte(ms, bType, 0xFF))
                {
                    switch (bType)
                    {
                        case 0x01:
                            uhData.Action = (UpdateAction)ReadFiles.ReadInt(ms);
                            break;
                        case 0x02: // update step we're on
                            uhData.UpdateStep = (UpdateStep)ReadFiles.ReadInt(ms);
                            break;
                        case 0x80:
                            uhData.ExtraDataIsRTF.Add(true);
                            break;
                        case 0x03: // extra data

                            uhData.ExtraData.Add(ReadFiles.ReadString(ms));

                            // keep the 'ExtraDataIsRTF' same length as ExtraData
                            if (uhData.ExtraDataIsRTF.Count != uhData.ExtraData.Count)
                                uhData.ExtraDataIsRTF.Add(false);

                            break;
                        case 0x04:
                            uhData.ProcessID = ReadFiles.ReadInt(ms);
                            break;
                        case 0x05:
                            uhData.Progress = ReadFiles.ReadInt(ms);
                            break;
                        case 0x06:
                            uhData.ResponseType = (Response)ReadFiles.ReadInt(ms);
                            break;

                        // 0x07, 0x08, and 0x09 used to be links data - obsolete

                        default:
                            ReadFiles.SkipField(ms, bType);
                            break;
                    }

                    bType = (byte)ms.ReadByte();
                }
            }

            return uhData;
        }

        public static string PipenameFromFilename(string filename)
        {
            // get the unique pipe name (the last 246 chars of the complete path)
            string pipeName = filename.Replace("\\", "").ToLower();
            int pipeNameL = pipeName.Length;
            return "\\\\.\\pipe\\" + pipeName.Substring(Math.Max(0, pipeNameL - 246), Math.Min(246, pipeNameL));
        }
    }

    internal enum UpdateAction { UpdateStep = 0, GetwyUpdateProcessID = 1, Cancel = 2, NewWyUpdateProcess = 3 }
    internal enum UpdateStep { CheckForUpdate = 0, ForceRecheckForUpdate = 5, DownloadUpdate = 1, BeginExtraction = 2, RestartInfo = 3, Install = 4 }
    internal enum Response { Failed = -1, Nothing = 0, Succeeded = 1, Progress = 2 }
}