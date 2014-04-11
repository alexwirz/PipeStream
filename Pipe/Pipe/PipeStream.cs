using System;
using System.IO;
using System.Collections.Generic;

namespace Pipe
{
	public class PipeStream : Stream
	{
		private readonly List<byte> _buffer = new List<byte> (); 

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
				buffer [bytesCount] = _buffer [0];
				_buffer.RemoveAt (0);
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
			_buffer.AddRange (buffer);
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
				throw new NotImplementedException ();
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

