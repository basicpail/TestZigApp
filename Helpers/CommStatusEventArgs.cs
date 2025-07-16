using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Helpers
{
    public enum CommStatusType
    {
        Success,
        Warning,
        Error,
        Info
    }

    public class CommStatusEventArgs : EventArgs
    {
        public CommStatusType StatusType { get; }
        public string Message { get; }

        public CommStatusEventArgs(CommStatusType statusType, string message)
        {
            StatusType = statusType;
            Message = message;
        }
    }

}
