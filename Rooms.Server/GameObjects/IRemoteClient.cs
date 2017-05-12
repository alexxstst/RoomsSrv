using Rooms.Protocol;

namespace Rooms.Server.GameObjects
{

    /// <summary>
    /// ����������� ��������� ������� ��� �������� �������������� �������
    /// </summary>
    public interface IRemoteClient : IRoomClient
    {
        /// <summary>
        /// �����, ������������ ������� � �������
        /// </summary>
        /// <param name="roomChannel"></param>
        void AttachToRoom(IRoomChannel roomChannel);

        /// <summary>
        /// �������, � ������� ���������� ������
        /// </summary>
        IRoomChannel RoomChannel { get; }
    }
}