using System;
using System.IO;

namespace Terraria.ModLoader.Core;

internal sealed class EntryReadStream : Stream
{
	private TmodFile file;
	private TmodFile.FileEntry entry;
	private Stream stream;
	private bool leaveOpen;

	private int Start => entry.Offset;

	public string Name => entry.Name;

	public EntryReadStream(TmodFile file, TmodFile.FileEntry entry, Stream stream, bool leaveOpen)
	{
		this.file = file;
		this.entry = entry;
		this.stream = stream;
		this.leaveOpen = leaveOpen;

		if (this.stream.Position != Start)
			this.stream.Position = Start;
	}

	public override bool CanRead => stream.CanRead;

	public override bool CanSeek => stream.CanSeek;

	public override bool CanWrite => false;

	public override long Length => entry.CompressedLength;

	public override long Position {
		get => stream.Position - Start;
		set {
			if (value < 0 || value > Length)
				throw new ArgumentOutOfRangeException($"Position {value} outside range (0-{Length})");

			stream.Position = value + Start;
		}
	}

	public override void Flush() => throw new NotImplementedException();

	public override int Read(byte[] buffer, int offset, int count)
	{
		count = Math.Min(count, (int)(Length - Position));
		return stream.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (origin == SeekOrigin.Current) {
			long target = Position + offset;
			if (target < 0 || target > Length)
				throw new ArgumentOutOfRangeException($"Position {target} outside range (0-{Length})");

			return stream.Seek(offset, origin) - Start;
		}

		Position = origin == SeekOrigin.Begin ? offset : Length - offset;
		return Position;
	}

	public override void SetLength(long value) => throw new NotImplementedException();

	public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

	public override void Close()
	{
		if (stream == null)
			return;

		if (!leaveOpen)
			stream.Close();

		stream = null;

		file.OnStreamClosed(this);
	}
}