using System;

namespace Rooms.Protocol.Parser
{

    /// <summary>
    /// Базовый интерфейс для парсинга команд
    /// </summary>
    public interface IProtocolParser
    {
        /// <summary>
        /// Метод считывает команду из переданного массива и возвращает оставшееся количество байт.
        /// Если в буфере содержится частичная команда, то возвращается null
        /// </summary>
        /// <param name="buffer">Массив байт</param>
        /// <param name="startIndex">Стартовый индекс</param>
        /// <param name="length">Длина масива</param>
        /// <param name="finishIndex">Индекс, с которого начинается новая команда</param>
        /// <returns>Команда, или null, если в массиве не храниться ни одной целой команды</returns>
        IRoomCommand FromBuffer(byte[] buffer, int startIndex, int length, out int finishIndex);

        /// <summary>
        /// Метод возвращает кортеж, состоящий из ссылки на массив и количества записанных в массив байт.
        /// Возвращенный массив нужно будет передать обратно в IPool<bytes[]>, так как он берется из пула 
        /// </summary>
        /// <param name="command">Транслируемая команда</param>
        /// <returns>Массив бйат и количество записанных в него байт, начиная с индекса 0</returns>
        Tuple<byte[], int> ToBuffer(IRoomCommand command);
    }
}