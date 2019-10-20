﻿using System.Collections;
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
		}
	}
}