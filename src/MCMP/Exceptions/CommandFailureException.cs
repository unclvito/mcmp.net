using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCMP.Exceptions
{
	public class CommandFailureException : ApplicationException
	{
		public CommandFailureException()
		{
		}

		internal CommandFailureException(string message, MCMP.Responses.McmpResponse mcmpResponse)
			: base(message)
		{
			McmpResponse = mcmpResponse;
		}

		internal CommandFailureException(string message, MCMP.Responses.McmpResponse mcmpResponse, Exception innerException)
			: base(message, innerException)
		{
			McmpResponse = mcmpResponse;
		}

		internal CommandFailureException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		internal CommandFailureException(MCMP.Responses.McmpResponse mcmpResponse)
			: base(mcmpResponse.ErrorMessage)
		{
			McmpResponse = mcmpResponse;
		}

		internal CommandFailureException(MCMP.Responses.McmpResponse mcmpResponse, Exception innerException)
			: base(mcmpResponse.ErrorMessage, innerException)
		{
			McmpResponse = mcmpResponse;
		}

		internal MCMP.Responses.McmpResponse McmpResponse { get; set; }
	}
}
