using SharpCommand.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpCommand
{
	internal static class Output
	{
		#region Print

		public static void Print(char c, bool enableColor, char colorChar, ref int colorStep)
		{
			InitColor();

			// render color
			if (enableColor)
			{
				if (colorStep > 0)
				{
					if (colorStep == 1)
					{
						SetBackgroundColor(c);
						++colorStep;
					}
					else
					{
						SetForegroundColor(c);
						colorStep = 0;
					}
					return;
				}
				else
				{
					if (c == colorChar)
					{
						colorStep = 1;
						return;
					}
				}
			}

			// render normally
			bool needsReturn;

			if (c == '\n')
			{
				var count = Console.BufferWidth - Console.CursorLeft;
				for (int i = 0; i < count; i++)
				{
					Console.Out.Write(' ');
				}

				needsReturn = true;
			}
			else if (c == '\r')
			{
				needsReturn = false;
			}
			else
			{
				needsReturn = Console.CursorLeft == Console.BufferWidth - CharHelper.GetCharDisplayLength(c);

				Console.Out.Write(c);
			}

			if (needsReturn)
			{
				if (Console.CursorTop + 1 >= Console.BufferHeight)
				{
					++Console.BufferHeight;
				}
				Console.SetCursorPosition(0, Console.CursorTop + 1);
			}
		}

		public static void Print(string str, bool enableColor, char colorChar, ref int colorStep)
		{
			if (str == null)
			{
				return;
			}

			if (enableColor)
			{
				foreach (var ch in str)
				{
					Print(ch, enableColor, colorChar, ref colorStep);
				}
			}
			else
			{
				Console.Out.Write(str);
			}
		}

		#endregion

		#region Color

		private static bool _isColorInitialized = false;

		private static ConsoleColor _foregroundColor;

		private static ConsoleColor _backgroundColor;

		private static void InitColor()
		{
			if (_isColorInitialized)
			{
				return;
			}
			_isColorInitialized = true;

			_foregroundColor = Console.ForegroundColor;
			_backgroundColor = Console.BackgroundColor;
		}

		public static void ResetColor()
		{
			InitColor();

			Console.ForegroundColor = _foregroundColor;
			Console.BackgroundColor = _backgroundColor;
		}

		private static void SetForegroundColor(char c)
		{
			switch (c)
			{
				case '0':
					Console.ForegroundColor = ConsoleColor.Black;
					break;
				case '1':
					Console.ForegroundColor = ConsoleColor.DarkBlue;
					break;
				case '2':
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					break;
				case '3':
					Console.ForegroundColor = ConsoleColor.DarkCyan;
					break;
				case '4':
					Console.ForegroundColor = ConsoleColor.DarkRed;
					break;
				case '5':
					Console.ForegroundColor = ConsoleColor.DarkMagenta;
					break;
				case '6':
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					break;
				case '7':
					Console.ForegroundColor = ConsoleColor.Gray;
					break;
				case '8':
					Console.ForegroundColor = ConsoleColor.DarkGray;
					break;
				case '9':
					Console.ForegroundColor = ConsoleColor.Blue;
					break;
				case 'a':
				case 'A':
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case 'b':
				case 'B':
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
				case 'c':
				case 'C':
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case 'd':
				case 'D':
					Console.ForegroundColor = ConsoleColor.Magenta;
					break;
				case 'e':
				case 'E':
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case 'f':
				case 'F':
					Console.ForegroundColor = ConsoleColor.White;
					break;
				case 'r':
				case 'R':
					Console.ForegroundColor = _foregroundColor;
					break;
				default:
					break;
			}
		}

		private static void SetBackgroundColor(char c)
		{
			switch (c)
			{
				case '0':
					Console.BackgroundColor = ConsoleColor.Black;
					break;
				case '1':
					Console.BackgroundColor = ConsoleColor.DarkBlue;
					break;
				case '2':
					Console.BackgroundColor = ConsoleColor.DarkGreen;
					break;
				case '3':
					Console.BackgroundColor = ConsoleColor.DarkCyan;
					break;
				case '4':
					Console.BackgroundColor = ConsoleColor.DarkRed;
					break;
				case '5':
					Console.BackgroundColor = ConsoleColor.DarkMagenta;
					break;
				case '6':
					Console.BackgroundColor = ConsoleColor.DarkYellow;
					break;
				case '7':
					Console.BackgroundColor = ConsoleColor.Gray;
					break;
				case '8':
					Console.BackgroundColor = ConsoleColor.DarkGray;
					break;
				case '9':
					Console.BackgroundColor = ConsoleColor.Blue;
					break;
				case 'a':
				case 'A':
					Console.BackgroundColor = ConsoleColor.Green;
					break;
				case 'b':
				case 'B':
					Console.BackgroundColor = ConsoleColor.Cyan;
					break;
				case 'c':
				case 'C':
					Console.BackgroundColor = ConsoleColor.Red;
					break;
				case 'd':
				case 'D':
					Console.BackgroundColor = ConsoleColor.Magenta;
					break;
				case 'e':
				case 'E':
					Console.BackgroundColor = ConsoleColor.Yellow;
					break;
				case 'f':
				case 'F':
					Console.BackgroundColor = ConsoleColor.White;
					break;
				case 'r':
				case 'R':
					Console.BackgroundColor = _backgroundColor;
					break;
				default:
					break;
			}
		}

		#endregion
	}
}
