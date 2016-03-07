using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Exceptions
{
    internal class ModSortingException : Exception
    {
        public ICollection<ModLoader.LoadingMod> errored;

        public ModSortingException(ICollection<ModLoader.LoadingMod> errored, string message) : base(message) {
            this.errored = errored;
        }
    }
}