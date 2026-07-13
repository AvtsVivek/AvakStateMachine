using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Sample.WpfViewChanged.StateManager.States
{
	public class Aa : StateBase
	{
		private IPageViewModel pageViewModel;
		public Aa(IPageViewModel pageViewModel)
		{
			if (pageViewModel == null)
			{
				throw new ArgumentNullException(nameof(pageViewModel));
			}

			this.pageViewModel = pageViewModel;
		}
		public override IPageViewModel GetStateViewModel()
		{
			return pageViewModel;
		}
	}

	public class Bb : StateBase
	{
		private IPageViewModel pageViewModel;
		public Bb(IPageViewModel pageViewModel)
		{
			if (pageViewModel == null)
			{
				throw new ArgumentNullException(nameof(pageViewModel));
			}

			this.pageViewModel = pageViewModel;
		}
		public override IPageViewModel GetStateViewModel()
		{
			return pageViewModel;
		}
	}

	public class Cc : StateBase
	{
		private IPageViewModel pageViewModel;
		public Cc(IPageViewModel pageViewModel)
		{
			if (pageViewModel == null)
			{
				throw new ArgumentNullException(nameof(pageViewModel));
			}

			this.pageViewModel = pageViewModel;
		}
		public override IPageViewModel GetStateViewModel()
		{
			return pageViewModel;
		}
	}
}
