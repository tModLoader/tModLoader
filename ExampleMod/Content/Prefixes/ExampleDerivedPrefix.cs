namespace ExampleMod.Content.Prefixes
{
	// Be sure to see 'ExamplePrefix' first.
	// This class showcases how you can use inheritance to have an easier time with making variants of a single prefix.
	// With that said, remember that inheritance is just one of the thousands of tools that are available to programmers, and that with great power comes great responsibility.
	public class ExampleDerivedPrefix : ExamplePrefix // Deriving from ExamplePrefix!
	{
		// Overriding the Power property to make it return 2.0 instead of 1.0!
		// If you want to modify the base class' implementation's value, you can write 'base.Power' to reference it. 
		public override float Power => base.Power * 2f;

		// No need for anything else, since members get inherited from the base class.
	}
}
