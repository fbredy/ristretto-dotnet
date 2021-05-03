using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ristretto.Test
{
    [TestClass]
    public class CompressedRistrettoTest
    {
        [TestMethod]
        public void constructorDoesNotThrowOnCorrectLength()
        {
            new CompressedRistretto(new byte[32]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void constructorThrowsOnTooShort()
        {
            new CompressedRistretto(new byte[31]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void constructorThrowsOnTooLong()
        {
            new CompressedRistretto(new byte[33]);
        }

        //[TestMethod]
        //public void serializeDeserialize()
        //{
        //    byte[] s = new byte[32];
        //    s[0] = 0x1f;
        //    CompressedRistretto expected = new CompressedRistretto(s);
        //    ByteArrayOutputStream baos = new ByteArrayOutputStream();
        //    ObjectOutputStream oos = new ObjectOutputStream(baos);
        //    oos.writeObject(expected);
        //    oos.close();
        //    ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
        //    ObjectInputStream ois = new ObjectInputStream(bais);
        //    CompressedRistretto actual = (CompressedRistretto)ois.readObject();
        //    assertThat(actual, is (expected));
        //}

        [TestMethod]
        public void toByteArray()
        {
            byte[] s = new byte[32];
            s[0] = 0x1f;
            CollectionAssert.AreEqual(s, new CompressedRistretto(s).ToByteArray());
        }

        [TestMethod]
        public void equalityRequiresSameClass()
        {
            byte[] s = new byte[32];
            CompressedRistretto r = new CompressedRistretto(s);
            CompressedEdwardsY e = new CompressedEdwardsY(s);
            Assert.AreNotEqual(r, e);
        }
    }
}
