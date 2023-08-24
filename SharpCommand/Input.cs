using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCommand
{
	/// <summary>
	/// Prompt input
	/// </summary>
	internal static class Input
	{
		#region Input Thread

		public static Task StartInputTask()
		{
			_cancellationTokenSource = new();
			return Task.Factory.StartNew(new Action(InputThread));
		}

		private static CancellationTokenSource _cancellationTokenSource;

		public static void StopInputTask()
		{
			_cancellationTokenSource.Cancel();
		}

		public static void InputThread()
		{
			try
			{
				// Input loop
				var delayTask = Task.Delay(Prompt.InputCheckDelay, _cancellationTokenSource.Token);
				while (true)
				{
					// Check input available
					while (!Console.KeyAvailable && !_cancellationTokenSource.IsCancellationRequested)
					{
						delayTask.Wait(_cancellationTokenSource.Token);
					}

					if (_cancellationTokenSource.IsCancellationRequested)
					{
						break;
					}

					var key = Console.ReadKey(true);
					Prompt.OnConsoleKey(key);
				}
			}
			catch (Exception)
			{

			}
			finally
			{

			}
		}

		public static void Cancel()
		{
			_cancellationTokenSource.Cancel();
		}

		#endregion
	}
}
