using System;
using System.Text;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol.Parser
{
    public class SimpleStringProtocolParser : IProtocolParser
    {
        private const byte EndDelimiter = 0;
        private readonly IPool<byte[]> _bytesPool;
        private readonly IPool<IRoomCommand> _commandPool;
        private readonly IPool<StringBuilder> _stringPool;

        public SimpleStringProtocolParser(IPool<Byte[]> bytesPool, IPool<IRoomCommand> commandPool, IPool<StringBuilder> stringPool)
        {
            if (bytesPool == null)
                throw new ArgumentNullException(nameof(bytesPool));

            if (commandPool == null)
                throw new ArgumentNullException(nameof(commandPool));

            if (stringPool == null)
                throw new ArgumentNullException(nameof(stringPool));

            _bytesPool = bytesPool;
            _commandPool = commandPool;
            _stringPool = stringPool;
        }

        /// <summary>
        /// ����� ��������� ������� �� ����������� ������� � ���������� ���������� ���������� ����.
        /// ���� � ������ ���������� ��������� �������, �� ������������ null
        /// </summary>
        /// <param name="buffer">������ ����</param>
        /// <param name="startIndex">��������� ������</param>
        /// <param name="length">����� ������</param>
        /// <param name="finishIndex">������, � �������� ���������� ����� �������</param>
        /// <returns>�������, ��� null, ���� � ������� �� ��������� �� ����� ����� �������</returns>
        public IRoomCommand FromBuffer(byte[] buffer, int startIndex, int length, out int finishIndex)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            finishIndex = length;
            for (var i = startIndex; i < length; ++i)
            {
                //���� ����������� �������
                if (buffer[i] == EndDelimiter)
                {
                    finishIndex = i + 1; //������� ���� - ��� ������� ������ + 1 ����(����������� �������)

                    //���������� ������ ���� � ������ � �������� ��������� �� ������ � �������.
                    var values = Encoding.UTF8.GetString(buffer, startIndex, i - startIndex).Split('|');

                    //�������� ������� � ���������� � �� ���������
                    var command = _commandPool.Get();
                    command.Command = values[0];

                    //�������� ����������� ����� � ������� (������ ��� ������ 1 ����)
                    for(var j = 1; j < values.Length; ++j)
                    {
                        //���������� ������ ������
                        if (string.IsNullOrEmpty(values[j]))
                            continue;

                        //��������� ��������� ���� �� ���� - ���� + ��������
                        var commands = values[j].Split('^');
                        command.Data.Add(commands[0], commands[1]);
                    }

                    return command;
                }
            }

            return null;
        }

        /// <summary>
        /// ����� ���������� ������, ��������� �� ������ �� ������ � ���������� ���������� � ������ ����.
        /// ������������ ������ ����� ����� �������� ������� � IPool<bytes[]>, ��� ��� �� ������� �� ���� 
        /// </summary>
        /// <param name="command">������������� �������</param>
        /// <returns>������ ���� � ���������� ���������� � ���� ����, ������� � ������� 0</returns>
        public Tuple<byte[], int> ToBuffer(IRoomCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var sb = _stringPool.Get();

            //���������� ��������� �������
            sb.Append(command.Command);
            sb.Append("|");

            foreach (var pair in command.Data)
            {
                //���������� �������� �� �������
                sb.Append(pair.Key);
                sb.Append("^");
                sb.Append(pair.Value);
                sb.Append("|");
            }

            //��������� �����
            sb.Append('\0');

            //��������� ������� � ������
            var value = sb.ToString();

            //������ ������� ������
            sb.Length = 0;
            _stringPool.Free(sb);

            //���������� � ���������� ������ ������
            var buffer = _bytesPool.Get();
            var length = Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);

            return new Tuple<byte[], int>(buffer, length);
        }
    }
}