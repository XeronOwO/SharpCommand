using System.Media;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpCommand.Example
{
	internal class Program
	{
		static void Main(string[] args)
		{
			//Console.ReadKey(true);

			// set color
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;

			// start example
			PromptExample();

			// example terminated
			Console.WriteLine("Prompt terminated.");
		}

		private static CancellationTokenSource _source = new();

		private static void PromptExample()
		{
			// properties
			Prompt.InputCheckDelay = 50;

			// register events
			Prompt.OnBeep += OnBeep;
			Prompt.OnInput += OnInput;
			Prompt.OnKey += OnKey;

			// start prompt
			Prompt.Start();

			// set input prefix
			Prompt.InputPrefix = "\n> ";

			// print a sentence
			Prompt.WriteLine("This is a test string.");

			// enable color char to print colorful sentences
			Prompt.IsColorCharEnabled = true;
			// you can change color char here
			Prompt.ColorChar = '§';
			// print a colorful sentence
			Prompt.WriteLine("§8EThis is a §-Acolorful §-Bstring§-E.§RR");

			// print help
			Prompt.WriteLine("Type \"help\" for help messages.");

			// start a timer task
			Task.Factory.StartNew(Timer, _source.Token);

			// wait for termination
			Prompt.WaitUntilStopped();
		}

		private static void Timer()
		{
			for (int i = 0; i < 11 ; i++)
			{
				Prompt.WriteLine($"Timer: {i}s");
				Task.Delay(1000).Wait();
			}
			Prompt.WriteLine($"Timer terminated.");
		}

		/// <inheritdoc cref="Prompt.OnBeepCallback"/>
		private static void OnBeep()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Windows only
				SystemSounds.Beep.Play();
			}
		}

		/// <inheritdoc cref="Prompt.OnInputCallback"/>
		private static void OnInput(string input)
		{
			// do your specialization
			switch (input)
			{
				case "exit": // when enters "exit"
					// stop timer
					_source.Cancel();
					// stop prompt
					Prompt.Stop();
					break;

				case "test":
					// print "Hello, World!" when enters "test"
					Prompt.WriteLine("Hello, World!");
					break;

				case "help":
					// print help messages when enters "help"
					Prompt.WriteLine(
@"§RBHelp:
exit	- exit prompt
test	- print hello world
help	- display help

§REMore: Try input ""/g"" and press TAB§RR");
					break;

				default:
					// write what the user enters
					Prompt.WriteLine(input);
					break;
			}

			// reset input prefix from command hint
			Prompt.InputPrefix = "\n> ";
		}

		private static string[] _commands = new string[]
		{
			"/gamemode",
			"/gameruld",
			"/give",
		};

		/// <inheritdoc cref="Prompt.OnKeyCallback"/>
		private static bool OnKey(ConsoleKeyInfo key)
		{
			if (key.Key == ConsoleKey.Z) // don't want the user to enter 'z'
			{
				Prompt.WriteLine("\"OnKey\" doesn't want you to enter 'Z' here.");
				return true;
			}
			else if (key.Key == ConsoleKey.F5) // Press F5 to fresh
			{
				Prompt.ForceReRender();
				return true;
			}
			else if (key.KeyChar == '\t') // tab hint
			{
				// here uses command hint in Minecraft as example

				// input prefix
				var inputPrefix = new StringBuilder();

				var inputContent = Prompt.InputContent;
				if (inputContent.StartsWith('/')) // command start symbol
				{
					// filter matching commands
					var commands = _commands.Where(c => c.StartsWith(inputContent)).ToArray();
					if (commands.Length == 0)
					{
						// no command available
						OnBeep();
					}
					else
					{
						inputPrefix.AppendLine();
						for (int i = 0; i < commands.Length; i++)
						{
							// append command to command hint
							if (i > 0)
							{
								inputPrefix.Append(' ');
							}
							inputPrefix.Append("§70");
							inputPrefix.Append(commands[i]);
							inputPrefix.Append("§RR");
						}
					}
				}

				inputPrefix.Append("\n> ");

				// set input prefix finally
				Prompt.InputPrefix = inputPrefix.ToString();

				return true;
			}

			// normally return false
			return false;
		}
	}
}