using System;
using System.Diagnostics;

namespace TVU.SharedLib.GenericUtility.Consuming
{
    public static class TimeConsumProxy
    {
        #region AssemblyMethod  

        public static bool TimeConsumingShell(Func<bool> action, string methodName, bool isCalcTimeElapsed, ILogPrinter logPrinter = null, Action<bool, Exception> callback = null)
        {
            if (action == null)
                return false;

            Exception exp = null;
            bool result = false;

            try
            {
                if (isCalcTimeElapsed)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    result = action();
                    sw.Stop();
                    logPrinter?.PrintMsg($"Time-Consuming, {methodName}, {sw.ElapsedMilliseconds}ms");
                }
                else
                {
                    result = action();
                }
            }
            catch (Exception ex)
            {
                exp = ex;
            }
            finally
            {
                callback?.Invoke(result, exp);
            }

            return result;
        }

        #endregion
    }
}
