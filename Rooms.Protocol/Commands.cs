﻿namespace Rooms.Protocol
{

    /// <summary>
    /// Примитивные команды обмена
    /// </summary>
    public class Commands
    {
        public const string EnterToRoom = "EnterToRoom";
        public const string SetClientId = "SetClientId";
        public const string Exit = "Exit";
        public const string PushMessage = "PushMessage";
    }
}