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
        /// ћетод считывает команду из переданного массива и возвращает оставшеес€ количество байт.
        /// ≈сли в буфере содержитс€ частична€ команда, то возвращаетс€ null
        /// </summary>
        /// <param name="buffer">ћассив байт</param>
        /// <param name="startIndex">—тартовый индекс</param>
        /// <param name="length">ƒлина масива</param>
        /// <param name="finishIndex">»ндекс, с которого начинаетс€ нова€ команда</param>
        /// <returns> оманда, или null, если в массиве не хранитьс€ ни одной целой команды</returns>
        public IRoomCommand FromBuffer(byte[] buffer, int startIndex, int length, out int finishIndex)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            finishIndex = length;
            for (var i = startIndex; i < length; ++i)
            {
                //ищем разделитель команды
                if (buffer[i] == EndDelimiter)
                {
                    finishIndex = i + 1; //остаток байт - это текущий индекс + 1 байт(разделитель команды)

                    //превращаем массив байт в строку и отдел€ем заголовок от хвоста с данными.
                    var values = Encoding.UTF8.GetString(buffer, startIndex, i - startIndex).Split('|');

                    //получаем команду и записываем в неЄ заголовок
                    var command = _commandPool.Get();
                    command.Command = values[0];

                    //начинаем сканировать блоки с данными (обычно это только 1 блок)
                    for(var j = 1; j < values.Length; ++j)
                    {
                        //пропускаем пустые секции
                        if (string.IsNullOrEmpty(values[j]))
                            continue;

                        //–аздел€ем текстовый блок на пары - ключ + значение
                        var commands = values[j].Split('^');
                        command.Data.Add(commands[0], commands[1]);
                    }

                    return command;
                }
            }

            return null;
        }

        /// <summary>
        /// ћетод возвращает кортеж, состо€щий из ссылки на массив и количества записанных в массив байт.
        /// ¬озвращенный массив нужно будет передать обратно в IPool<bytes[]>, так как он беретс€ из пула 
        /// </summary>
        /// <param name="command">“ранслируема€ команда</param>
        /// <returns>ћассив бйат и количество записанных в него байт, начина€ с индекса 0</returns>
        public Tuple<byte[], int> ToBuffer(IRoomCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var sb = _stringPool.Get();

            //записываем заголовок команды
            sb.Append(command.Command);
            sb.Append("|");

            foreach (var pair in command.Data)
            {
                //записываем значени€ из команды
                sb.Append(pair.Key);
                sb.Append("^");
                sb.Append(pair.Value);
                sb.Append("|");
            }

            //добавл€ем хвост
            sb.Append('\0');

            //сохран€ем команду в строке
            var value = sb.ToString();

            //отдаем обратно билдер
            sb.Length = 0;
            _stringPool.Free(sb);

            //записываем в выделенный буффер данные
            var buffer = _bytesPool.Get();
            var length = Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);

            return new Tuple<byte[], int>(buffer, length);
        }
    }
}