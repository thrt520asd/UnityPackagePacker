using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Platform.Editor.Tool
{
  public class CMDHelper
  {
    public static void RunFileCmd(string scriptName, List<string> args = null, bool isShowWin = false, string workDir = null)
    {
      string arg = scriptName;
      if (args != null)
      {
        for (int j = 0; j < args.Count; j++)
        {
          arg = arg + " " + args[j];
        }
      }
      RunStringCmd(arg, isShowWin, workDir);
    }

    public static void RunStringCmd(string cmdContent, bool showWin = false, string workDir = null)
    {
      string exeName = "";
#if UNITY_EDITOR_WIN
      exeName = "cmd.exe";
#elif UNITY_EDITOR_OSX
        exeName = "sh";
#endif
      string arg = cmdContent;
      Process proc = new Process
      {
        StartInfo =
        {
          FileName = exeName,
          Arguments = "/C"+arg,
          CreateNoWindow = showWin,
          WindowStyle = ProcessWindowStyle.Normal,
          UseShellExecute = false,
          ErrorDialog =  true,
          RedirectStandardInput = false ,
          RedirectStandardOutput =  false,
          RedirectStandardError = true,
          WorkingDirectory =  workDir
        }
      };

      proc.Start();
      proc.WaitForExit();
      var sr = proc.StandardError;
      string str = sr.ReadToEnd();
      UnityEngine.Debug.Log(str);

      proc.Close();



    }


  }
}

