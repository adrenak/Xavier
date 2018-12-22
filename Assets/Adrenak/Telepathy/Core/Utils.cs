using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

using Object = System.Object;

namespace Adrenak.Telepathy {
	public class Utils {
		public static Object ByteArrayToObject(byte[] arrBytes) {
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write(arrBytes, 0, arrBytes.Length);
			memStream.Seek(0, SeekOrigin.Begin);
			Object obj = (Object)binForm.Deserialize(memStream);
			return obj;
		}

		public static byte[] ObjectToByteArray(Object obj) {
			if (obj == null)
				return null;
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, obj);
			return ms.ToArray();
		}
	}
}
