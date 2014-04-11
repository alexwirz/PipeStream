using NUnit.Framework;
using System;

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
	}
}

