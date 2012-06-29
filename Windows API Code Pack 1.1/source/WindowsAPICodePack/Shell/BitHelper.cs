using System;

namespace ZiadSpace.Util
{

	/// <summary>
	/// Helps perform certain operations on primative types
	/// that deal with bits
	/// </summary>
	public sealed class BitHelper
	{
		/// <summary>
		/// The max number of bits in byte
		/// </summary>
		public const int BIT_SIZE_BYTE = 8;
		/// <summary>
		/// The max number of bits in short 
		/// </summary>
		public const int BIT_SIZE_SHORT = 16;
		/// <summary>
		/// The max number of bits in int
		/// </summary>
		public const int BIT_SIZE_INT = 32;
		/// <summary>
		/// The max number of bits in long
		/// </summary>
		public const int BIT_SIZE_LONG = 64;


		/// <summary>
		/// Private constructor
		/// </summary>
		private BitHelper()
		{
		}




		#region Byte Methods
		/// <summary>
		/// Gets the size of the input value in bits
		/// </summary>
		/// <param name="pInput">The input value</param>
		/// <returns></returns>
		public static int SizeOf ( byte pInput )
		{
			int iRetval = 0;
			if ( pInput == 0 )
			{
				iRetval = 0;
			}
			else if ( pInput == 1 )
			{
				iRetval = 1;
			}
			else if ( pInput < 0 )
			{
				iRetval = BIT_SIZE_BYTE;
			}
			else 
			{
				int lTemp = 0;
				for ( int i = BIT_SIZE_BYTE -1; i > 1; i-- )
				{
					lTemp = 1 << i-1;
					if ( (pInput & lTemp) == lTemp )
					{
						iRetval = i;
						break;
					}
				}
			}
			return iRetval;
		}


		/// <summary>
		/// Gets the bits from a number as a number.
		/// </summary>
		/// <param name="pInput">The input value.</param>
		/// <param name="pStart">The start position.</param>
		/// <returns></returns>
		public static byte GetBits ( byte pInput, int pStartIndex )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_BYTE, false );
		}

	
		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pShift">if set to <c>true</c> [p shift].</param>
		/// <returns></returns>
		public static byte GetBits ( byte pInput, int pStartIndex, bool pShift )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_BYTE, pShift);
		}


		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static byte GetBits ( byte pInput, int pStartIndex,  int pLength )
		{
			return GetBits ( pInput, pStartIndex, pLength, false);
		}

		
		/// <summary>
		/// Gets a number in the specified range of bits
		/// </summary>
		/// <param name="pStart"></param>
		/// <param name="pEnd"></param>
		/// <returns></returns>
		public static byte GetBits ( byte pInput, int pStartIndex, int pLength, bool pShift )
		{			
			int lRetval = 0,lSize = 0,lTemp = 0;
			int lPosition = 1;
			if ( pInput < 2 && pInput > 0 )
			{
				return pInput; //Should be either a 0 or 1
			}
			lSize = SizeOf(pInput);
			
			
			if ( pStartIndex < 1 || pStartIndex > BIT_SIZE_SHORT )
			{
				throw new ArgumentException("Start bit is out of range.","pStartIndex");
			}
			if ( pLength < 0 || pLength + pStartIndex > BIT_SIZE_BYTE + 1 )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			for ( int i = pStartIndex; (i < pLength + pStartIndex) && (lPosition <= lSize); i++ )
			{
				lTemp = 1 << i - 1;
				if ( (pInput & lTemp) == lTemp )
				{
					lRetval |= (1 << (lPosition - 1));
				}
				lPosition++;
			}
			if ( pShift && lPosition < lSize )
			{
				lRetval <<= lSize - lPosition;
			}
			return (byte) lRetval;
		}

		
		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <returns></returns>
		public static byte SetBits ( byte pDest, byte pSource, int pSourceIndex )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, BIT_SIZE_BYTE );
		}

		
		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static byte SetBits ( byte pDest, byte pSource, int pSourceIndex, int pLength )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, pLength );
		}


		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The dest.</param>
		/// <param name="pSource">The source.</param>
		/// <param name="pSourceIndex">Index of the source.</param>
		/// <param name="pDestIndex">Index of the dest.</param>
		/// <param name="pLength">Length to read.</param>
		/// <returns></returns>
		public static byte SetBits ( byte pDest, byte pSource, int pSourceIndex, 
			int pDestIndex, int pLength )
		{
			int lSourceSize = 0, lTemp1 = 0;
			if ( pSourceIndex < 1 || pSourceIndex > BIT_SIZE_BYTE )
			{
				throw new ArgumentException("Start bit is out of range.","pSourceIndex");
			}
			if ( pDestIndex < 0 || pDestIndex > BIT_SIZE_BYTE )
			{
				throw new ArgumentException("End bit is out of range.","pDestIndex");
			}
			if ( pLength < 0 || pLength + pDestIndex > BIT_SIZE_BYTE )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			pSource = GetBits(pSource,pSourceIndex,pLength);
			lSourceSize = SizeOf(pSource);

			int lPosition = 1;
			for ( int i = pDestIndex; (i < lSourceSize + pDestIndex); i++ )
			{
				lTemp1 = 1 << lPosition - 1;
				if ( (pSource & lTemp1) == lTemp1 )
				{
					pDest |= ((byte)(1 << (i - 1)));
				}
				else
				{
					lTemp1 = 1 << i - 1;
					if ( (pDest & lTemp1) == lTemp1 )
					{
						pDest ^= ((byte)(1 << (i - 1)));
					}
				}
				lPosition++;
			}
			return (byte) pDest;
		}

		
		/// <summary>
		/// Determines whether [is bit set] [the specified p input].
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <returns>
		/// 	<c>true</c> if [is bit set] [the specified p input]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsBitSet ( byte pInput, int pPosition)
		{
			return GetBits(pInput,pPosition,1,false) == 1;
		}

		/// <summary>
		/// Changes the value of the bit at the specified positon
		/// </summary>
		/// <param name="pInput"></param>
		/// <param name="pPosition"></param>
		/// <returns></returns>
		public static byte ChangeBit ( byte pInput, int pPosition )
		{
			if ( pPosition > BIT_SIZE_BYTE )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			return pInput ^= (byte)(1 << (pPosition - 1));
		}
		
		/// <summary>
		/// Sets the value of a bit
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <param name="pOn">if set to <c>true</c> [p on].</param>
		/// <returns></returns>
		public static byte SetBit ( byte pInput, int pPosition, bool pOn )
		{
			if ( pPosition > BIT_SIZE_BYTE )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			bool lIsSet = IsBitSet(pInput,pPosition);
			if ( pOn && !lIsSet || pOn && lIsSet)
			{
				pInput ^= (byte)((1 << (pPosition - 1)));
			}
			return pInput;
		}

		#endregion Byte Methods



		#region Short Methods

		/// <summary>
		/// Checks to see if number is less than 0.
		/// </summary>
		/// <param name="pInputValue"></param>
		/// <returns></returns>
		public static bool IsNegative ( short pInputValue )
		{
			return (pInputValue & 0x8000) == 0x8000;
		}

		/// <summary>
		/// Changes the value from positive to negative and vis versa
		/// </summary>
		/// <param name="pInputValue">The value</param>
		/// <returns></returns>
		public short ChangeSign ( short pInputValue )
		{
			return (short)(pInputValue ^ 0x8000);
		}

		/// <summary>
		/// Gets the size of the input value in bits
		/// </summary>
		/// <param name="pInput">The input value</param>
		/// <returns></returns>
		public static int SizeOf ( short pInput )
		{
			int iRetval = 0;
			if ( pInput == 0 )
			{
				iRetval = 0;
			}
			else if ( pInput == 1 )
			{
				iRetval = 1;
			}
			else if ( pInput < 0 )
			{
				iRetval = BIT_SIZE_SHORT;
			}
			else 
			{
				int lTemp = 0;
				for ( int i = BIT_SIZE_SHORT -1; i > 1; i-- )
				{
					lTemp = 1 << i-1;
					if ( (pInput & lTemp) == lTemp )
					{
						iRetval = i;
						break;
					}
				}
			}
			return iRetval;
		}


		/// <summary>
		/// Gets the bits from a number as a number.
		/// </summary>
		/// <param name="pInput">The input value.</param>
		/// <param name="pStart">The start position.</param>
		/// <returns></returns>
		public static short GetBits ( short pInput, int pStartIndex )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_SHORT, false );
		}

	
		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pShift">if set to <c>true</c> [p shift].</param>
		/// <returns></returns>
		public static short GetBits ( short pInput, int pStartIndex, bool pShift )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_SHORT, pShift);
		}


		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static short GetBits ( short pInput, int pStartIndex,  int pLength )
		{
			return GetBits ( pInput, pStartIndex, pLength, false);
		}

		
		/// <summary>
		/// Gets a number in the specified range of bits
		/// </summary>
		/// <param name="pStart"></param>
		/// <param name="pEnd"></param>
		/// <returns></returns>
		public static short GetBits ( short pInput, int pStartIndex, int pLength, bool pShift )
		{			
			int lRetval = 0,lSize = 0,lTemp = 0;
			int lPosition = 1;
			if ( pInput < 2 && pInput > 0 )
			{
				return pInput; //Should be either a 0 or 1
			}
			lSize = SizeOf(pInput);
			
			
			if ( pStartIndex < 1 || pStartIndex > BIT_SIZE_SHORT )
			{
				throw new ArgumentException("Start bit is out of range.","pStartIndex");
			}
			if ( pLength < 0 || pLength + pStartIndex > BIT_SIZE_SHORT + 1 )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			for ( int i = pStartIndex; (i < pLength + pStartIndex) && (lPosition <= lSize); i++ )
			{
				lTemp = 1 << i - 1;
				if ( (pInput & lTemp) == lTemp )
				{
					lRetval |= (1 << (lPosition - 1));
				}
				lPosition++;
			}
			if ( pShift && lPosition < lSize )
			{
				lRetval <<= lSize - lPosition;
			}
			return (short) lRetval;
		}

		
		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <returns></returns>
		public static short SetBits ( short pDest, short pSource, int pSourceIndex )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, BIT_SIZE_SHORT );
		}

		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static int SetBits ( short pDest, short pSource, int pSourceIndex, int pLength )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, pLength );
		}


		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The dest.</param>
		/// <param name="pSource">The source.</param>
		/// <param name="pSourceIndex">Index of the source.</param>
		/// <param name="pDestIndex">Index of the dest.</param>
		/// <param name="pLength">Length to read.</param>
		/// <returns></returns>
		public static short SetBits ( short pDest, short pSource, int pSourceIndex, 
			int pDestIndex, int pLength )
		{
			int lSourceSize = 0, lTemp1 = 0;
			if ( pSourceIndex < 1 || pSourceIndex > BIT_SIZE_SHORT )
			{
				throw new ArgumentException("Start bit is out of range.","pSourceIndex");
			}
			if ( pDestIndex < 0 || pDestIndex > BIT_SIZE_SHORT )
			{
				throw new ArgumentException("End bit is out of range.","pDestIndex");
			}
			if ( pLength < 0 || pLength + pDestIndex > BIT_SIZE_SHORT )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			pSource = GetBits(pSource,pSourceIndex,pLength);
			lSourceSize = SizeOf(pSource);

			int lPosition = 1;
			for ( int i = pDestIndex; (i < lSourceSize + pDestIndex); i++ )
			{
				lTemp1 = 1 << lPosition - 1;
				if ( (pSource & lTemp1) == lTemp1 )
				{
					pDest |= ((short)(1 << (i - 1)));
				}
				else
				{
					lTemp1 = 1 << i - 1;
					if ( (pDest & lTemp1) == lTemp1 )
					{
						pDest ^= ((short)(1 << (i - 1)));
					}
				}
				lPosition++;
			}
			return pDest;
		}

		
		/// <summary>
		/// Determines whether [is bit set] [the specified p input].
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <returns>
		/// 	<c>true</c> if [is bit set] [the specified p input]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsBitSet ( short pInput, int pPosition)
		{
			return GetBits(pInput,pPosition,1,false) == 1;
		}
		

		/// <summary>
		/// Changes the value of the bit at the specified positon
		/// </summary>
		/// <param name="pInput"></param>
		/// <param name="pPosition"></param>
		/// <returns></returns>
		public static short ChangeBit ( short pInput, int pPosition )
		{
			if ( pPosition > BIT_SIZE_SHORT )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			return pInput ^= (short)(1 << (pPosition - 1));
		}

		/// <summary>
		/// Sets the value of a bit
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <param name="pOn">if set to <c>true</c> [p on].</param>
		/// <returns></returns>
		public static short SetBit ( short pInput, int pPosition, bool pOn )
		{
			if ( pPosition > BIT_SIZE_SHORT )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			bool lIsSet = IsBitSet(pInput,pPosition);
			if ( pOn && !lIsSet || pOn && lIsSet)
			{
				pInput ^= (short)(1 << (pPosition - 1));
			}
			return pInput;
		}

		#endregion Short Methods



		#region Int Methods



		/// <summary>
		/// Checks to see if number is less than 0.
		/// </summary>
		/// <param name="pInputValue"></param>
		/// <returns></returns>
		public static bool IsNegative ( int pInputValue )
		{
			return (pInputValue & 0x80000000) == 0x80000000;
		}

		/// <summary>
		/// Changes the value from positive to negative and vis versa
		/// </summary>
		/// <param name="pInputValue">The value</param>
		/// <returns></returns>
		public int ChangeSign ( int pInputValue )
		{
			return (int) (pInputValue ^ 0x80000000);
		}

		/// <summary>
		/// Gets the size of the input value in bits
		/// </summary>
		/// <param name="pInput">The input value</param>
		/// <returns></returns>
		public static int SizeOf ( int pInput )
		{
			int iRetval = 0;
			if ( pInput == 0 )
			{
				iRetval = 0;
			}
			else if ( pInput == 1 )
			{
				iRetval = 1;
			}
			else if ( pInput < 0 )
			{
				iRetval = BIT_SIZE_INT;
			}
			else 
			{
				int lTemp = 0;
				for ( int i = BIT_SIZE_INT -1; i > 1; i-- )
				{
					lTemp = 1 << i-1;
					if ( (pInput & lTemp) == lTemp )
					{
						iRetval = i;
						break;
					}
				}
			}
			return iRetval;
		}


		/// <summary>
		/// Gets the bits from a number as a number.
		/// </summary>
		/// <param name="pInput">The input value.</param>
		/// <param name="pStart">The start position.</param>
		/// <returns></returns>
		public static int GetBits ( int pInput, int pStartIndex )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_INT, false );
		}

	
		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pShift">if set to <c>true</c> [p shift].</param>
		/// <returns></returns>
		public static int GetBits ( int pInput, int pStartIndex, bool pShift )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_INT, pShift);
		}


		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static int GetBits ( int pInput, int pStartIndex,  int pLength )
		{
			return GetBits ( pInput, pStartIndex, pLength, false);
		}

		
		/// <summary>
		/// Gets a number in the specified range of bits
		/// </summary>
		/// <param name="pStart"></param>
		/// <param name="pEnd"></param>
		/// <returns></returns>
		public static int GetBits ( int pInput, int pStartIndex, int pLength, bool pShift )
		{			
			int lRetval = 0,lSize = 0,lTemp = 0;
			int lPosition = 1;
			if ( pInput < 2 && pInput > 0 )
			{
				return pInput; //Should be either a 0 or 1
			}
			lSize = SizeOf(pInput);
			
			
			if ( pStartIndex < 1 || pStartIndex > BIT_SIZE_INT )
			{
				throw new ArgumentException("Start bit is out of range.","pStartIndex");
			}
			if ( pLength < 0 || pLength + pStartIndex > BIT_SIZE_INT + 1 )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			for ( int i = pStartIndex; (i < pLength + pStartIndex) && (lPosition <= lSize); i++ )
			{
				lTemp = 1 << i - 1;
				if ( (pInput & lTemp) == lTemp )
				{
					lRetval |= (1 << (lPosition - 1));
				}
				lPosition++;
			}
			if ( pShift && lPosition < lSize )
			{
				lRetval <<= lSize - lPosition;
			}
			return lRetval;
		}

		
		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <returns></returns>
		public static int SetBits ( int pDest, int pSource, int pSourceIndex )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, BIT_SIZE_INT );
		}

		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static int SetBits ( int pDest, int pSource, int pSourceIndex, int pLength )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, pLength );
		}


		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The dest.</param>
		/// <param name="pSource">The source.</param>
		/// <param name="pSourceIndex">Index of the source.</param>
		/// <param name="pDestIndex">Index of the dest.</param>
		/// <param name="pLength">Length to read.</param>
		/// <returns></returns>
		public static int SetBits ( int pDest, int pSource, int pSourceIndex, 
			int pDestIndex, int pLength )
		{
			int lSourceSize = 0, lTemp1 = 0;
			if ( pSourceIndex < 1 || pSourceIndex > BIT_SIZE_INT )
			{
				throw new ArgumentException("Start bit is out of range.","pSourceIndex");
			}
			if ( pDestIndex < 0 || pDestIndex > BIT_SIZE_INT )
			{
				throw new ArgumentException("End bit is out of range.","pDestIndex");
			}
			if ( pLength < 0 || pLength + pDestIndex > BIT_SIZE_INT )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			pSource = GetBits(pSource,pSourceIndex,pLength);
			lSourceSize = SizeOf(pSource);

			int lPosition = 1;
			for ( int i = pDestIndex; (i < lSourceSize + pDestIndex); i++ )
			{
				lTemp1 = 1 << lPosition - 1;
				if ( (pSource & lTemp1) == lTemp1 )
				{
					pDest |= (1 << (i - 1));
				}
				else
				{
					lTemp1 = 1 << i - 1;
					if ( (pDest & lTemp1) == lTemp1 )
					{
						pDest ^= (1 << (i - 1));
					}
				}
				lPosition++;
			}
			return pDest;
		}

		
		/// <summary>
		/// Determines whether [is bit set] [the specified p input].
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <returns>
		/// 	<c>true</c> if [is bit set] [the specified p input]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsBitSet ( int pInput, int pPosition)
		{
			return GetBits(pInput,pPosition,1,false) == 1;
		}

		/// <summary>
		/// Changes the value of the bit at the specified positon
		/// </summary>
		/// <param name="pInput"></param>
		/// <param name="pPosition"></param>
		/// <returns></returns>
		public static int ChangeBit ( int pInput, int pPosition )
		{
			if ( pPosition > BIT_SIZE_INT )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			return pInput ^= (1 << (pPosition - 1));
		}
		
		/// <summary>
		/// Sets the value of a bit
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <param name="pOn">if set to <c>true</c> [p on].</param>
		/// <returns></returns>
		public static int SetBit ( int pInput, int pPosition, bool pOn )
		{
			if ( pPosition > BIT_SIZE_INT )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			bool lIsSet = IsBitSet(pInput,pPosition);
			if ( pOn && !lIsSet || pOn && lIsSet)
			{
				pInput ^= (1 << (pPosition - 1));
			}
			return pInput;
		}

		#endregion Int Methods




		#region Long Methods

		/// <summary>
		/// Checks to see if number is less than 0.
		/// </summary>
		/// <param name="pInputValue"></param>
		/// <returns></returns>
		public static bool IsNegative ( long pInputValue )
		{
			return (((ulong)pInputValue) & 0x8000000000000000) == 0x8000000000000000;
		}

		/// <summary>
		/// Changes the value from positive to negative and vis versa
		/// </summary>
		/// <param name="pInputValue">The value</param>
		/// <returns></returns>
		public long ChangeSign ( long pInputValue )
		{
			return (long)(((ulong)pInputValue) ^ 0x8000000000000000);
		}

		/// <summary>
		/// Gets the size of the input value in bits
		/// </summary>
		/// <param name="pInput">The input value</param>
		/// <returns></returns>
		public static int SizeOf ( long pInput )
		{
			int iRetval = 0;
			if ( pInput == 0 )
			{
				iRetval = 0;
			}
			else if ( pInput == 1 )
			{
				iRetval = 1;
			}
			else if ( pInput < 0 )
			{
				iRetval = BIT_SIZE_LONG;
			}
			else 
			{
				long lTemp = 0;
				for ( int i = BIT_SIZE_LONG -1; i > 1; i-- )
				{
					lTemp = 1 << i-1;
					if ( (pInput & lTemp) == lTemp )
					{
						iRetval = i;
						break;
					}
				}
			}
			return iRetval;
		}


		/// <summary>
		/// Gets the bits from a number as a number.
		/// </summary>
		/// <param name="pInput">The input value.</param>
		/// <param name="pStart">The start position.</param>
		/// <returns></returns>
		public static long GetBits ( long pInput, int pStartIndex )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_LONG, false );
		}

	
		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pShift">if set to <c>true</c> [p shift].</param>
		/// <returns></returns>
		public static long GetBits ( long pInput, int pStartIndex, bool pShift )
		{
			return GetBits ( pInput, pStartIndex, BIT_SIZE_LONG, pShift);
		}


		/// <summary>
		/// Gets the bits.
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pStartIndex">Start index of the p.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static long GetBits ( long pInput, int pStartIndex,  int pLength )
		{
			return GetBits ( pInput, pStartIndex, pLength, false);
		}

		
		/// <summary>
		/// Gets a number in the specified range of bits
		/// </summary>
		/// <param name="pStart"></param>
		/// <param name="pEnd"></param>
		/// <returns></returns>
		public static long GetBits ( long pInput, int pStartIndex, int pLength, bool pShift )
		{			
			long lRetval = 0,lSize = 0,lTemp = 0;
			long lPosition = 1;
			if ( pInput < 2 && pInput > 0 )
			{
				return pInput; //Should be either a 0 or 1
			}
			lSize = SizeOf(pInput);
			
			
			if ( pStartIndex < 1 || pStartIndex > BIT_SIZE_LONG )
			{
				throw new ArgumentException("Start bit is out of range.","pStartIndex");
			}
			if ( pLength < 0 || pLength + pStartIndex > BIT_SIZE_LONG + 1 )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			for ( int i = pStartIndex; (i < pLength + pStartIndex) && (lPosition <= lSize); i++ )
			{
				lTemp = 1 << i - 1;
				if ( (pInput & lTemp) == lTemp )
				{
					lRetval |= (1 << ((int)(lPosition - 1)));
				}
				lPosition++;
			}
			if ( pShift && lPosition < lSize )
			{
				lRetval <<= ((int)(lSize - lPosition));
			}
			return lRetval;
		}

		
		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <returns></returns>
		public static long SetBits ( long pDest, long pSource, int pSourceIndex )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, BIT_SIZE_LONG );
		}

		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The p dest.</param>
		/// <param name="pSource">The p source.</param>
		/// <param name="pSourceIndex">Index of the p source.</param>
		/// <param name="pLength">Length of the p.</param>
		/// <returns></returns>
		public static long SetBits ( long pDest, long pSource, int pSourceIndex, int pLength )
		{
			return SetBits ( pDest, pSource, pSourceIndex, 0, pLength );
		}


		/// <summary>
		/// Sets the bits.
		/// </summary>
		/// <param name="pDest">The dest.</param>
		/// <param name="pSource">The source.</param>
		/// <param name="pSourceIndex">Index of the source.</param>
		/// <param name="pDestIndex">Index of the dest.</param>
		/// <param name="pLength">Length to read.</param>
		/// <returns></returns>
		public static long SetBits ( long pDest, long pSource, int pSourceIndex, 
			int pDestIndex, int pLength )
		{
			long lSourceSize = 0, lTemp1 = 0;
			if ( pSourceIndex < 1 || pSourceIndex > BIT_SIZE_LONG )
			{
				throw new ArgumentException("Start bit is out of range.","pSourceIndex");
			}
			if ( pDestIndex < 0 || pDestIndex > BIT_SIZE_LONG )
			{
				throw new ArgumentException("End bit is out of range.","pDestIndex");
			}
			if ( pLength < 0 || pLength + pDestIndex > BIT_SIZE_LONG )
			{
				throw new ArgumentException("End bit is out of range.","pLength");
			}
			pSource = GetBits(pSource,pSourceIndex,pLength);
			lSourceSize = SizeOf(pSource);

			int lPosition = 1;
			for ( int i = pDestIndex; (i < lSourceSize + pDestIndex); i++ )
			{
				lTemp1 = 1 << lPosition - 1;
				if ( (pSource & lTemp1) == lTemp1 )
				{
					pDest |= (1 << (i - 1));
				}
				else
				{
					lTemp1 = 1 << i - 1;
					if ( (pDest & lTemp1) == lTemp1 )
					{
						pDest ^= (1 << (i - 1));
					}
				}
				lPosition++;
			}
			return pDest;
		}

		
		/// <summary>
		/// Determines whether [is bit set] [the specified p input].
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <returns>
		/// 	<c>true</c> if [is bit set] [the specified p input]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsBitSet ( long pInput, int pPosition)
		{
			return GetBits(pInput,pPosition,1,false) == 1;
		}

		/// <summary>
		/// Changes the value of the bit at the specified positon
		/// </summary>
		/// <param name="pInput"></param>
		/// <param name="pPosition"></param>
		/// <returns></returns>
		public static long ChangeBit ( long pInput, int pPosition )
		{
			if ( pPosition > BIT_SIZE_LONG )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			return pInput ^= (1 << (pPosition - 1));
		}

		/// <summary>
		/// Sets the value of a bit
		/// </summary>
		/// <param name="pInput">The p input.</param>
		/// <param name="pPosition">The p position.</param>
		/// <param name="pOn">if set to <c>true</c> [p on].</param>
		/// <returns></returns>
		public static long SetBit ( long pInput, int pPosition, bool pOn )
		{
			if ( pPosition > BIT_SIZE_LONG )
			{
				throw new ArgumentException("Position out of range","pPosition");
			}
			bool lIsSet = IsBitSet(pInput,pPosition);
			if ( pOn && !lIsSet || pOn && lIsSet)
			{
				pInput ^= (1 << (pPosition - 1));
			}
			return pInput;
		}


		

		#endregion Long Methods



		/// <summary>
		/// The return value is the high-order double word of the specified value.
		/// </summary>
		/// <param name="pDWord"></param>
		/// <returns></returns>
		public static int HiDword ( long pDWord )
		{
			return ((int) (((pDWord) >> 32) & 0xFFFFFFFF)) ;
		}
		
		
		/// <summary>
		/// The return value is the low-order word of the specified value.
		/// </summary>
		/// <param name="pDWord">The value</param>
		/// <returns></returns>
		public static int LoDword(long pDWord)
		{
			return ((int)pDWord);
		}





		/// <summary>
		/// The return value is the high-order word of the specified value.
		/// </summary>
		/// <param name="pDWord"></param>
		/// <returns></returns>
		public static short HiWord ( int pDWord )
		{
			return ((short) (((pDWord) >> 16) & 0xFFFF)) ;
		}
		
		
		/// <summary>
		/// The return value is the low-order word of the specified value.
		/// </summary>
		/// <param name="pDWord">The value</param>
		/// <returns></returns>
		public static short LoWord(int pDWord)
		{
			return ((short)pDWord);
		}

		
		/// <summary>
		/// The return value is the high-order byte of the specified value.
		/// </summary>
		/// <param name="pWord">The value</param>
		/// <returns></returns>
		public static byte HiByte(short pWord)
		{
			return ((byte) (((short) (pWord) >> 8) & 0xFF));
		}

		/// <summary>
		/// The return value is the low-order byte of the specified value.
		/// </summary>
		/// <param name="pWord">The value</param>
		/// <returns></returns>
		public static byte LoByte(short pWord)
		{
			return ((byte)pWord);
		}

		/// <summary>
		/// Makes a 64 bit long from two 32 bit integers
		/// </summary>
		/// <param name="pValueLow">The low order value.</param>
		/// <param name="pValueHigh">The high order value.</param>
		/// <returns></returns>
		public static long MakeLong ( int pValueLow, int pValueHigh )
		{
			if ( pValueHigh == 0 )
			{
				return (long) pValueLow;
			}
			long lTemp = SizeOf(pValueHigh);
			lTemp = (pValueHigh << ((BIT_SIZE_LONG) - ((int)lTemp + 1)));
			return (long)(pValueLow | lTemp);
		}

		/// <summary>
		/// Makes a 32 bit integer from two 16 bit shorts
		/// </summary>
		/// <param name="pValueLow">The low order value.</param>
		/// <param name="pValueHigh">The high order value.</param>
		/// <returns></returns>
		public static int MakeDword ( short pValueLow, short pValueHigh )
		{
			if ( pValueHigh == 0 )
			{
				return (int) pValueLow;
			}
			int lTemp = SizeOf(pValueHigh);
			lTemp = pValueHigh << ((BIT_SIZE_INT) - (lTemp + 1));
			return (int)(lTemp | pValueLow);
		}


		/// <summary>
		/// Makes a 16 bit short from two bytes
		/// </summary>
		/// <param name="pValueLow">The low order value.</param>
		/// <param name="pValueHigh">The high order value.</param>
		/// <returns></returns>
		public static short MakeWord ( byte pValueLow, byte pValueHigh )
		{
			if ( pValueHigh == 0 )
			{
				return (short) pValueLow;
			}
			int lTemp = SizeOf(pValueHigh);
			lTemp = pValueHigh << ((BIT_SIZE_SHORT) - (lTemp + 1));
			return (short)(pValueLow | lTemp);
		}
	}
}
