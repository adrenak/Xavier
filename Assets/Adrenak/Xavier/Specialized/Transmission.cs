using System;

namespace Adrenak.Xavier.Specialized {
	[Serializable]
	public class Transmission {
		public string id;
		public byte[] bytes;
	}

	[Serializable]
	public class Invocation {
		public string method;
		public string invokeID;
		public object obj;
	}
}
