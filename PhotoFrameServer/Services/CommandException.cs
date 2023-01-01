using System;

namespace PhotoFrameServer.Services
{
	public class CommandException : Exception
	{
		public CommandException(string message) : base(message)
		{
		}
	}
}

