using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Pipe.Tests
{
	[TestFixture ()]
	public class ProducerConsumerTests
	{
		[Test]
		public void CanStreamMoreDataThanFitsIntoInternalBuffer ()
		{
			System.Security.Cryptography.RNGCryptoServiceProvider randomNumbers = 
				new System.Security.Cryptography.RNGCryptoServiceProvider();
			var buffer = new byte[1 * 1024 * 1024];
			randomNumbers.GetBytes(buffer);
			var pipeStream = new PipeStream (buffer.Length / 10);
			var readBuffer = new byte[buffer.Length];
			var bytesRead = 0;
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
		public void CanReadWriteDataInRandomSizedChunks ()
		{
			System.Security.Cryptography.RNGCryptoServiceProvider randomNumbers = 
				new System.Security.Cryptography.RNGCryptoServiceProvider();
			var buffer = new byte[1 * 1024 * 1024];
			randomNumbers.GetBytes(buffer);
			var pipeStream = new PipeStream (buffer.Length / 10);
			var readBuffer = new byte[buffer.Length];
			var bytesRead = 0;
			var readTask = Task.Run (() => bytesRead = ReadRandomSizedChunks (pipeStream, readBuffer));
			Task.Delay (100);
			WriteRandomSizedChunks (pipeStream, buffer);
			pipeStream.Flush ();
			var taskFinished = readTask.Wait (TimeSpan.FromSeconds (10));
			Assert.IsTrue (taskFinished);
			Assert.AreEqual (buffer.Length, bytesRead);
			CollectionAssert.AreEquivalent (buffer, readBuffer);
		}

		private int ReadRandomSizedChunks (PipeStream pipeStream, byte[] buffer)
		{
			var random = new Random ();
			var totalBytesRead = 0;
			var offset = 0;
			while (totalBytesRead < buffer.Length) {
				var count = random.Next() % Math.Min (10, buffer.Length - totalBytesRead) + 1;
				var bytesRead = pipeStream.Read (buffer, offset, count);
				totalBytesRead += bytesRead;
				offset += bytesRead;
			}

			return totalBytesRead;
		}

		private int WriteRandomSizedChunks (PipeStream pipeStream, byte[] buffer)
		{
			var random = new Random ();
			var totalBytesWritten = 0;
			var offset = 0;
			while (totalBytesWritten < buffer.Length) {
				var count = random.Next() % Math.Min (10, buffer.Length - totalBytesWritten) + 1;
				pipeStream.Write (buffer, offset, count);
				totalBytesWritten += count;
				offset += count;
			}

			return totalBytesWritten;
		}
	}
}

