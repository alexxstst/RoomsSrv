using System.Collections.Generic;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol
{

    /// <summary>
    /// Общий интерфейс команды
    /// </summary>
    public interface IRoomCommand : IPoolChecker
    {
        /// <summary>
        /// Заголовок команды
        /// </summary>
        string Command { get; set; }

        /// <summary>
        /// Массив с данными команды
        /// </summary>
        Dictionary<string,string> Data { get; }
    }
}