namespace Rooms.Server.GameObjects
{

    /// <summary>
    /// ��������� ��������� ������
    /// </summary>
    public interface IRoomManager
    {
        /// <summary>
        /// ������ ���� ��������� ������
        /// </summary>
        IRoomChannel[] Rooms { get; }

        /// <summary>
        /// �����, ���������� ������� �� � ��������������
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        IRoomChannel GetRoom(string roomId);

        /// <summary>
        /// �����, ������� ������� �� � ��������������
        /// </summary>
        /// <param name="roomChannel"></param>
        /// <param name="reason"></param>
        void FreeRoom(IRoomChannel roomChannel, RoomRemoveReason reason);

        /// <summary>
        /// �����, ������������ ������� � ������ ������ ��������� ������
        /// </summary>
        /// <param name="remotClient"></param>
        void AttachClient(IRemoteClient remotClient);

        /// <summary>
        /// ��������, ������������ ������, ���������� ����������
        /// </summary>
        IRoomManagerStatistics Statistics { get; }
    }

    public enum RoomRemoveReason
    {
        Timeout,
        DisconnectAllClients,
        ServerStop
    }
}