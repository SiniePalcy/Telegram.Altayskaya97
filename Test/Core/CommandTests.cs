using Telegram.SafeBot.Core.Model;
using Xunit;

namespace Telegram.SafeBot.Test.Core
{
    public class CommandTests
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
            Assert.Equal(Commands.Unknown.Name, command.Name);
            Assert.True(command.IsValid);
        }

        [Fact]
        public void PostCommandTest()
        {
            var commandText = "/post 123 testText";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Post.Name, command.Name);
            Assert.True(command.IsValid);

            command.Text = "/post 123";
            Assert.True(command.IsValid);
        }

        [Fact]
        public void BanCommandTest()
        {
            var commandText = "/ban";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Ban.Name, command.Name);
            Assert.False(command.IsValid);

            command.Text = "/ban 123";
            Assert.True(command.IsValid);
        }
        
        [Fact]
        public void IwalkCommandTest()
        {
            var commandText = "/iwalk";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.IWalk.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/Iwalk";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.IWalk.Name, command.Name);
            Assert.True(command.IsValid);
        }

        [Fact]
        public void SecretCommandTest()
        {
            var commandText = "/unknown";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Unknown.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/admin";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Unknown.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/return";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.Unknown.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "simple text";
            command = Commands.GetCommand(commandText);
            Assert.Null(command);
        }

        [Fact]
        public void DeleteChatCommandTest()
        {
            var commandText = "/deletechat";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.DeleteChat.Name, command.Name);
            Assert.False(command.IsValid);

            commandText = "/deletechat newchat";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.DeleteChat.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/deletechat newchat sometext";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.DeleteChat.Name, command.Name);         
            Assert.True(command.IsValid);
        }

        [Fact]
        public void DeleteUserCommandTest()
        {
            var commandText = "/deleteuser";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.DeleteUser.Name, command.Name);
            Assert.False(command.IsValid);

            commandText = "/deleteuser user1";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.DeleteUser.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/deleteuser user1 sometext";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.DeleteUser.Name, command.Name);
            Assert.True(command.IsValid);
        }

        [Fact]
        public void ChangeUserTypeTest()
        {
            var commandText = "/changeusertype";
            var command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.ChangeUserType.Name, command.Name);
            Assert.False(command.IsValid);

            commandText = "/changeusertype admin";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.ChangeUserType.Name, command.Name);
            Assert.False(command.IsValid);

            commandText = "/changeusertype admin admin";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.ChangeUserType.Name, command.Name);
            Assert.True(command.IsValid);

            commandText = "/changeusertype admin admin admin";
            command = Commands.GetCommand(commandText);
            Assert.Equal(Commands.ChangeUserType.Name, command.Name);
            Assert.True(command.IsValid);
        }
    }
}
