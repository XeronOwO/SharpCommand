using SharpCommand.Utils;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace SharpCommand
{
	/// <summary>
	/// SharpCommand Prompt
	/// </summary>
	public static class Prompt
	{
		#region Settings

		/// <summary>
		/// The start point of rendering
		/// </summary>
		private static Point _renderStartPoint;

		private static int _inputCheckDelay = 50;

		/// <summary>
		/// Gets or sets the delay between each input check.
		/// </summary>
		public static int InputCheckDelay { get { return _inputCheckDelay; } set { _inputCheckDelay = value; } }

		private static bool _isColorCharEnabled = false;

		/// <summary>
		/// Gets or sets enable color char '§'.
		/// </summary>
		public static bool IsColorCharEnabled { get { return _isColorCharEnabled; } set { _isColorCharEnabled = value; } }

		private static char _colorChar = '§';

		/// <summary>
		/// The color char. '§' by default.<br/>
		/// <br/>
		/// <strong>Usage:</strong><br/>
		/// <code>Prompt.Write("§xy")</code>
		/// 'x' represents the color of the Terminal’s background, whereas,<br/>
		/// 'y' represents the color of the font on the Command Prompt Terminal.<br/>
		/// <br/>
		/// <strong>Color codes:</strong><br/>
		/// 0 = Black | 8 = DarkGray<br/>
		/// 1 = DarkBlue | 9 = Blue<br/>
		/// 2 = DarkGreen | A = Green<br/>
		/// 3 = DarkCyan | B = Cyan<br/>
		/// 4 = DarkRed | C = Red<br/>
		/// 5 = DarkMagenta | D = Magenta<br/>
		/// 6 = DarkYellow | E = Yellow<br/>
		/// 7 = Gray | F = White<br/>
		/// R = Reset | Others = Don't change (e.g. ' ' for not changing)
		/// </summary>
		public static char ColorChar { get { return _colorChar; } set { _colorChar = value; } }

		#endregion

		#region Basic

		private static bool _isRunning = false;

		/// <summary>
		/// Start prompt.
		/// </summary>
		public static void Start()
		{
			if (_isRunning)
			{
				return;
			}
			_isRunning = true;

			_renderStartPoint = new(Console.CursorLeft, Console.CursorTop);
			_needsReRenderInputPrefix = true;
			_needsReRenderInputContent = true;

			ReRender();

			_inputTask = Input.StartInputTask();
		}

		private static Task _inputTask;

		/// <summary>
		/// Wait until the prompt stopped.
		/// </summary>
		/// <returns></returns>
		public static void WaitUntilStopped()
		{
			if (!_isRunning)
			{
				return;
			}

			WaitUntilStoppedAsync().Wait();
		}

		/// <summary>
		/// Wait until the prompt stopped asynchronously.
		/// </summary>
		/// <returns></returns>
		public static async Task WaitUntilStoppedAsync()
		{
			if (!_isRunning)
			{
				return;
			}

			await _inputTask;
		}

		/// <summary>
		/// Stop prompt.
		/// </summary>
		public static void Stop()
		{
			if (!_isRunning)
			{
				return;
			}

			// cancel input task
			Input.Cancel();

			// clear input prefix and content
			_inputPrefix = string.Empty;
			_needsReRenderInputPrefix = true;
			_inputContent = string.Empty;
			_needsReRenderInputContent = true;
			ReRender();

			_isRunning = false;
		}

		#endregion

		#region Output

		private static object _locker = new object();

		/// <summary>
		/// Pre-Print
		/// </summary>
		private static void PrePrint()
		{
			// hide cursor when rendering
			Console.CursorVisible = false;

			Output.ResetColor();

			Console.SetCursorPosition(_renderStartPoint.X, _renderStartPoint.Y);
		}

		/// <summary>
		/// Post-Print
		/// </summary>
		private static void PostPrint()
		{
			_renderStartPoint.X = Console.CursorLeft;
			_renderStartPoint.Y = Console.CursorTop;
			_needsReRenderInputPrefix = true;
			_needsReRenderInputContent = true;

			ReRender();
		}

		/// <summary>
		/// Write a char value.
		/// </summary>
		/// <param name="value">char value</param>
		public static void Write(char value)
		{
			lock (_locker)
			{
				PrePrint();

				var colorStep = 0;
				Output.Print(value, _isColorCharEnabled, _colorChar, ref colorStep);

				PostPrint();
			}
		}

		/// <summary>
		/// Write a string value.
		/// </summary>
		/// <param name="value">string value</param>
		public static void Write(string value)
		{
			lock (_locker)
			{
				PrePrint();

				var colorStep = 0;
				foreach (var ch in value)
				{
					Output.Print(ch, _isColorCharEnabled, _colorChar, ref colorStep);
				}

				PostPrint();
			}
		}

		/// <summary>
		/// WriteLine a char value.
		/// </summary>
		/// <param name="value">char value</param>
		public static void WriteLine(char value)
		{
			lock (_locker)
			{
				PrePrint();

				var colorStep = 0;
				Output.Print(value, _isColorCharEnabled, _colorChar, ref colorStep);
				Output.Print('\n', _isColorCharEnabled, _colorChar, ref colorStep);

				PostPrint();
			}
		}

		/// <summary>
		/// WriteLine a string value.
		/// </summary>
		/// <param name="value">string value</param>
		public static void WriteLine(string value)
		{
			lock (_locker)
			{
				PrePrint();

				var colorStep = 0;
				Output.Print(value, _isColorCharEnabled, _colorChar, ref colorStep);
				Output.Print('\n', _isColorCharEnabled, _colorChar, ref colorStep);

				PostPrint();
			}
		}

		#endregion

		#region Input

		private static string _inputContent = string.Empty;

		/// <summary>
		/// User input content
		/// </summary>
		public static string InputContent
		{
			get
			{
				return _inputContent;
			}
			set
			{
				_inputContent = value;
				_needsReRenderInputContent = true;
				ReRender();
			}
		}

		/// <summary>
		/// OnBeep callback.
		/// </summary>
		public delegate void OnBeepCallback();

		/// <summary>
		/// When prompt receives invalid operation.<br/>
		/// eg. User presses Backspace when InputContent is already empty.
		/// </summary>
		public static event OnBeepCallback OnBeep;

		/// <summary>
		/// OnInput callback.
		/// </summary>
		/// <param name="input">user input</param>
		public delegate void OnInputCallback(string input);

		/// <summary>
		/// When the user inputs something.
		/// </summary>
		public static event OnInputCallback OnInput;

		/// <summary>
		/// OnKey callback.
		/// </summary>
		/// <param name="key">key value</param>
		/// <returns>Return true if you don't want the key to be processed by Prompt, otherwise false.</returns>
		public delegate bool OnKeyCallback(ConsoleKeyInfo key);

		/// <summary>
		/// When the user pressed a key.
		/// </summary>
		public static event OnKeyCallback OnKey;

		/// <summary>
		/// When user press a key.
		/// </summary>
		/// <param name="key">console key info</param>
		internal static void OnConsoleKey(ConsoleKeyInfo key)
		{
			var result = OnKey?.Invoke(key);
			if (result.HasValue && result.Value)
			{
				return;
			}

			if (key.Key == ConsoleKey.Enter) // Enter
			{
				var inputContent = _inputContent;

				// clear input
				_inputContent = string.Empty;

				_needsReRenderInputContent = true;
				ReRender();

				// OnCommand event
				OnInput?.Invoke(inputContent);
			}
			else if (key.Key == ConsoleKey.Backspace) // Backspace
			{
				if (_inputContent.Length > 0)
				{
					// remove the last char
					_inputContent = _inputContent.Remove(_inputContent.Length - 1);

					RenderBackspace(_inputContent);
				}
				else
				{
					// nothing to backspace
					OnBeep?.Invoke();
				}
			}
			else if (key.Key == ConsoleKey.Escape) // Escape
			{
				// clear input
				_inputContent = string.Empty;

				_needsReRenderInputContent = true;
				ReRender();
			}
			else // normal input
			{
				_inputContent += key.KeyChar;

				RenderAppend(key.KeyChar);
			}
		}

		#endregion

		#region Render

		private static string _inputPrefix = string.Empty;

		/// <summary>
		/// Gets or sets the value displayed before InputContent.
		/// </summary>
		public static string InputPrefix
		{
			get
			{
				return _inputPrefix;
			}
			set
			{
				_inputPrefix = value;
				_needsReRenderInputPrefix = true;
				ReRender();
			}
		}

		/// <summary>
		/// If the rendering went wrong, call this to re-render all contents.
		/// </summary>
		public static void ForceReRender()
		{
			_needsReRenderInputPrefix = true;
			_needsReRenderInputContent = true;
			ReRender();
		}

		/// <summary>
		/// Render all contents.
		/// </summary>
		private static void ReRender()
		{
			if (!_isRunning)
			{
				return;
			}

			lock (_locker)
			{
				// hide cursor when rendering
				Console.CursorVisible = false;

				// reset color
				Output.ResetColor();

				// re-render InputPrefix
				if (_needsReRenderInputPrefix)
				{
					_needsReRenderInputPrefix = false;
					ReRenderInputPrefix();
				}

				// re-render InputContent
				if (_needsReRenderInputContent)
				{
					_needsReRenderInputContent = false;
					ReRenderInputContent();
				}

				// show cursor after rendering
				Console.CursorVisible = true;
			}
		}

		#region Input Prefix

		private static bool _needsReRenderInputPrefix;

		/// <summary>
		/// Re-render InputPrefix
		/// </summary>
		private static void ReRenderInputPrefix()
		{
			Console.SetCursorPosition(_renderStartPoint.X, _renderStartPoint.Y);

			var colorStep = 0;
			Output.Print(_inputPrefix, _isColorCharEnabled, _colorChar, ref colorStep);

			_inputContentRenderStartPoint.X = Console.CursorLeft;
			_inputContentRenderStartPoint.Y = Console.CursorTop;

			_needsReRenderInputContent = true;
		}

		#endregion

		#region Input Content

		private static bool _needsReRenderInputContent;

		/// <summary>
		/// The start point of rendering input content
		/// </summary>
		private static Point _inputContentRenderStartPoint = new();

		/// <summary>
		/// Re-render InputContent
		/// </summary>
		private static void ReRenderInputContent()
		{
			Console.SetCursorPosition(_inputContentRenderStartPoint.X, _inputContentRenderStartPoint.Y);

			var colorStep = 0;
			Output.Print(_inputContent, false, _colorChar, ref colorStep);

			var renderEndPoint = new Point(Console.CursorLeft, Console.CursorTop);
			RenderSpaceTillEndPoint();
			_renderEndPoint = renderEndPoint;

			Console.SetCursorPosition(renderEndPoint.X, renderEndPoint.Y);
		}

		private static int _inputColorStep = 0;

		/// <summary>
		/// Render when the user appends a char to input.
		/// </summary>
		/// <param name="ch">char value</param>
		private static void RenderAppend(char ch)
		{
			lock (_locker)
			{
				Output.Print(ch, false, _colorChar, ref _inputColorStep);

				_renderEndPoint = new Point(Console.CursorLeft, Console.CursorTop);
			}
		}

		/// <summary>
		/// Render when the user presses backspace.
		/// </summary>
		/// <param name="str">input value</param>
		private static void RenderBackspace(string str)
		{
			lock (_locker)
			{
				// hide cursor when rendering
				Console.CursorVisible = false;

				var renderEndPoint = CharHelper.MovePoint(_inputContentRenderStartPoint, str);
				Console.SetCursorPosition(renderEndPoint.X, renderEndPoint.Y);

				RenderSpaceTillEndPoint();

				_renderEndPoint = renderEndPoint;
				Console.SetCursorPosition(renderEndPoint.X, renderEndPoint.Y);

				// show cursor after rendering
				Console.CursorVisible = true;
			}
		}

		#endregion

		/// <summary>
		/// The end point of rendering
		/// </summary>
		private static Point _renderEndPoint = new();

		private static void RenderSpaceTillEndPoint()
		{
			var colorStep = 0;
			while (!(Console.CursorLeft >= _renderEndPoint.X && Console.CursorTop >= _renderEndPoint.Y))
			{
				Output.Print(' ', _isColorCharEnabled, _colorChar, ref colorStep);
			}
		}

		#endregion
	}
}
