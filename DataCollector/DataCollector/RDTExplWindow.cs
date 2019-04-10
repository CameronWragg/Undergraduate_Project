using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Services;
using EnvDTE;
using EnvDTE80;
using Task = System.Threading.Tasks.Task;

namespace DataCollector
{
    public class RDTExplWindow : IVsRunningDocTableEvents3
    {
        #region Members

        public static RDTExplWindow mRDTExplWindow;
        public static ErrorList errList;
        private static DTE mDte;
        private static DTE2 eDte;
        public static List<string> list = new List<string>();

        //public delegate void OnBeforeSaveHandler(object sender, Document document);
        //public event OnBeforeSaveHandler BeforeSave;

        public static uint rdtCookie;

        #endregion

        #region Constructor

        //public RDTExplWindow(Package aPackage)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    mDte = (DTE)Package.GetGlobalService(typeof(DTE));
        //    mRDTExplWindow = new RDTExplWindow(aPackage);
        //    mRDTExplWindow.Advise(this);
        //}

        public static async Task InitializeAsync(AsyncPackage aPackage)
        {
            // Switch to the main thread - the call to AddCommand in EnableDisableDataCollectorCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);
            mDte = (DTE)Package.GetGlobalService(typeof(DTE));
            eDte = (DTE2)Package.GetGlobalService(typeof(DTE));
            mRDTExplWindow = new RDTExplWindow();
            Instance = new RDTExplWindow();
            errList = eDte.ToolWindows.ErrorList;
            errList.ShowMessages = false;
            errList.ShowWarnings = false;
        }

        public static RDTExplWindow Instance 
        {
            get;
            private set;
        }

        #endregion

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            ErrorHandler.AddMessage("Before Save Handler Triggered");
            ErrorHandler.ClearList();
            list.Clear();
            string message = ""; //ErrorHandler.GetError();
            int i = errList.ErrorItems.Count;
            if (i != 0)
            {
                for (int j = 1; j <= i; j++)
                {
                    list.Add(errList.ErrorItems.Item(j).Description.ToString());
                    message += errList.ErrorItems.Item(j).Description.ToString();
                    string linePath = errList.ErrorItems.Item(j).FileName;
                    int errLine = errList.ErrorItems.Item(j).Line;
                    string line = File.ReadLines(linePath).Skip(errLine - 1).Take(1).First();
                    string encryptedLine = EncryptCode(line);
                    JsnToFile(errList.ErrorItems.Item(j).Description.ToString(), errList.ErrorItems.Item(j).ErrorLevel.ToString(), encryptedLine);
                }
            }
            ErrorHandler.AddMessage(message);
            return VSConstants.S_OK;
        }

        public static void JsnToFile(string description, string errorlevel, string line)
        {
            Random r = new Random();
            string rid = r.Next(1, 1025).ToString();
            StreamWriter fle = File.AppendText("database.json");
            fle.Write("\r\n{\"id\":\"" + $"{DateTime.Now.ToLongTimeString()}{rid}" + "\",\"description\":\"" + $"{description}" + "\",\"errorlevel\":\"" + $"{errorlevel}" + "\",\"line\":\"" + $"{line}" + "\"}");
            fle.Close();
        }

        public static string EncryptCode(string line)
        {
            bool fileExists = File.Exists("codenc.txt");
            bool encFileExists = File.Exists("codencF.txt");
            string plainTxt = "codenc.txt";
            string encTxt = "codencF.txt";
            if (fileExists == true)
            {
                File.Delete(plainTxt);
            }
            StreamWriter fle = File.CreateText(plainTxt);
            fle.Write(line);
            fle.Close();

            if (encFileExists == true)
            {
                File.Delete(encTxt);
            }

            FileStream fleStr = new FileStream(encTxt, FileMode.Create);
            FileInfo fleInf = new FileInfo(plainTxt);
            PgpEncKeys pgpEncKeys = new PgpEncKeys("recipient_pgp_public.txt", "pgp_private.txt", "unioflincoln");
            Encryption pgpEncryption = new Encryption(pgpEncKeys);
            pgpEncryption.EncryptSign(fleStr, fleInf);
            fleStr.Close();

            string rtn = File.ReadAllText(encTxt);
            //Commented out for testing purposes.
            //File.Delete(encTxt);
            //File.Delete(plainTxt);
            return rtn;
        }
    }
}
