using System;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;

namespace Rooms.Server.GameObjects
{

    /// <summary>
    /// ��������� �������
    /// </summary>
    public interface IRoomChannel: IPoolChecker
    {
        /// <summary>
        /// ������������� �������
        /// </summary>
        string RoomId { get; set; }

        /// <summary>
        /// �������� ���������� ���� �������� ����������� � �������
        /// </summary>
        IRemoteClient[] Clients { get; }

        /// <summary>
        /// ����� ��������� �������� ��������� ��������
        /// </summary>
        DateTime LastClientAccess { get; set; }

        /// <summary>
        /// ��������, ������������, ��� ������� ��� �� �������
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// ��������, ������������, ��� ������� ������
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// �����, ��������� ������� � �������
        /// </summary>
        /// <param name="client"></param>
        void Add(IRemoteClient client);

        /// <summary>
        /// �����, ������� ������� �� �������
        /// </summary>
        /// <param name="client"></param>
        void Remove(IRemoteClient client);

        /// <summary>
        /// �����, ���������� ��������� ���� ��������, ����� ���, ��� ������ �������
        /// </summary>
        /// <param name="command"></param>
        /// <param name="filter"></param>
        void SendAll(IRoomCommand command, Func<IRemoteClient, bool> filter);
    }
}