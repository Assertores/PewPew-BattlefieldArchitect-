using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
	public class LibTest {
		[Test]
		public void LibTestSimplePasses() {

			{
				Assert.AreEqual(2, Lib.mod(5, 3));
				Assert.AreEqual(3, Lib.mod(3, 5));
				Assert.AreEqual(2, Lib.mod(-10, 3));

				Assert.AreEqual(0.25f, Lib.mod(3.25f, 1.5f));
				Assert.AreEqual(1.25f, Lib.mod(-3.25f, 1.5f));

				Assert.AreEqual(-1, Lib.mod(5,-3));
				Assert.AreEqual(-2, Lib.mod(-5, -3));

				Assert.AreEqual(-1.25f, Lib.mod(3.25f, -1.5f));
				Assert.AreEqual(-0.25f, Lib.mod(-3.25f, -1.5f));
			}
		}
	}
}
