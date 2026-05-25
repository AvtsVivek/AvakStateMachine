namespace Avak.StateMachine.Core
{
	public enum TriggerSource
	{
		//MenuKey,
		//SoftKey,
		//HardKey,
		Event
	}

	public class Trigger : IEquatable<Trigger>
	{
		public string Name { get; init; }
		public TriggerSource Source { get; init; }

		public Trigger(string name, TriggerSource source)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			Source = source;
			Name = name;
		}

		public bool Equals(Trigger? other)
		{
			if (other == null)
			{
				return false;
			}

			if (!ReferenceEquals(this, other))
			{
				return false;
			}

			if (this.Source != other.Source)
			{
				return false;
			}

			if (this.Name != other.Name)
			{
				return false;
			}

			return true;
		}
	}
}
