using System;
using System.Data.SqlClient;
using System.Text;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol.Parser
{
    public class SimpleStringProtocolParser : IProtocolParser
    {
        private const byte EndDelimiter = 0;
        private readonly IPool<byte[]> _bytesPool;
        private readonly IPool<IRoomCommand> _commandPool;

        public SimpleStringProtocolParser(IPool<Byte[]> bytesPool, IPool<IRoomCommand> commandPool)
        {
            _bytesPool = bytesPool;
            _commandPool = commandPool;
        }

        public IRoomCommand FromBuffer(byte[] buffer, int startIndex, int length, out int resultLength)
        {
            resultLength = length;
            for (var i = startIndex; i < length; ++i)
            {
                if (buffer[i] == EndDelimiter)
                {
                    resultLength = i + 1;
                    var values = Encoding.UTF8.GetString(buffer, startIndex, i - startIndex).Split('|');
                    var command = _commandPool.Get();
                    command.Command = values[0];

                    for(var j = 1; j < values.Length; ++j)
                    {
                        var commands = values[j].Split('^');
                        var key = commands[0];
                        var value = commands[1];
                        command.Data.Add(key, value);


                    }

                    return command;
                }
            }

            return null;
        }

        public Tuple<byte[], int> ToBuffer(IRoomCommand command)
        {
            var buffer = _bytesPool.Get();
            if (buffer == null)
                throw new InvalidOperationException("Buffer is empty!");

            var sb = new StringBuilder(2048);

            sb.Append(command.Command);
            sb.Append("|");

            foreach (var pair in command.Data)
            {
                sb.Append(pair.Key);
                sb.Append("^");
                sb.Append(pair.Value);
            }

            sb.Append('\0');

            var value = sb.ToString();
            var length = Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);

            return new Tuple<byte[], int>(buffer, length);
        }
    }
}