using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLocoTool.DatFileParsing
{
	public class StringTableEncoding : Encoding
	{
		public override int GetByteCount(char[] chars, int index, int count)
		{
			throw new NotImplementedException();
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			throw new NotImplementedException();
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			throw new NotImplementedException();
		}

		public Rune toUnicode(ReadOnlySpan<byte> bytes)
		{
			var result = new Rune(bytes[0]);
			if (bytes[0] == 0xFF)
			{
				result = new Rune(ByteReaderT.Read_uint16t(bytes,1));
			}
			return result;
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			
			throw new NotImplementedException();
		}

		public override int GetMaxByteCount(int charCount)
		{
			throw new NotImplementedException();
		}

		public override int GetMaxCharCount(int byteCount)
		{
			throw new NotImplementedException();
		}
	}
}
