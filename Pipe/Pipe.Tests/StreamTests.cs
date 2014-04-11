using NUnit.Framework;
using System;
using System.Linq;

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
	}
}

