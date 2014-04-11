using NUnit.Framework;
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
			var readBuffer = new byte [buffer.Length * 2];
			var bytesRead = 0;
			var pipeStream = new PipeStream ();
			var readTask = Task.Run (() => bytesRead = pipeStream.Read (readBuffer, 0, readBuffer.Length));
			Task.Delay (100);
			pipeStream.Write (buffer, 0, buffer.Length);
			pipeStream.Flush ();
			var taskFinished = readTask.Wait (TimeSpan.FromSeconds (1));
			Assert.IsTrue (taskFinished);
			Assert.AreEqual (buffer.Length, bytesRead);
			var readData = new byte [bytesRead];
			Array.Copy (readBuffer, readData, bytesRead);
			CollectionAssert.AreEquivalent (readData, buffer);
		}

		[Test]
		public void NoSeekAllowed ()
		{
			var pipeStream = new PipeStream ();
			Assert.IsFalse (pipeStream.CanSeek);
		}
	}
}

