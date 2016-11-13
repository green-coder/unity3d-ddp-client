using System;

namespace DDP {

	public static class JSONObjectExtensions {

		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime GetDateTime(this JSONObject obj) {
			return epoch.AddMilliseconds(obj["$date"].i);
		}

	}

}
