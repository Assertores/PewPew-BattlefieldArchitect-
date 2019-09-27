using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
	public class RingBufferTest {
		// A Test behaves as an ordinary method
		[Test]
		public void RingBufferTestSimplePasses() {
			// Use the Assert class to test conditions

			RingBuffer<int> element = new RingBuffer<int>();

			element[0] = 0;
			element[2] = 2;
			{
				Assert.Catch(delegate () { int x = element[-1]; });
				Assert.DoesNotThrow(delegate () { int x = element[0]; });
				Assert.AreEqual(0, element[0]);
				Assert.DoesNotThrow(delegate () { int x = element[1]; });
				Assert.AreEqual(0, element[1]);
				Assert.DoesNotThrow(delegate () { int x = element[2]; });
				Assert.AreEqual(2, element[2]);
				Assert.Catch(delegate () { int x = element[3]; });
				Assert.AreEqual(0, element.GetLowEnd());
				Assert.AreEqual(2, element.GetHighEnd());
			}

			element[1] = 1;
			element[-1] = -1;
			{
				Assert.Catch(delegate () { int x = element[-2]; });
				Assert.DoesNotThrow(delegate () { int x = element[-1]; });
				Assert.AreEqual(-1, element[-1]);
				Assert.DoesNotThrow(delegate () { int x = element[0]; });
				Assert.AreEqual(0, element[0]);
				Assert.DoesNotThrow(delegate () { int x = element[1]; });
				Assert.AreEqual(1, element[1]);
				Assert.DoesNotThrow(delegate () { int x = element[2]; });
				Assert.AreEqual(2, element[2]);
				Assert.Catch(delegate () { int x = element[3]; });
				Assert.AreEqual(-1, element.GetLowEnd());
				Assert.AreEqual(2, element.GetHighEnd());
			}

			element[3] = 3;
			{
				Assert.Catch(delegate () { int x = element[-2]; });
				Assert.DoesNotThrow(delegate () { int x = element[-1]; });
				Assert.AreEqual(-1, element[-1]);
				Assert.DoesNotThrow(delegate () { int x = element[0]; });
				Assert.AreEqual(0, element[0]);
				Assert.DoesNotThrow(delegate () { int x = element[1]; });
				Assert.AreEqual(1, element[1]);
				Assert.DoesNotThrow(delegate () { int x = element[2]; });
				Assert.AreEqual(2, element[2]);
				Assert.DoesNotThrow(delegate () { int x = element[3]; });
				Assert.AreEqual(3, element[3]);
				Assert.Catch(delegate () { int x = element[4]; });
				Assert.AreEqual(-1, element.GetLowEnd());
				Assert.AreEqual(3, element.GetHighEnd());
			}

			element.FreeUpTo(1);
			{
				Assert.Catch(delegate () { int x = element[-2]; });
				Assert.Catch(delegate () { int x = element[-1]; });
				Assert.Catch(delegate () { int x = element[0]; });
				Assert.DoesNotThrow(delegate () { int x = element[1]; });
				Assert.AreEqual(1, element[1]);
				Assert.DoesNotThrow(delegate () { int x = element[2]; });
				Assert.AreEqual(2, element[2]);
				Assert.DoesNotThrow(delegate () { int x = element[3]; });
				Assert.AreEqual(3, element[3]);
				Assert.Catch(delegate () { int x = element[4]; });
				Assert.AreEqual(1, element.GetLowEnd());
				Assert.AreEqual(3, element.GetHighEnd());
			}

			element.FreeDownTo(2);
			{
				Assert.Catch(delegate () { int x = element[0]; });
				Assert.DoesNotThrow(delegate () { int x = element[1]; });
				Assert.AreEqual(1, element[1]);
				Assert.DoesNotThrow(delegate () { int x = element[2]; });
				Assert.AreEqual(2, element[2]);
				Assert.Catch(delegate () { int x = element[3]; });
				Assert.Catch(delegate () { int x = element[4]; });
				Assert.AreEqual(1, element.GetLowEnd());
				Assert.AreEqual(2, element.GetHighEnd());
			}

			element[3] = 10;
			element[30] = 30;
			{
				Assert.Catch(delegate () { int x = element[0]; });
				Assert.DoesNotThrow(delegate () { int x = element[1]; });
				Assert.AreEqual(1, element[1]);
				Assert.DoesNotThrow(delegate () { int x = element[2]; });
				Assert.AreEqual(2, element[2]);
				Assert.DoesNotThrow(delegate () { int x = element[3]; });
				Assert.AreEqual(10, element[3]);
				Assert.DoesNotThrow(delegate () { int x = element[30]; });
				Assert.AreEqual(30, element[30]);
				Assert.Catch(delegate () { int x = element[31]; });
				Assert.AreEqual(1, element.GetLowEnd());
				Assert.AreEqual(30, element.GetHighEnd());
			}

			element[0] = 22;
			element[31] = 31;
			{
				Assert.Catch(delegate () { int x = element[-1]; });
				Assert.DoesNotThrow(delegate () { int x = element[0]; });
				Assert.AreEqual(22, element[0]);
				Assert.DoesNotThrow(delegate () { int x = element[1]; });
				Assert.AreEqual(1, element[1]);
				Assert.DoesNotThrow(delegate () { int x = element[2]; });
				Assert.AreEqual(2, element[2]);
				Assert.DoesNotThrow(delegate () { int x = element[3]; });
				Assert.AreEqual(10, element[3]);
				Assert.DoesNotThrow(delegate () { int x = element[30]; });
				Assert.AreEqual(30, element[30]);
				Assert.DoesNotThrow(delegate () { int x = element[31]; });
				Assert.AreEqual(31, element[31]);
				Assert.Catch(delegate () { int x = element[32]; });
				Assert.AreEqual(0, element.GetLowEnd());
				Assert.AreEqual(31, element.GetHighEnd());
			}

			Debug.Log("befor");
			Debug.Log(element.ToString());

			element[32] = 32;

			Debug.Log("after");
			Debug.Log(element.ToString());
		}
	}
}
