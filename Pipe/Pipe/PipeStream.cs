using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace Pipe
{
	public class PipeStream : Stream
	{
		private readonly BlockingCollection<byte> _buffer = new BlockingCollection<byte> (); 

		public PipeStream ()
		{
		}

		#region implemented abstract members of Stream

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			var bytesRead = 0;
			for (var bytesCount = offset; bytesCount < offset + count; ++bytesCount) {
				buffer [bytesCount] = _buffer.Take ();
				++bytesRead;
			}

			return bytesRead;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotImplementedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			foreach (byte item in buffer) {
				_buffer.Add (item);
			}
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool CanWrite {
			get {
				return true;
			}
		}

		public override long Length {
			get {
				return _buffer.Count ();
			}
		}

		public override long Position {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}

