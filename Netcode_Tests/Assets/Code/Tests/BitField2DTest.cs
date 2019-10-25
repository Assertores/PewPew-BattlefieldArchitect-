using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
	public class BitField2DTest {

		[Test]
		public void BitField2DTestSimplePasses() {

			BitField2D target = new BitField2D(2, 8);
			{
				Assert.DoesNotThrow(delegate () { bool tmp = target[0, 0]; });
				Assert.Catch(delegate () { bool tmp = target[-1, 0]; });
				Assert.Catch(delegate () { bool tmp = target[2, 0]; });
				Assert.Catch(delegate () { bool tmp = target[0, -1]; });
				Assert.Catch(delegate () { bool tmp = target[0, 8]; });

				Assert.DoesNotThrow(delegate () { target[0, 0] = false; });
				Assert.Catch(delegate () { target[-1, 0] = false; });
				Assert.Catch(delegate () { target[2, 0] = false; });
				Assert.Catch(delegate () { target[0, -1] = false; });
				Assert.Catch(delegate () { target[0, 8] = false; });

				Assert.AreEqual(false, target[0, 0]);
				Assert.AreEqual(false, target[0, 1]);
				Assert.AreEqual(false, target[0, 2]);
				Assert.AreEqual(false, target[0, 3]);
				Assert.AreEqual(false, target[1, 0]);
				Assert.AreEqual(false, target[1, 1]);
				Assert.AreEqual(false, target[1, 2]);
				Assert.AreEqual(false, target[1, 3]);
			}

			target[0, 0] = true;
			{
				Assert.AreEqual(true, target[0, 0]);
			}

			target[0, 0] = false;
			{
				Assert.AreEqual(false, target[0, 0]);
			}

			target[1, 0] = true;
			target[1, 3] = true;
			{
				Assert.AreEqual(true, target[1, 0]);
				Assert.AreEqual(true, target[1, 3]);
			}

			byte[] retVal = target.ToArray();
			target = null;
			target = new BitField2D(2, 3, retVal);
			{
				Assert.AreEqual(true, target[1, 0]);
				Assert.AreEqual(true, target[1, 3]);
				Assert.Catch(delegate () { bool tmp = target[0, 4]; });
			}

			BitField2D a = new BitField2D(2, 4);
			BitField2D b = new BitField2D(2, 4);
			a[0, 0] = true;
			a[0, 1] = true;
			b[1, 2] = true;
			b[1, 3] = true;

			target = a + b;
			{
				Assert.AreEqual(true, target[0, 0]);
				Assert.AreEqual(true, target[0, 1]);
				Assert.AreEqual(true, target[1, 2]);
				Assert.AreEqual(true, target[1, 3]);
			}

			a.FromArray(target.ToArray());
			{
				Assert.AreEqual(true, a[0, 0]);
				Assert.AreEqual(true, a[0, 1]);
				Assert.AreEqual(true, a[1, 2]);
				Assert.AreEqual(true, a[1, 3]);
			}

			byte[] tmpByteArray = new byte[2];
			{
				Assert.Catch(delegate () { b.FromArray(tmpByteArray); });
			}

			Vector2[] result = target.GetActiveBits();
			{
				Assert.AreEqual(4, result.Length);
				Assert.AreEqual(new Vector2(0, 0), result[0]);
				Assert.AreEqual(new Vector2(0, 1), result[1]);
				Assert.AreEqual(new Vector2(1, 2), result[2]);
				Assert.AreEqual(new Vector2(1, 3), result[3]);
			}

			result = target.GetInactiveBits();
			{
				Assert.AreEqual(4, result.Length);
				Assert.AreEqual(new Vector2(0, 2), result[2]);
				Assert.AreEqual(new Vector2(0, 3), result[3]);
				Assert.AreEqual(new Vector2(1, 0), result[0]);
				Assert.AreEqual(new Vector2(1, 1), result[1]);
			}

			BitField2D targetCoppy1 = new BitField2D(target, false);
			BitField2D targetCoppy2 = new BitField2D(target, true);
			{
				Assert.AreEqual(true, targetCoppy1[0, 0]);
				Assert.AreEqual(true, targetCoppy1[0, 1]);
				Assert.AreEqual(true, targetCoppy1[1, 2]);
				Assert.AreEqual(true, targetCoppy1[1, 3]);

				Assert.AreEqual(false, targetCoppy2[0, 0]);
				Assert.AreEqual(false, targetCoppy2[0, 1]);
				Assert.AreEqual(false, targetCoppy2[1, 2]);
				Assert.AreEqual(false, targetCoppy2[1, 3]);
			}

			target[0, 0] = true;
			target[0, 1] = true;
			target[0, 2] = true;
			target[0, 3] = true;
			target[1, 0] = true;
			target[1, 1] = true;
			target[1, 2] = true;
			target[1, 3] = true;
			{
				Assert.AreEqual(false, targetCoppy1.AreAllBytesActive());
				Assert.AreEqual(false, targetCoppy1.AreAllByteInactive());

				Assert.AreEqual(true, targetCoppy2.AreAllByteInactive());
				Assert.AreEqual(true, target.AreAllBytesActive());
			}

			a[0, 0] = false;
			a[0, 1] = true;
			a[0, 2] = false;
			a[0, 3] = true;
			a[1, 0] = false;
			a[1, 1] = false;
			a[1, 2] = true;
			a[1, 3] = true;

			b[0, 0] = true;
			b[0, 1] = true;
			b[0, 2] = false;
			b[0, 3] = false;
			b[1, 0] = false;
			b[1, 1] = false;
			b[1, 2] = true;
			b[1, 3] = true;
			target = a - b;
			{
				Assert.AreEqual(false, target[0, 0]);
				Assert.AreEqual(false, target[0, 1]);
				Assert.AreEqual(false, target[0, 2]);
				Assert.AreEqual(true, target[0, 3]);
				Assert.AreEqual(false, target[1, 0]);
				Assert.AreEqual(false, target[1, 1]);
				Assert.AreEqual(false, target[1, 2]);
				Assert.AreEqual(false, target[1, 3]);
			}
		}
	}
}
