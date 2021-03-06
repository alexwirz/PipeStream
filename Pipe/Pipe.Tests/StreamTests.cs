﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipe.Tests
{
	[TestFixture ()]
	public class StreamTests
	{
		[Test ()]
		public void CanWriteToPipeStream ()
		{
			var pipeStream = new PipeStream ();
			Assert.IsTrue (pipeStream.CanWrite);
		}

		[Test]
		public void CanReadFromPipeStream ()
		{
			var pipeStream = new PipeStream ();
			Assert.IsTrue (pipeStream.CanRead);
		}

		[Test]
		public void CanReadBytesPerviouslyWrittenToStream ()
		{
			var buffer = new byte [] { 1, 1, 2, 3, 5, 8, 13, 21 };
			var pipeStream = new PipeStream ();
			pipeStream.Write (buffer, 0, buffer.Length);
			var readBuffer = new byte[buffer.Length];
			pipeStream.Read (readBuffer, 0, readBuffer.Length);
			CollectionAssert.AreEqual (buffer, readBuffer);
		}

		[Test]
		public void CanReadBytesPreviouslyWrittenInTwoChunks ()
		{
			var firstChunk = new byte [] { 1, 1, 2, 3 };
			var secondChunk = new byte [] { 5, 8, 13, 21 };
			var expectedResult = new byte [firstChunk.Length + secondChunk.Length];
			Array.Copy (firstChunk, expectedResult, firstChunk.Length);
			Array.Copy (secondChunk, 0, expectedResult, firstChunk.Length, secondChunk.Length);
			var pipeStream = new PipeStream ();
			pipeStream.Write (firstChunk, 0, firstChunk.Length);
			pipeStream.Write (secondChunk, 0, secondChunk.Length);
			var readBuffer = new byte[firstChunk.Length + secondChunk.Length];
			pipeStream.Read (readBuffer, 0, readBuffer.Length);
			CollectionAssert.AreEqual (expectedResult, readBuffer);
		}

		[Test]
		public void LengthReturnsZeroForEmptyStream ()
		{
			var pipeSteam = new PipeStream ();
			Assert.AreEqual (0, pipeSteam.Length);
		}

		[Test]
		public void LengthReturnsTheNumberOfBytesCanBeReadFromTheStream ()
		{
			var buffer = new byte [] { 1 };
			var pipeStream = new PipeStream ();
			pipeStream.Write (buffer, 0, buffer.Length);
			Assert.AreEqual (buffer.Length, pipeStream.Length);
		}

		[Test]
		public void ReadBlocksUntilEnoughBytesWrittenToStream ()
		{
			var buffer = new byte [] { 1, 1, 2, 3, 5, 8, 13, 21 };
			var readBuffer = new byte [buffer.Length];
			var pipeStream = new PipeStream ();
			var readTask = Task.Run (() => pipeStream.Read (readBuffer, 0, buffer.Length));
			Task.Delay (100);
			pipeStream.Write (buffer, 0, buffer.Length);
			var taskFinished = readTask.Wait (TimeSpan.FromSeconds (1));
			Assert.IsTrue (taskFinished);
			CollectionAssert.AreEquivalent (buffer, readBuffer);
		}

		[Test]
		public void FlushUnblocksRead ()
		{
			var buffer = new byte [] { 1, 2, 3 };
			var readBuffer = new byte [buffer.Length];
			var bytesRead = 0;
			var pipeStream = new PipeStream ();
			var readTask = Task.Run (() => bytesRead = pipeStream.Read (readBuffer, 0, readBuffer.Length));
			Task.Delay (100);
			pipeStream.Write (buffer, 0, buffer.Length);
			pipeStream.Flush ();
			var taskFinished = readTask.Wait (TimeSpan.FromSeconds (1));
			Assert.IsTrue (taskFinished);
			Assert.AreEqual (buffer.Length, bytesRead);
			CollectionAssert.AreEquivalent (readBuffer, buffer);
		}

		[Test]
		public void NoSeekAllowed ()
		{
			var pipeStream = new PipeStream ();
			Assert.IsFalse (pipeStream.CanSeek);
		}

		[Test]
		public void WriteTakesBytesFromTheGivenOffset ()
		{
			var buffer = new byte [] { 1, 2 };
			var pipeStream = new PipeStream ();
			var offset = buffer.Length / 2;
			pipeStream.Write (buffer, offset, buffer.Length - offset);
			Assert.AreEqual (buffer.Length - offset, pipeStream.Length);
		}

		[Test]
		public void CanReadToEndAfterFlush ()
		{
			var buffer = new byte [] { 1, 1, 2, 3, 5, 8, 13, 21 };
			var pipeStream = new PipeStream ();
			pipeStream.Write (buffer, 0, buffer.Length);
			pipeStream.Flush ();
			var readBuffer = new byte[buffer.Length];
			var bytesRead = pipeStream.Read (readBuffer, 0, readBuffer.Length);
			Assert.AreEqual (buffer.Length, bytesRead);
			CollectionAssert.AreEqual (buffer, readBuffer);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReadThrowsArgumentOfRangeIfOffsetIsNegative ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Read (new byte[10], -1, 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReadThrowsArgumentOfRangeIfOffsetGreaterThanBufferSize ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Read (new byte[10], 10, 1);
		}

		[Test]
		public void NoBytesTakenFromStreamIfOffsetOutOfRange ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[] { 1, 2, 3 }, 0, 3);
			try {
				pipeStream.Read (new byte[10], 10, 1);
			}
			catch (ArgumentOutOfRangeException) {
			}

			Assert.AreEqual (3, pipeStream.Length);
		}

		[Test]
		public void NoBytesTakenFromStreamIfCountIsZero ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[] { 1, 2, 3 }, 0, 3);
			pipeStream.Read (new byte [1], 0, 0);
			Assert.AreEqual (3, pipeStream.Length);
		}

		[Test]
		public void NoBytesWrittenToStreamIfCountIsZero ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[] { 1, 2, 3 }, 0, 0);
			Assert.AreEqual (0, pipeStream.Length);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void WriteThrowsArgumentOfRangeIfOffsetIsNegative ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[10], -1, 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void WriteThrowsArgumentOfRangeIfOffsetGreaterThanBufferSize ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[10], 10, 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReadThrowsArgumentOfRangeIfCountIsNegative ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Read (new byte[10], 0, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReadThrowsArgumentOfRangeIfCountGreaterThanBufferSize ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Flush (); // to prevent endless waiting when the test fails
			pipeStream.Read (new byte[10], 0, 11);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void WriteThrowsArgumentOfRangeIfCountIsNegative ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[2] {1, 2}, 0, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void WriteThrowsArgumentOfRangeIfCountGreaterThanBufferSize ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[2] {1, 2}, 0, 3);
		}


		[Test]
		public void ReadReturnsNumberOfBytesReadAfterFlush ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Write (new byte[2] {1, 2}, 0, 2);
			pipeStream.Flush ();
			var bytesRead = pipeStream.Read (new byte [3], 0, 3);
			Assert.AreEqual (2, bytesRead);
		}

		[Test]
		public void CanReadStreamToEndAfterItWasClosed ()
		{
			var pipeStream = new PipeStream ();
			var buffer = new byte[2] { 1, 2 };
			pipeStream.Write (buffer, 0, buffer.Length);
			pipeStream.Close ();
			var readBuffer = new byte [buffer.Length];
			pipeStream.Read (readBuffer, 0, readBuffer.Length);
			CollectionAssert.AreEquivalent (buffer, readBuffer);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CannotWriteToClosedStream ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Close ();
			pipeStream.Write (new byte[] { 1, 2 }, 0, 2);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CannotWriteToDisposedStream ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Dispose ();
			pipeStream.Write (new byte[] { 1, 2 }, 0, 2);
		}

		[Test]
		public void CanFlushStreamMultipleTimes ()
		{
			var pipeStream = new PipeStream ();
			pipeStream.Flush ();
			pipeStream.Flush ();
		}
	}
}

