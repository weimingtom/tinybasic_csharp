/*
 * 由SharpDevelop创建。
 * 用户： a
 * 日期: 2022/12/18
 * 时间: 10:47
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Diagnostics;
using System.IO;
using AT.MIN;

namespace tinybasic
{
	/// <summary>
	/// Description of CLib.
	/// </summary>
	public partial class TinyBasic
	{
		public class DoublePtr
		{
			public double[] chars;
			public int index;
			
			public double this[int offset]
			{
				get { return chars[index + offset]; }
				set { chars[index + offset] = value; }
			}
			public double this[uint offset]
			{
				get { return chars[index + offset]; }
				set { chars[index + offset] = value; }
			}
			public double this[long offset]
			{
				get { return chars[index + (int)offset]; }
				set { chars[index + (int)offset] = value; }
			}

//			public static implicit operator CharPtr(string str) { return new CharPtr(str); }
			public static implicit operator DoublePtr(double[] chars) { return new DoublePtr(chars); }

			public DoublePtr()
			{
				this.chars = null;
				this.index = 0;
			}

//			public CharPtr(string str)
//			{
//				this.chars = (str + '\0').ToCharArray();
//				this.index = 0;
//			}

			public DoublePtr(DoublePtr ptr)
			{
				this.chars = ptr.chars;
				this.index = ptr.index;
			}

			public DoublePtr(DoublePtr ptr, int index)
			{
				this.chars = ptr.chars;
				this.index = index;
			}

			public DoublePtr(double[] chars)
			{
				this.chars = chars;
				this.index = 0;
			}

			public DoublePtr(double[] chars, int index)
			{
				this.chars = chars;
				this.index = index;
			}

			public static DoublePtr operator +(DoublePtr ptr, int offset) {return new DoublePtr(ptr.chars, ptr.index+offset);}
			public static DoublePtr operator -(DoublePtr ptr, int offset) {return new DoublePtr(ptr.chars, ptr.index-offset);}
			public static DoublePtr operator +(DoublePtr ptr, uint offset) { return new DoublePtr(ptr.chars, ptr.index + (int)offset); }
			public static DoublePtr operator -(DoublePtr ptr, uint offset) { return new DoublePtr(ptr.chars, ptr.index - (int)offset); }

			public void inc() { this.index++; }
			public void dec() { this.index--; }
			public DoublePtr next() { return new DoublePtr(this.chars, this.index + 1); }
			public DoublePtr prev() { return new DoublePtr(this.chars, this.index - 1); }
			public DoublePtr add(int ofs) { return new DoublePtr(this.chars, this.index + ofs); }
			public DoublePtr sub(int ofs) { return new DoublePtr(this.chars, this.index - ofs); }
			
			public static bool operator ==(DoublePtr ptr, int ch) { return ptr[0] == ch; }
			public static bool operator ==(int ch, DoublePtr ptr) { return ptr[0] == ch; }
			public static bool operator !=(DoublePtr ptr, int ch) { return ptr[0] != ch; }
			public static bool operator !=(int ch, DoublePtr ptr) { return ptr[0] != ch; }

//			public static CharPtr operator +(BytePtr ptr1, BytePtr ptr2)
//			{
//				string result = "";
//				for (int i = 0; ptr1[i] != '\0'; i++)
//					result += ptr1[i];
//				for (int i = 0; ptr2[i] != '\0'; i++)
//					result += ptr2[i];
//				return new CharPtr(result);
//			}
			public static int operator -(DoublePtr ptr1, DoublePtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index - ptr2.index; }
			public static bool operator <(DoublePtr ptr1, DoublePtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index < ptr2.index; }
			public static bool operator <=(DoublePtr ptr1, DoublePtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index <= ptr2.index; }
			public static bool operator >(DoublePtr ptr1, DoublePtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index > ptr2.index; }
			public static bool operator >=(DoublePtr ptr1, DoublePtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index >= ptr2.index; }
			public static bool operator ==(DoublePtr ptr1, DoublePtr ptr2) {
				object o1 = ptr1 as DoublePtr;
				object o2 = ptr2 as DoublePtr;
				if ((o1 == null) && (o2 == null)) return true;
				if (o1 == null) return false;
				if (o2 == null) return false;
				return (ptr1.chars == ptr2.chars) && (ptr1.index == ptr2.index); }
			public static bool operator !=(DoublePtr ptr1, DoublePtr ptr2) {return !(ptr1 == ptr2); }

			public override bool Equals(object o)
			{
				return this == (o as DoublePtr);
			}

			public override int GetHashCode()
			{
				return 0;
			}
//			public override string ToString()
//			{
//				string result = "";
//				for (int i = index; (i<chars.Length) && (chars[i] != '\0'); i++)
//					result += chars[i];
//				return result;
//			}
		}
		
		
		
		
		public class CharPtr
		{
			public bool checkChange = false;
			public char[] chars;
			private int _index;
			public int index
			{
				get
				{
					return _index;
				}
				set
				{
					if (checkChange)
					{
						Debug.Assert(false, "index changed");
					}
					_index = value;
				}
			}
			
			public char this[int offset]
			{
				get { return chars[index + offset]; }
				set { chars[index + offset] = value; }
			}
			public char this[uint offset]
			{
				get { return chars[index + offset]; }
				set { chars[index + offset] = value; }
			}
			public char this[long offset]
			{
				get { return chars[index + (int)offset]; }
				set { chars[index + (int)offset] = value; }
			}

			public static implicit operator CharPtr(string str) { return new CharPtr(str); }
			public static implicit operator CharPtr(char[] chars) { return new CharPtr(chars); }

			public CharPtr()
			{
				this.chars = null;
				this.index = 0;
			}

			public CharPtr(char str)
			{
				this.chars = (("" + str) + '\0').ToCharArray();
				this.index = 0;
			}
			
			public CharPtr(string str)
			{
				this.chars = (str + '\0').ToCharArray();
				this.index = 0;
			}

			public CharPtr(CharPtr ptr)
			{
				this.chars = ptr.chars;
				this.index = ptr.index;
			}

			public CharPtr(CharPtr ptr, bool checkChange)
			{
				this.chars = ptr.chars;
				this.index = ptr.index;
				this.checkChange = checkChange;
			}
			
			public CharPtr(CharPtr ptr, int index)
			{
				this.chars = ptr.chars;
				this.index = index;
			}

			public CharPtr(char[] chars)
			{
				this.chars = chars;
				this.index = 0;
			}

			public CharPtr(char[] chars, int index)
			{
				this.chars = chars;
				this.index = index;
			}

			public CharPtr(IntPtr ptr)
			{
				this.chars = new char[0];
				this.index = 0;
			}

			public static CharPtr operator +(CharPtr ptr, int offset) {return new CharPtr(ptr.chars, ptr.index+offset);}
			public static CharPtr operator -(CharPtr ptr, int offset) {return new CharPtr(ptr.chars, ptr.index-offset);}
			public static CharPtr operator +(CharPtr ptr, uint offset) { return new CharPtr(ptr.chars, ptr.index + (int)offset); }
			public static CharPtr operator -(CharPtr ptr, uint offset) { return new CharPtr(ptr.chars, ptr.index - (int)offset); }

			public void inc() { this.index++; }
			public void dec() { this.index--; }
			public CharPtr next() { return new CharPtr(this.chars, this.index + 1); }
			public CharPtr prev() { return new CharPtr(this.chars, this.index - 1); }
			public CharPtr add(int ofs) { return new CharPtr(this.chars, this.index + ofs); }
			public CharPtr sub(int ofs) { return new CharPtr(this.chars, this.index - ofs); }
			
			public static bool operator ==(CharPtr ptr, char ch) { return ptr[0] == ch; }
			public static bool operator ==(char ch, CharPtr ptr) { return ptr[0] == ch; }
			public static bool operator !=(CharPtr ptr, char ch) { return ptr[0] != ch; }
			public static bool operator !=(char ch, CharPtr ptr) { return ptr[0] != ch; }

			public static CharPtr operator +(CharPtr ptr1, CharPtr ptr2)
			{
				string result = "";
				for (int i = 0; ptr1[i] != '\0'; i++)
					result += ptr1[i];
				for (int i = 0; ptr2[i] != '\0'; i++)
					result += ptr2[i];
				return new CharPtr(result);
			}
			public static int operator -(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index - ptr2.index; }
			public static bool operator <(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index < ptr2.index; }
			public static bool operator <=(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index <= ptr2.index; }
			public static bool operator >(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index > ptr2.index; }
			public static bool operator >=(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index >= ptr2.index; }
			public static bool operator ==(CharPtr ptr1, CharPtr ptr2) {
				object o1 = ptr1 as CharPtr;
				object o2 = ptr2 as CharPtr;
				if ((o1 == null) && (o2 == null)) return true;
				if (o1 == null) return false;
				if (o2 == null) return false;
				return (ptr1.chars == ptr2.chars) && (ptr1.index == ptr2.index); }
			public static bool operator !=(CharPtr ptr1, CharPtr ptr2) {return !(ptr1 == ptr2); }

			public override bool Equals(object o)
			{
				return this == (o as CharPtr);
			}

			public override int GetHashCode()
			{
				return 0;
			}
			public override string ToString()
			{
				string result = "";
				for (int i = index; (i<chars.Length) && (chars[i] != '\0'); i++)
					result += chars[i];
				return result;
			}
		}
		
		
		public static int isalpha(char c)
		{
			return Char.IsLetter(c) ? 1 : 0;
		}
		
		public static int isdigit(int c)
		{
			return Char.IsDigit((char)c) ? 1 : 0;
		}
		
		public static char toupper(char c)
		{
			return Char.ToUpper(c);
		}
		
		public static char tolower(char c)
		{
			return Char.ToLower(c);
		}
		
		public static CharPtr malloc(uint n) 
		{
			return new CharPtr(new char[n]);
		}
		
		public static void free(object obj)
		{
			
		}
		
		public static CharPtr strcpy(CharPtr dst, CharPtr src)
		{
			int i;
			for (i = 0; src[i] != '\0'; i++)
				dst[i] = src[i];
			dst[i] = '\0';
			return dst;
		}
		
		public static int strcmp(CharPtr s1, CharPtr s2)
		{
			if (s1 == s2)
				return 0;
			if (s1 == null)
				return -1;
			if (s2 == null)
				return 1;

			for (int i = 0; ; i++)
			{
				if (s1[i] != s2[i])
				{
					if (s1[i] < s2[i])
						return -1;
					else
						return 1;
				}
				if (s1[i] == '\0')
					return 0;
			}
		}
		
		public static CharPtr strchr(CharPtr str, char c)
		{
			if (c != '\0')
			{
				for (int index = str.index; str.chars[index] != 0; index++)
					if (str.chars[index] == c)
						return new CharPtr(str.chars, index);
			}
			else
			{
				for (int index = str.index; index < str.chars.Length; index++)
					if (str.chars[index] == c)
						return new CharPtr(str.chars, index);
			}
			return null;
		}
		
		public static uint strlen(CharPtr str)
		{
			uint index = 0;
			while (str[index] != '\0')
				index++;
			return index;
		}
		
				
				
				
				
				
				
				
				
				
				
				
				
				
				

		
		public class FILE
		{
			public Stream stream;
			
			public FILE()
			{
				
			}
			
			public FILE(Stream stream)
			{
				this.stream = stream;
			}
		}
		
		public static int fgetc(FILE fp)
		{
			int result = fp.stream.ReadByte();
			/*
			if (result == (int)'\r') //FIXME: only tested under Windows
			{
				result = fp.stream.ReadByte();
			}*/
			return result;
		}
		public static void ungetc(int c, FILE fp)
		{
			if (fp.stream.Position > 0)
				fp.stream.Seek(-1, SeekOrigin.Current);
		}
		
		public static int EOF = -1;
		
		public static FILE fopen(CharPtr filename, CharPtr mode)
		{
			FileStream stream = null;
			string str = filename.ToString();			
			FileMode filemode = FileMode.Open;
			FileAccess fileaccess = (FileAccess)0;			
			for (int i=0; mode[i] != '\0'; i++)
				switch (mode[i])
				{
					case 'r': 
						fileaccess = fileaccess | FileAccess.Read;
						if (!File.Exists(str))
							return null;
						break;

					case 'w':
						filemode = FileMode.Create;
						fileaccess = fileaccess | FileAccess.Write;
						break;
						
					case 'b':
						break;
				}
			try
			{
				stream = new FileStream(str, filemode, fileaccess);
			}
			catch
			{
				stream = null;
			}			
			
			FILE ret = new FILE();
			ret.stream = stream;
			return ret;
		}
		
		public static void fclose(FILE fp)
		{
			try
			{
				fp.stream.Flush();
				fp.stream.Close();
			}
			catch { }
		}
		
		public static FILE stdin = new FILE(Console.OpenStandardInput());
		public static FILE stdout = new FILE(Console.OpenStandardOutput());
		public static FILE stderr = new FILE(Console.OpenStandardError());
		
		public static int getc(FILE fin)
		{
			return fgetc(fin);
		}
		
		public static int feof(FILE fin)
		{
			//FIXME:
			return fin.stream.Position >= fin.stream.Length ? 1 : 0;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		public static int fprintf(FILE fp, string str, params object[] argv)
		{
			string result = Tools.sprintf(str.ToString(), argv);
			char[] chars = result.ToCharArray();
			byte[] bytes = new byte[chars.Length];
			for (int i=0; i<chars.Length; i++)
				bytes[i] = (byte)chars[i];
			fp.stream.Write(bytes, 0, bytes.Length);
			return 1; //Returns the number of characters printed
		}
		public static int printf(string str, params object[] argv)
		{
			Tools.printf(str.ToString(), argv);
			return 1; //Returns the number of characters printed
		}
		public static int fscanf(FILE fp, CharPtr format, params object[] argp)
		{
			string str = null;
			while (true) {
				str = Console.ReadLine();
				if (str == null) { //FIXME: Ctrl+C
					if (false) {
						continue;
					} else {
						throw new LongjmpException();
					}
				}
				str = str.Trim();
				if (str != null && !str.Equals("")) {
					break;
				}
			}
			return sscanf(str, format, argp);
		}
		public static int scanf(CharPtr format, params object[] argp)
		{
			return fscanf(stdin, format, argp);
		}
		private static int sscanf(string str, CharPtr fmt, params object[] argp)
		{
			int parm_index = 0;
			int index = 0;
			while (fmt[index] != 0)
			{
				if (fmt[index++]=='%')
					switch (fmt[index++])
					{
						case 's':
							{
								argp[parm_index++] = str;
								break;
							}
						case 'c':
							{
								argp[parm_index++] = Convert.ToChar(str);
								break;
							}
						case 'd':
							{
								argp[parm_index++] = Convert.ToInt32(str);
								break;
							}
						case 'l':
							{
								argp[parm_index++] = Convert.ToDouble(str);
								break;
							}
						case 'f':
							{
								argp[parm_index++] = Convert.ToDouble(str);
								break;
							}
						//case 'p': //FIXME:
						//    {
						//        result += "(pointer)";
						//        break;
						//    }
					}
			}
			return parm_index;
		}
		
				
		public class jmp_buf {
			
		}
		
		public class LongjmpException : Exception {
			
		}
		
		public static void longjmp(jmp_buf buf, int a)
		{
			//throw new Exception("not implement");
			throw new LongjmpException();
		}
		
		public static int setjmp(jmp_buf buf)
		{
			throw new Exception("not implement");
		}
		
		public static double atof(CharPtr nptr)
		{
			return Convert.ToDouble(nptr.ToString());
		}
		
		public static void exit(int code)
		{
			Environment.Exit(code);
		}
		
		public static double floor(double x)
		{
			return Math.Floor(x);
		}
	}
}
