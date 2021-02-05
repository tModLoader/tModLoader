using System;
using System.Linq.Expressions;
using Terraria.Localization;

namespace Terraria.ModLoader.Shops
{
	public abstract partial class Condition
	{
		private readonly NetworkText DescriptionText;
		public string Description => DescriptionText.ToString();

		public Condition(NetworkText description) {
			DescriptionText = description ?? throw new ArgumentNullException(nameof(description));
		}

		public abstract bool Evaluate();

		public virtual void SetCustomData(object obj) {
		}

		public static Condition operator !(Condition condition) {
			Expression<Func<bool>> exp = () => condition.Evaluate();
			var neg = Expression.Lambda<Func<bool>>(Expression.Not(exp.Body));

			// todo: localization
			return new SimpleCondition(NetworkText.FromLiteral("Not " + condition.DescriptionText), neg.Compile());
		}

		public static Condition operator |(Condition condition1, Condition condition2) {
			Expression<Func<bool>> exp1 = () => condition1.Evaluate();
			Expression<Func<bool>> exp2 = () => condition2.Evaluate();

			var neg = Expression.Lambda<Func<bool>>(Expression.Or(exp1.Body, exp2.Body));

			// todo: localization
			return new SimpleCondition(NetworkText.FromLiteral(condition1.DescriptionText + " or " + condition2.DescriptionText), neg.Compile());
		}

		public static Condition operator &(Condition condition1, Condition condition2) {
			Expression<Func<bool>> exp1 = () => condition1.Evaluate();
			Expression<Func<bool>> exp2 = () => condition2.Evaluate();

			var neg = Expression.Lambda<Func<bool>>(Expression.And(exp1.Body, exp2.Body));

			// todo: localization
			return new SimpleCondition(NetworkText.FromLiteral(condition1.DescriptionText + " and " + condition2.DescriptionText), neg.Compile());
		}
	}

	public class SimpleCondition : Condition
	{
		private readonly Func<bool> Predicate;

		public SimpleCondition(NetworkText description, Func<bool> predicate) : base(description) {
			Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public override bool Evaluate() => Predicate();
	}
}