using System;
using System.Net.Sockets;

namespace Rooms.Protocol.Sockets
{

    /// <summary>
    /// ������� ��������� ��� ������ �������������� ������
    /// </summary>
    public interface IBaseSocketClient: IDisposable
    {
        /// <summary>
        /// �������, ����������� ��� ����������� �������
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> Disconnect;

        /// <summary>
        /// ������� ����������� ����� ������ ���������
        /// </summary>
        event EventHandler<SocketReceiveEventArgs> AfterReceive;

        /// <summary>
        /// �������, ����������� ����� �������� ���������
        /// </summary>
        event EventHandler<SocketSendEventArgs> AfterSend;

        /// <summary>
        /// ��������, ������������ ������ �������
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// �����, ������������� ����� ������� � ������������ �������
        /// </summary>
        /// <param name="socket"></param>
        void Attach(Socket socket);

        /// <summary>
        /// �����, ������������ ����� � ����������� ���������� � ��� 
        /// </summary>
        void Detach();

        /// <summary>
        /// �����, ������������ ������ ������. ������ ������������ ����������, ������� ����� ����� ���������� ����������
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        void SendBytes(byte[] data, int length);
    }
}