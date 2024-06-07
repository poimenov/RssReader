using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;
using ReactiveUI;

namespace RssReader.MVVM.Tests.ViewModels
{
    [TestFixture]
    public class ContentViewModelTests
    {
        private ContentViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            viewModel = new ContentViewModel();
        }

        [Test]
        public void CopyLinkCommand_ShouldNotBeNull()
        {
            Assert.IsNotNull(viewModel.CopyLinkCommand);
        }

        [Test]
        public void CopyLinkCommand_ShouldExecuteWithoutError()
        {
            var canExecute = viewModel.CopyLinkCommand.CanExecute(null);
            Assert.IsTrue(canExecute);

            var executionSubject = new Subject<Unit>();
            viewModel.CopyLinkCommand.Execute(null).Subscribe(executionSubject);

            Assert.DoesNotThrow(() => executionSubject.OnNext(Unit.Default));
            Assert.DoesNotThrow(() => executionSubject.OnCompleted());
        }
    }
}
