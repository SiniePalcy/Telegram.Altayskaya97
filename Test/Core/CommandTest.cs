using Telegram.Altayskaya97.Core.Constant;
using Xunit;

namespace Telegram.Altayskaya97.Test
{
    public class CommandTest
    {
        [Fact]
        public void StartCommandTest()
        {
            string commandText = "/start";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Start.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/start 123";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Start.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/sturt";
            command = Commands.GetCommand(commandText);
            Assert.Null(command);

            commandText = "/post 123 testText";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Post.Name, command.Name);
            Assert.True(command.IsValid);
            command.Text = "/post 123";
            Assert.False(command.IsValid);

            commandText = "/ban";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Ban.Name, command.Name);
            Assert.False(command.IsValid);
            command.Text = "/ban 123";
            Assert.True(command.IsValid);
        }
    }
}
