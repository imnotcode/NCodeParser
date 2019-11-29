using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NCodeParser
{
	public static class NativeMethods
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(String fileName);

		[DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi)]
		public static extern IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

		[DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
		public static extern bool FreeLibrary(int hModule);
	}
}
