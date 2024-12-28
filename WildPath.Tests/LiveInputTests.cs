using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;
using WildPath.LiveInput;
using Xunit;

namespace WildPath.Tests
{
    public class LiveInputTests
    {
        [Fact]
        public async Task LiveInput_StartAsync_InvokesOnRun()
        {
            // Arrange
            var console = AnsiConsole.Console;
            var table = new Table();
            var liveInput = new LiveInput(console, table);
            var onRunInvoked = false;

            // Act
            await liveInput.StartAsync(ctx =>
            {
                onRunInvoked = true;
                return Task.CompletedTask;
            });

            // Assert
            Assert.True(onRunInvoked);
        }

        [Fact]
        public void LiveInputState_AddInput_AppendsCharacter()
        {
            // Arrange
            var state = new LiveInputState();

            // Act
            state.AddInput('a');

            // Assert
            Assert.Equal("a", state.Input);
        }

        [Fact]
        public void LiveInputState_PopInput_RemovesLastCharacter()
        {
            // Arrange
            var state = new LiveInputState();
            state.AddInput('a');
            state.AddInput('b');

            // Act
            state.PopInput();

            // Assert
            Assert.Equal("a", state.Input);
        }

        [Fact]
        public void LiveInputState_MoveLeft_MovesCursorLeft()
        {
            // Arrange
            var state = new LiveInputState();
            state.AddInput('a');
            state.AddInput('b');
            state.MoveLeft();

            // Act
            state.AddInput('c');

            // Assert
            Assert.Equal("acb", state.Input);
        }

        [Fact]
        public void LiveInputContext_Refresh_SetsRefreshRequested()
        {
            // Arrange
            var state = new LiveInputState();
            var layout = new Layout("Root");
            var context = new LiveInputContext(state, layout);

            // Act
            context.Refresh();

            // Assert
            Assert.True(context.RefreshRequested);
        }
    }
}
