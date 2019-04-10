using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace DataCollector
{
    // Class for enabling the extension to send Errors/Warnings/Messages to the Error List.
    internal static class ErrorHandler
    {
        private static ErrorListProvider _errorListProvider;
        //private static StringCollection tl;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _errorListProvider = new ErrorListProvider(serviceProvider);
        }

        public static void AddTask(string message, TaskErrorCategory category)
        {
            _errorListProvider.Tasks.Add(new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = category,
                Text = message
            });
        }

        public static void AddError(string message)
        {
            AddTask(message, TaskErrorCategory.Error);
        }

        public static void AddWarning(string message)
        {
            AddTask(message, TaskErrorCategory.Warning);
        }

        public static void AddMessage(string message)
        {
            AddTask(message, TaskErrorCategory.Message);
        }

        public static string GetError()
        {
            string rtn = "";
            //tl = _errorListProvider.Subcategories;
            //int tlsz = tl.Count;
            //string[] strarray = new string[tlsz];
            //tl.CopyTo(strarray, 0);
            //for (int i = 0; i < strarray.Length; i++)
            //{
            //    rtn += strarray[i];
            //}
            return rtn;
        }

        public static void ClearList()
        {
            _errorListProvider.Tasks.Clear();
        }
    }
}
