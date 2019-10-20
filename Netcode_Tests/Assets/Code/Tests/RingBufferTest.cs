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

			RingBuffer<int> target = new RingBuffer<int>();

			target[0] = 0;
			target[2] = 2;
			{
				Assert.Catch(delegate () { int x = target[-1]; });
				Assert.DoesNotThrow(delegate () { int x = target[0]; });
				Assert.AreEqual(0, target[0]);
				Assert.DoesNotThrow(delegate () { int x = target[1]; });
				Assert.AreEqual(0, target[1]);
				Assert.DoesNotThrow(delegate () { int x = target[2]; });
				Assert.AreEqual(2, target[2]);
				Assert.Catch(delegate () { int x = target[3]; });
				Assert.AreEqual(0, target.GetLowEnd());
				Assert.AreEqual(2, target.GetHighEnd());
			}

			target[1] = 1;
			target[-1] = -1;
			{
				Assert.Catch(delegate () { int x = target[-2]; });
				Assert.DoesNotThrow(delegate () { int x = target[-1]; });
				Assert.AreEqual(-1, target[-1]);
				Assert.DoesNotThrow(delegate () { int x = target[0]; });
				Assert.AreEqual(0, target[0]);
				Assert.DoesNotThrow(delegate () { int x = target[1]; });
				Assert.AreEqual(1, target[1]);
				Assert.DoesNotThrow(delegate () { int x = target[2]; });
				Assert.AreEqual(2, target[2]);
				Assert.Catch(delegate () { int x = target[3]; });
				Assert.AreEqual(-1, target.GetLowEnd());
				Assert.AreEqual(2, target.GetHighEnd());
			}

			target[3] = 3;
			{
				Assert.Catch(delegate () { int x = target[-2]; });
				Assert.DoesNotThrow(delegate () { int x = target[-1]; });
				Assert.AreEqual(-1, target[-1]);
				Assert.DoesNotThrow(delegate () { int x = target[0]; });
				Assert.AreEqual(0, target[0]);
				Assert.DoesNotThrow(delegate () { int x = target[1]; });
				Assert.AreEqual(1, target[1]);
				Assert.DoesNotThrow(delegate () { int x = target[2]; });
				Assert.AreEqual(2, target[2]);
				Assert.DoesNotThrow(delegate () { int x = target[3]; });
				Assert.AreEqual(3, target[3]);
				Assert.Catch(delegate () { int x = target[4]; });
				Assert.AreEqual(-1, target.GetLowEnd());
				Assert.AreEqual(3, target.GetHighEnd());
			}

			target.FreeUpTo(1);
			{
				Assert.Catch(delegate () { int x = target[-2]; });
				Assert.Catch(delegate () { int x = target[-1]; });
				Assert.Catch(delegate () { int x = target[0]; });
				Assert.DoesNotThrow(delegate () { int x = target[1]; });
				Assert.AreEqual(1, target[1]);
				Assert.DoesNotThrow(delegate () { int x = target[2]; });
				Assert.AreEqual(2, target[2]);
				Assert.DoesNotThrow(delegate () { int x = target[3]; });
				Assert.AreEqual(3, target[3]);
				Assert.Catch(delegate () { int x = target[4]; });
				Assert.AreEqual(1, target.GetLowEnd());
				Assert.AreEqual(3, target.GetHighEnd());
			}

			target.FreeDownTo(2);
			{
				Assert.Catch(delegate () { int x = target[0]; });
				Assert.DoesNotThrow(delegate () { int x = target[1]; });
				Assert.AreEqual(1, target[1]);
				Assert.DoesNotThrow(delegate () { int x = target[2]; });
				Assert.AreEqual(2, target[2]);
				Assert.Catch(delegate () { int x = target[3]; });
				Assert.Catch(delegate () { int x = target[4]; });
				Assert.AreEqual(1, target.GetLowEnd());
				Assert.AreEqual(2, target.GetHighEnd());
			}

			target[3] = 10;
			target[30] = 30;
			{
				Assert.Catch(delegate () { int x = target[0]; });
				Assert.DoesNotThrow(delegate () { int x = target[1]; });
				Assert.AreEqual(1, target[1]);
				Assert.DoesNotThrow(delegate () { int x = target[2]; });
				Assert.AreEqual(2, target[2]);
				Assert.DoesNotThrow(delegate () { int x = target[3]; });
				Assert.AreEqual(10, target[3]);
				Assert.DoesNotThrow(delegate () { int x = target[30]; });
				Assert.AreEqual(30, target[30]);
				Assert.Catch(delegate () { int x = target[31]; });
				Assert.AreEqual(1, target.GetLowEnd());
				Assert.AreEqual(30, target.GetHighEnd());
			}

			target[0] = 22;
			target[31] = 31;
			{
				Assert.Catch(delegate () { int x = target[-1]; });
				Assert.DoesNotThrow(delegate () { int x = target[0]; });
				Assert.AreEqual(22, target[0]);
				Assert.DoesNotThrow(delegate () { int x = target[1]; });
				Assert.AreEqual(1, target[1]);
				Assert.DoesNotThrow(delegate () { int x = target[2]; });
				Assert.AreEqual(2, target[2]);
				Assert.DoesNotThrow(delegate () { int x = target[3]; });
				Assert.AreEqual(10, target[3]);
				Assert.DoesNotThrow(delegate () { int x = target[30]; });
				Assert.AreEqual(30, target[30]);
				Assert.DoesNotThrow(delegate () { int x = target[31]; });
				Assert.AreEqual(31, target[31]);
				Assert.Catch(delegate () { int x = target[32]; });
				Assert.AreEqual(0, target.GetLowEnd());
				Assert.AreEqual(31, target.GetHighEnd());
			}

			Debug.Log("befor");
			Debug.Log(target.ToString());

			target[32] = 32;

			Debug.Log("after");
			Debug.Log(target.ToString());
		}
	}
}
