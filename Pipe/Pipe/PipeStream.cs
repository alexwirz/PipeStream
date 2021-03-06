﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Pipe
{
	public class PipeStream : Stream
	{
		private readonly BlockingCollection<byte> _buffer;

		public PipeStream () : this (1 * 1024 * 1024)
		{
		}

		public PipeStream (int bufferSize)
		{
			_buffer = new BlockingCollection<byte> (bufferSize);
		}

		#region implemented abstract members of Stream

		public override void Close ()
		{
			Flush ();
			base.Close ();
		}

		public override void Flush ()
		{
			_buffer.CompleteAdding ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckArguments (buffer, offset, count);

			var bytesRead = 0;
			var haveMoreBytes = true;
			for (var index = offset; (index < offset + count) && haveMoreBytes; ++index) {
				haveMoreBytes = TryReadNextByteIntoBuffer (buffer, index);
				if (haveMoreBytes) ++bytesRead;
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

		private bool TryReadNextByteIntoBuffer (byte[] buffer, int index)
		{
			var readSucceeded = false;
			while (!readSucceeded) {
				readSucceeded = _buffer.TryTake (out buffer [index]);
				if (!readSucceeded && _buffer.IsAddingCompleted) {
					break;
				}
			}

			return readSucceeded;
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

