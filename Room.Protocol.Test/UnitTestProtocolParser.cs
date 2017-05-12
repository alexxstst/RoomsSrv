using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rooms.Protocol;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;

namespace Room.Protocol.Test
{
    [TestClass]
    public class UnitTestProtocolParser
    {
        private IProtocolParser CreateParser()
        {
            var pool = new StandartPool<byte[]>(() => new byte[2048]);
            var commandPool = new StandartPool<IRoomCommand>(() => new RoomCommand());

            return new SimpleStringProtocolParser(pool, commandPool);
        }

        [TestMethod]
        public void TestParseCommand1()
        {
            var parser = CreateParser();

            var testCommand = "Test";
            var testCommandBuff = Encoding.UTF8.GetBytes(testCommand);

            var resultLength = 0;
            var command = parser.FromBuffer(testCommandBuff, 0, testCommandBuff.Length, out resultLength);

            Assert.AreEqual(testCommandBuff.Length, resultLength);
            Assert.IsNull(command);
        }

        [TestMethod]
        public void TestParseCommand2()
        {
            var parser = CreateParser();

            var testCommand = "Test|XX^00\0";
            var testCommandBuff = Encoding.UTF8.GetBytes(testCommand);

            var resultLength = 0;
            var command = parser.FromBuffer(testCommandBuff, 0, testCommandBuff.Length, out resultLength);

            Assert.AreEqual(testCommandBuff.Length, resultLength);
            Assert.AreEqual("Test", command.Command);
            Assert.AreEqual("00", command.Data["XX"]);
        }


        [TestMethod]
        public void TestParseCommand3()
        {
            var parser = CreateParser();

            var testCommand = "Test|XX^\0Data|xxx^\0";
            var testCommandBuff = Encoding.UTF8.GetBytes(testCommand);

            var resultLength = 0;
            var command = parser.FromBuffer(testCommandBuff, 0, testCommandBuff.Length, out resultLength);

            Assert.AreEqual(testCommand.IndexOf("\0") + 1, resultLength);
            Assert.AreEqual("Test", command.Command);
            Assert.AreEqual("", command.Data["XX"]);

            command = parser.FromBuffer(testCommandBuff, resultLength, testCommandBuff.Length, out resultLength);

            Assert.AreEqual(testCommandBuff.Length, resultLength);
            Assert.AreEqual("Data", command.Command);
            Assert.AreEqual("", command.Data["xxx"]);

            command = parser.FromBuffer(testCommandBuff, resultLength, testCommandBuff.Length, out resultLength);
            Assert.AreEqual(testCommandBuff.Length, resultLength);
            Assert.IsNull(command);
        }
    }
}
