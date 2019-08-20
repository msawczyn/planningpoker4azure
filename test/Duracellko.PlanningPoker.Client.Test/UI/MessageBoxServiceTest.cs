using System;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.UI
{
    [TestClass]
    public class MessageBoxServiceTest
    {
        [TestMethod]
        public void ShowMessage_NoHandler_ReturnsCompletedTask()
        {
            MessageBoxService target = CreateMessageBoxService();

            Task result = target.ShowMessage("My message", "Test");

            Assert.IsTrue(result.IsCompletedSuccessfully);
        }

        [TestMethod]
        public async Task ShowMessage_Handler_HandlerIsExecuted()
        {
            MessageHandler handler = new MessageHandler();
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            await target.ShowMessage("My message", "Test");

            Assert.AreEqual(1, handler.Counter);
            Assert.AreEqual("My message", handler.Message);
            Assert.AreEqual("Test", handler.Title);
            Assert.IsNull(handler.PrimaryButton);
        }

        [TestMethod]
        public async Task ShowMessage_MessageIsNull_HandlerIsExecuted()
        {
            MessageHandler handler = new MessageHandler();
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            await target.ShowMessage(null, null);

            Assert.AreEqual(1, handler.Counter);
            Assert.IsNull(handler.Message);
            Assert.IsNull(handler.Title);
            Assert.IsNull(handler.PrimaryButton);
        }

        [TestMethod]
        public async Task ShowMessage_MessageIsEmpty_HandlerIsExecuted()
        {
            MessageHandler handler = new MessageHandler();
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            await target.ShowMessage(string.Empty, string.Empty);

            Assert.AreEqual(1, handler.Counter);
            Assert.AreEqual(string.Empty, handler.Message);
            Assert.AreEqual(string.Empty, handler.Title);
            Assert.IsNull(handler.PrimaryButton);
        }

        [TestMethod]
        public async Task ShowMessage_HandlerIsNotCompleted_ReturnsNotCompletedTask()
        {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            MessageHandler handler = new MessageHandler() { ResultTask = task.Task };
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            Task result = target.ShowMessage("My message", "Test");

            Assert.IsFalse(result.IsCompleted);

            task.SetResult(true);

            Assert.IsTrue(result.IsCompleted);
            await result;
        }

        [TestMethod]
        public void ShowMessage_PrimaryButtonAndNoHandler_ReturnsCompletedTask()
        {
            MessageBoxService target = CreateMessageBoxService();

            Task<bool> result = target.ShowMessage("My message", "Test", "Click me");

            Assert.IsTrue(result.IsCompletedSuccessfully);
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonAndHandler_HandlerIsExecuted()
        {
            MessageHandler handler = new MessageHandler();
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            await target.ShowMessage("My message", "Test", "Click me");

            Assert.AreEqual(1, handler.Counter);
            Assert.AreEqual("My message", handler.Message);
            Assert.AreEqual("Test", handler.Title);
            Assert.AreEqual("Click me", handler.PrimaryButton);
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonIsNull_HandlerIsExecuted()
        {
            MessageHandler handler = new MessageHandler();
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            await target.ShowMessage("My message", "Test", null);

            Assert.AreEqual(1, handler.Counter);
            Assert.AreEqual("My message", handler.Message);
            Assert.AreEqual("Test", handler.Title);
            Assert.IsNull(handler.PrimaryButton);
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonAndMessageIsNull_HandlerIsExecuted()
        {
            MessageHandler handler = new MessageHandler();
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            await target.ShowMessage(null, null, null);

            Assert.AreEqual(1, handler.Counter);
            Assert.IsNull(handler.Message);
            Assert.IsNull(handler.Title);
            Assert.IsNull(handler.PrimaryButton);
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonAndHandlerReturnsTrue_ReturnsTrue()
        {
            MessageHandler handler = new MessageHandler();
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            bool result = await target.ShowMessage("My message", "Test", "Click me");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonAndHandlerReturnsFalse_ReturnsFalse()
        {
            MessageHandler handler = new MessageHandler() { Result = false };
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            bool result = await target.ShowMessage("My message", "Test", "Click me");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonIsNullAndHandlerReturnsFalse_ReturnsFalse()
        {
            MessageHandler handler = new MessageHandler() { Result = false };
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            bool result = await target.ShowMessage("My message", "Test", null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonAndHandlerIsNotCompleted_ReturnsNotCompletedTask()
        {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            MessageHandler handler = new MessageHandler() { ResultTask = task.Task };
            MessageBoxService target = CreateMessageBoxService(messageHandler: handler);

            Task<bool> result = target.ShowMessage("My message", "Test", "Click me");

            Assert.IsFalse(result.IsCompleted);

            task.SetResult(true);

            Assert.IsTrue(result.IsCompleted);
            await result;
        }

        private static MessageBoxService CreateMessageBoxService(MessageHandler messageHandler = null)
        {
            MessageBoxService result = new MessageBoxService();
            result.SetMessageHandler(messageHandler != null ? messageHandler.HandleMessage : default(Func<string, string, string, Task<bool>>));
            return result;
        }

        private class MessageHandler
        {
            public bool Result { get; set; } = true;

            public Task<bool> ResultTask { get; set; }

            public int Counter { get; private set; }

            public string Message { get; private set; }

            public string Title { get; private set; }

            public string PrimaryButton { get; private set; }

            public Task<bool> HandleMessage(string message, string title, string primaryButton)
            {
                Counter++;
                Message = message;
                Title = title;
                PrimaryButton = primaryButton;
                return ResultTask ?? Task.FromResult(Result);
            }
        }
    }
}
