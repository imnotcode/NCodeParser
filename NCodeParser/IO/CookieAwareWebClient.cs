using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NCodeParser.IO
{
	public class CookieAwareWebClient : WebClient
	{
		public CookieContainer CookieContainer
		{
			get;
			private set;
		}

		public CookieAwareWebClient()
		{
			CookieContainer = new CookieContainer();
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			var Request = (HttpWebRequest) base.GetWebRequest(address);
			Request.CookieContainer = CookieContainer;

			return Request;
		}
	}
}
