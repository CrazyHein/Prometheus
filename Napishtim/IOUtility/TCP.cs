﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.IOUtility
{
    public class TCP : SocketInterface, IDisposable
    {
        private bool disposedValue;

        private Socket __tcp;
        private IPEndPoint __destination_endpoint;

        public TCP(IPEndPoint source, IPEndPoint destination, int sendTimeout, int receiveTimeout)
        {
            try
            {
                __tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (source != null)
                    __tcp.Bind(source);
                __destination_endpoint = destination;
                __tcp.SendTimeout = sendTimeout;
                __tcp.ReceiveTimeout = receiveTimeout;
                __tcp.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            }
            catch (System.Exception e)
            {
                throw new NaposhtimIOException(NaposhtimExceptionCode.RUNTIME_EXCEPTION, "Socket API threw an exception.", e);
            }
        }

        public TCP(Socket sc, IPEndPoint destination)
        {
            __tcp = sc;
            __destination_endpoint = destination;
        }

        public void Connect()
        {
            try
            {
                __tcp.Connect(__destination_endpoint);
            }
            catch (System.Exception e)
            {
                throw new NaposhtimIOException(NaposhtimExceptionCode.RUNTIME_EXCEPTION, "Socket API threw an exception.", e);
            }
        }

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags = SocketFlags.None)
        {
            int length = 0;
            try
            {
                while (length != size)
                {
                    length += __tcp.Receive(buffer, offset + length, size - length, socketFlags);
                    if (length == 0)
                        throw new NaposhtimIOException(NaposhtimExceptionCode.REMOTE_SERVER_DISCONNECTED);
                }
            }
            catch(NaposhtimException)
            {
                throw;
            }
            catch (System.Exception e)
            {
                throw new NaposhtimIOException(NaposhtimExceptionCode.RUNTIME_EXCEPTION, "Socket API threw an exception.", e);
            }
            return size;
        }

        public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags = SocketFlags.None)
        {
            try
            {
                return __tcp.Send(buffer, socketFlags);
            }
            catch (System.Exception e)
            {
                throw new NaposhtimIOException(NaposhtimExceptionCode.RUNTIME_EXCEPTION, "Socket API threw an exception.", e);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    __tcp.Close();
                    __tcp.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~TCP()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public string Name()
        {
            return "TCP";
        }
    }
}
