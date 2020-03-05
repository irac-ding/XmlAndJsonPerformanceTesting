using System;
using Microsoft.Win32.TaskScheduler;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public static class TaskSchedulerUtility
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static bool IsTaskSchedulerExist(string taskName)
        {
            using (TaskService ts = new TaskService())
            {
                Task fnd = ts.FindTask(taskName, true);
                if (fnd == null)
                {
                    logger.Warn("IsTaskSchedulerExist: task {0} not exist.", taskName);
                    return false;
                }
                return true;
            }
        }

        public static void CreatLogonTrigerTaskScheduler(string taskName, string path, string arguments, string workingDirectory)
        {
            CreatLogonTrigerTaskScheduler(taskName, path, arguments, workingDirectory, Environment.UserName, new TimeSpan(0, 0, 1, 0));
        }

        public static void CreatLogonTrigerTaskScheduler(string taskName, string path, string arguments, string workingDirectory, TimeSpan timeSpan)
        {
            CreatLogonTrigerTaskScheduler(taskName, path, arguments, workingDirectory, Environment.UserName, timeSpan);
        }

        public static void CreatLogonTrigerTaskScheduler(string taskName, string path, string arguments, string workingDirectory, string userId)
        {
            CreatLogonTrigerTaskScheduler(taskName, path, arguments, workingDirectory, userId, new TimeSpan(0, 0, 1, 0));
        }

        private static void CreatLogonTrigerTaskScheduler(string taskName, string path, string arguments, string workingDirectory, string userId, TimeSpan timeSpan)
        {
            CreatLogonTrigerTaskScheduler(taskName, new ExecAction(path, arguments, workingDirectory), userId, timeSpan);
        }

        private static void CreatLogonTrigerTaskScheduler(string taskName, ExecAction execAction, string userId, TimeSpan timeSpan)
        {
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();

                //Running tasks with Administrator rights that ignore UAC warnings
                td.Principal.RunLevel = TaskRunLevel.Highest;

                td.RegistrationInfo.Description = string.Format("Start up program: {0} after user: {1} logon, delay: {2} mins", execAction.Path, userId, timeSpan.TotalMinutes);

                // Create a trigger that will fire the task at this time every other day
                td.Triggers.Add(new LogonTrigger() { UserId = userId, Delay = timeSpan });

                // Create an action that will launch Notepad whenever the trigger fires
                td.Actions.Add(execAction);

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition(taskName, td);

                logger.Warn("CreatLogonTrigerTaskScheduler: task {0} added to schedule", taskName);
            }
        }

        public static void DeleteLogonTrigerTaskScheduler(string taskName)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    ts.RootFolder.DeleteTask(taskName);
                    logger.Warn("DeleteLogonTrigerTaskScheduler: task {0} deleted from schedule.", taskName);
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteLogonTrigerTaskScheduler: error {0}", ex.Message);
            }
        }
    }
}
