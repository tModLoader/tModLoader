using System;
using System.IO;

namespace Terraria.ModLoader.IO
{
	internal class EntryReadStream : Stream
	{
		private Stream _underlying;
		private int start;
		private int length;

		public readonly string name;

		public EntryReadStream(Stream stream, int offset, int count, string name = null)
		{
			_underlying = stream;
			start = offset;
			length = count;
			this.name = name;
			
			if (_underlying.Position != start)
				_underlying.Position = start;
		}

		public override bool CanRead => _underlying.CanRead;

		public override bool CanSeek => _underlying.CanSeek;

		public override bool CanWrite => false;

		public override long Length => length;

		public override long Position {
			get => _underlying.Position - start;
			set
			{
				if (value < 0 || value > length)
					throw new ArgumentOutOfRangeException($"Position {value} outside range (0-{length})");

				_underlying.Position = value + start;
			}
		}

		public override void Flush() => throw new NotImplementedException();

		public override int Read(byte[] buffer, int offset, int count)
		{
			count = Math.Min(count, (int)(length - Position));
			return _underlying.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Current) 
			{
				long target = Position + offset;
				if (target < 0 || target > length)
					throw new ArgumentOutOfRangeException($"Position {target} outside range (0-{length})");

				return _underlying.Seek(offset, origin) - start;
			}

			Position = origin == SeekOrigin.Begin ? offset : length - offset;
			return Position;
		}

		public override void SetLength(long value) => throw new NotImplementedException();

		public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

		public bool IsDisposed {get; private set;}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				IsDisposed = true;
		}
	}
}