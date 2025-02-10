using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DAQPort = AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.IOUtility.TCP;
using DAQMaster = AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol.Master;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol;
using System.Runtime.InteropServices;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Storage;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Workloads
{
    internal class IOConfiguration
    {
        public static (VariableDictionary?, ControllerConfiguration?, ObjectDictionary?,
                    ProcessDataImage?, ProcessDataImage?, ProcessDataImage?,
                    ProcessDataImage?, ProcessDataImage?, ProcessDataImage?, 
                    InterlockCollection?, Miscellaneous?,
                    byte[]? varHash, byte[]? ioHash,
                    Exception?) UploadOfflineIOConfiguration(FTPTargetProperty ftp, DataTypeCatalogue dataTypes, ControllerModelCatalogue controllerModels)
        {
            try
            {
                NetworkCredential cred = null;
                if (ftp.User != null && ftp.User.Trim().Length > 0 && ftp.Password != null && ftp.Password.Trim().Length > 0)
                    cred = new NetworkCredential(ftp.User.Trim(), ftp.Password.Trim());
                FtpWebRequest request;
                VariableDictionary vd;
                ControllerConfiguration cc;
                ObjectDictionary od;
                ProcessDataImage txdiag, txbit, txblk, rxctl, rxbit, rxblk;
                InterlockCollection intlk;
                Miscellaneous misc;
                byte[] varHash, ioHash;

#pragma warning disable SYSLIB0014 // 类型或成员已过时
                request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + ftp.HostIPv4 + ":" + ftp.HostPort.ToString() + ftp.VariableDictionaryPath);
#pragma warning restore SYSLIB0014 // 类型或成员已过时
                request.Credentials = cred;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.Timeout = ftp.TimeoutValue;
                request.ReadWriteTimeout = ftp.ReadWriteTimeoutValue;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (System.IO.Stream sm = response.GetResponseStream())
                using (System.IO.MemoryStream ms = new MemoryStream())
                {
                    sm.CopyTo(ms);
                    ms.Position = 0;
                    vd = IOCelcetaHelper.Import(ms, dataTypes);
                    using (MD5 hash = MD5.Create())
                    {
                        ms.Position = 0;
                        varHash = hash.ComputeHash(ms);
                    }
                    ms.Close();
                    sm.Close();
                    response.Close();
                }

#pragma warning disable SYSLIB0014 // 类型或成员已过时
                request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + ftp.HostIPv4 + ":" + ftp.HostPort.ToString() + ftp.IOListPath);
#pragma warning restore SYSLIB0014 // 类型或成员已过时
                request.Credentials = cred;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;


                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (System.IO.Stream sm = response.GetResponseStream())
                using (System.IO.MemoryStream ms = new MemoryStream())
                {
                    sm.CopyTo(ms);
                    ms.Position = 0;
                    (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = IOCelcetaHelper.Import(ms, vd, dataTypes, controllerModels);
                    using (MD5 hash = MD5.Create())
                    {
                        ms.Position = 0;
                        ioHash = hash.ComputeHash(ms);
                    }
                    ms.Close();
                    sm.Close();
                    response.Close();
                }

                return (vd, cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc, varHash, ioHash, null);
            }
            catch(Exception ex)
            {
                return (null, null, null, null, null, null, null, null, null, null, null, null, null, ex);
            }
        }

        public static (DAQ_SERVER_INFO_T?, DAQ_SERVER_IO_HASH_T?, DAQ_SERVER_IO_HASH_T?, uint,
                Exception?) UploadOnlineIOConfiguration(DAQTargetProperty property)
        {
            try
            {
                var com = new DAQPort(new System.Net.IPEndPoint(property.SourceIPv4, 0), new System.Net.IPEndPoint(property.DestinationIPv4, property.DestinationPort), property.SendTimeoutValue, property.ReceiveTimeoutValue);
                var master = new DAQMaster(com, property.InternalReservedBufferSize);
                
                com.Connect();

                DAQ_SERVER_INFO_T info = new DAQ_SERVER_INFO_T();
                DAQ_SERVER_IO_HASH_T varHash = new DAQ_SERVER_IO_HASH_T(), ioHash = new DAQ_SERVER_IO_HASH_T();

                master.AcquisiteServerInfoEx(0xBBBB, ref info, ref varHash, ref ioHash);

                ReadOnlyMemory<byte> data = null;
                int received = 0;
                do
                    master.AcquisiteData(true, 0xAAAA, 1, out received, out _, out _, out data);
                while (received != 1);
                uint t0 = MemoryMarshal.Read<uint>(data.Span);
                do
                    master.AcquisiteData(true, 0xAAAA, 1, out received, out _, out _, out data);
                while (received != 1);
                uint t1 = MemoryMarshal.Read<uint>(data.Span);

                return (info, varHash, ioHash, t1 - t0, null);
            }
            catch(Exception ex)
            {
                return (null, null, null, 0, ex);
            }
        }

        public static string MD5String(byte[]? data)
        {
            if (data != null)
                return string.Concat((data.Select(x => x.ToString("X02"))));
            else
                return string.Empty;
        }

        public static string MD5String(ref DAQ_SERVER_IO_HASH_T? data)
        {
            StringBuilder sb = new StringBuilder();
            if (data.HasValue)
            {
                foreach (var i in data.Value)
                {
                    sb.Append(i.ToString("X02"));
                }
            }
            return sb.ToString();
        }
    }
}
