using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ristretto.Test
{
    [TestClass]
    public class CompressedEdwardsYTest
    {
        [TestMethod]
        public void constructorDoesNotThrowOnCorrectLength()
        {
            new CompressedEdwardsY(new byte[32]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void constructorThrowsOnTooShort()
        {
            new CompressedEdwardsY(new byte[31]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void constructorThrowsOnTooLong()
        {
            new CompressedEdwardsY(new byte[33]);
        }

        //[TestMethod]
        //public void serializeDeserialize()
        //{
        //    byte[] s = new byte[32];
        //    s[0] = 0x1f;
        //    CompressedEdwardsY expected = new CompressedEdwardsY(s);
        //    ByteArrayOutputStream baos = new ByteArrayOutputStream();
        //    ObjectOutputStream oos = new ObjectOutputStream(baos);
        //    oos.writeObject(expected);
        //    oos.close();
        //    ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
        //    ObjectInputStream ois = new ObjectInputStream(bais);
        //    CompressedEdwardsY actual = (CompressedEdwardsY)ois.readObject();
        //    Assert.AreEqual(expected, actual);
        //}

        [TestMethod]
        public void toByteArray()
        {
            byte[] s = new byte[32];
            s[0] = 0x1f;
            CollectionAssert.AreEqual(s, new CompressedEdwardsY(s).ToByteArray());
            //assertThat(new CompressedEdwardsY(s).toByteArray(), is (s));
        }
    }
}
