using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Pipe
{
	public class PipeStream : Stream
	{
		private readonly BlockingCollection<byte> _buffer = new BlockingCollection<byte> (); 

		#region implemented abstract members of Stream

		public override void Flush ()
		{
			_buffer.CompleteAdding ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckArguments (buffer, offset, count);

			var bytesRead = 0;
			bool taken;

			for (var index = offset; index < offset + count; ++index) {
				taken = false;
				while (!taken) {
					taken = _buffer.TryTake (out buffer [index]);
					if (taken) {
						++bytesRead;
					}

					if (!taken && _buffer.IsCompleted) {
						return bytesRead;
					}
				}
			}

			return bytesRead;
		}

		private static void CheckArguments (byte[] buffer, int offset, int count)
		{
			if ((offset < 0) || (offset >= buffer.Length)) {
				throw new ArgumentOutOfRangeException ("offset");
			}
			if ((count < 0) || (count > buffer.Length)) {
				throw new ArgumentOutOfRangeException ("count");
			}
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
			CheckArguments (buffer, offset, count);

			for (var index = offset; index < (offset + count); ++index) {
				_buffer.Add (buffer [index]);
			}
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return false;
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

