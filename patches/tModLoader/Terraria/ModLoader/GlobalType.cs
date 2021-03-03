namespace Terraria.ModLoader
{
	public abstract class GlobalType<TEntity> : ModType
	{
		internal ushort index;

		/// <summary>
		/// Whether to create a new instance of this Global for every entity that exists. 
		/// Useful for storing information on an entity. Defaults to false. 
		/// Return true if you need to store information (have non-static fields).
		/// </summary>
		public virtual bool InstancePerEntity => false;

		/// <summary>
		/// Use this to control whether or not this global should be associated with the provided entity instance.
		/// </summary>
		/// <param name="entity"> The entity for which the global instantion is being checked. </param>
		/// <param name="lateInstantiation">
		/// Whether this check occurs before or after the ModX.SetDefaults call.
		/// <br/> If you're relying on entity values that can be changed by that call, you should likely prefix your return value with the following:
		/// <code> lateInstantiation &amp;&amp; ... </code>
		/// </param>
		public virtual bool InstanceForEntity(TEntity entity, bool lateInstantiation) => true;
	}
}
