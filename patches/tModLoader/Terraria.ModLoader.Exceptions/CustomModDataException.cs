using System;
using System.IO;

namespace Terraria.ModLoader.Exceptions
{
    public class CustomModDataException : IOException
    {
        public CustomModDataException(string message, Exception inner) : base(message, inner) { }
    }
}
