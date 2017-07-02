using System;
using System.Threading.Tasks;
using System.IO;

namespace ParallelTest1_ForEach
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get a list of all the bat files in the current directory, so we can execute them later
            String[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bat");

            // Set the maximum parallel executions via an argument. The other option would be to hardcode it, which I am not a fan of. 
            // The third option is to do (1) "Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 1.0))", making use of 75% of the total processer usage. 
            // Currently this does not work. 
            string str = null;
            if (args[0] == null)
            {
                args[0] = "3";
                str = args[0];
            }
            else
            {
                str = args[0];
            }
            
            int maxParallelExecutions = Convert.ToInt32(str); // I am currently unable to take an int directly from the argument list, hence the conversion here
            // Crashes if no argument is given


            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            // remove the next 2 lines to show the execution of processes. 
            // They start in focus, interrupting your work. 
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;

            proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory(); // It's easier than having to specify where this program will be run.

            // The magick is in the foreach statement: 
            // For each of the files in the list, execute the 'current file' in effect the next in the list.
            // While no more than maxParallelExecutions is running at the same time

            Parallel.ForEach(
                files, 
                new ParallelOptions { MaxDegreeOfParallelism = maxParallelExecutions }, // use the line from (1) .
                currentFile =>
            {
                String filename = Path.GetFileName(currentFile);

                proc.StartInfo.FileName = currentFile; // Set the currentfile as the one being executed. Incrementing automatically.


                try
                {
                    proc.Start();
                Console.WriteLine("started " + currentFile);

                // if (proc.waitforexit throws exception, ignore and delete file)

                
                    proc.WaitForExit();
                    //proc.WaitForExit(2147483647);  // Another key point: As we are executing a external batch file, this program thinks that it can start as many bat files as it wants,
                                         // leading to excessive cpu and ram usage. 
                }
                catch (AggregateException) // An exception is thrown when the bat file gives an error.
                // The underlaying problem is that I generate the bat files by going from a 'start number' to 'start number + n' without checking if 'start number + n' is a valid filename
                // I let imagemagick handle that error (by giving a console file not found error), and just move on.
                
                {
                    File.Delete(filename); // This is the "The only tool I have is a hammer" approach. I am sure there are smarter and better ways to catch the exceptions
                    // but for the purposes of this program, this is fine. 
                    // Since the bat files are single use and can be remade, it does not matter that they are deleted. The files in here are not worth spending time on anyway. 
                }
                catch (UnauthorizedAccessException)
                {
                    File.Delete(filename);
                    // test
                }
                catch (InvalidOperationException)
                {
                    File.Delete(filename);
                }
                // First level error given is :
                /*
   Unhandled Exception: System.AggregateException: One or more errors occurred. ---> System.InvalidOperationException: No process is associated with this object.
   at System.Diagnostics.Process.EnsureState(State state)
   at System.Diagnostics.Process.EnsureState(State state)
   at System.Diagnostics.Process.GetProcessHandle(Int32 access, Boolean throwIfExited)
   at System.Diagnostics.Process.WaitForExit(Int32 milliseconds)
   at System.Diagnostics.Process.WaitForExit()
   at ParallelTest1_ForEach.Program.<>c__DisplayClass0_0.<Main>b__0(String currentFile)
   at System.Threading.Tasks.Parallel.<>c__DisplayClass30_0`2.<ForEachWorker>b__0(Int32 i)
   at System.Threading.Tasks.Parallel.<>c__DisplayClass17_0`1.<ForWorker>b__1()
   at System.Threading.Tasks.Task.InnerInvoke()
   at System.Threading.Tasks.Task.InnerInvokeWithArg(Task childTask)
   at System.Threading.Tasks.Task.<>c__DisplayClass176_0.<ExecuteSelfReplicating>b__0(Object )
   
   --- End of inner exception stack trace ---
   at System.Threading.Tasks.Task.ThrowIfExceptional(Boolean includeTaskCanceledExceptions)
   at System.Threading.Tasks.Task.Wait(Int32 millisecondsTimeout, CancellationToken cancellationToken)
   at System.Threading.Tasks.Task.Wait()
   at System.Threading.Tasks.Parallel.ForWorker[TLocal](Int32 fromInclusive, Int32 toExclusive, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, 
   Func`4 bodyWithLocal, Func`1 localInit, Action`1 localFinally)
   at System.Threading.Tasks.Parallel.ForEachWorker[TSource,TLocal](TSource[] array, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, 
   Action`3 bodyWithStateAndIndex, Func`4 bodyWithStateAndLocal, Func`5 bodyWithEverything, Func`1 localInit, Action`1 localFinally)
   at System.Threading.Tasks.Parallel.ForEachWorker[TSource,TLocal](IEnumerable`1 source, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, 
   Action`3 bodyWithStateAndIndex, Func`4 bodyWithStateAndLocal, Func`5 bodyWithEverything, Func`1 localInit, Action`1 localFinally)
   at System.Threading.Tasks.Parallel.ForEach[TSource](IEnumerable`1 source, ParallelOptions parallelOptions, Action`1 body)
   at ParallelTest1_ForEach.Program.Main(String[] args)

                Second level error: 
                Unhandled Exception: System.AggregateException: One or more errors occurred. ---> System.UnauthorizedAccessException: Access to the path 'E:\Pharrell Williams - Happy (1AM)\52945.b
at' is denied.
   at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
   at System.IO.File.InternalDelete(String path, Boolean checkHost)
   at System.IO.File.Delete(String path)
   at ParallelTest1_ForEach.Program.<>c__DisplayClass0_0.<Main>b__0(String currentFile)
   at System.Threading.Tasks.Parallel.<>c__DisplayClass30_0`2.<ForEachWorker>b__0(Int32 i)
   at System.Threading.Tasks.Parallel.<>c__DisplayClass17_0`1.<ForWorker>b__1()
   at System.Threading.Tasks.Task.InnerInvoke()
   at System.Threading.Tasks.Task.InnerInvokeWithArg(Task childTask)
   at System.Threading.Tasks.Task.<>c__DisplayClass176_0.<ExecuteSelfReplicating>b__0(Object )
   --- End of inner exception stack trace ---
   at System.Threading.Tasks.Task.ThrowIfExceptional(Boolean includeTaskCanceledExceptions)
   at System.Threading.Tasks.Task.Wait(Int32 millisecondsTimeout, CancellationToken cancellationToken)
   at System.Threading.Tasks.Task.Wait()
   at System.Threading.Tasks.Parallel.ForWorker[TLocal](Int32 fromInclusive, Int32 toExclusive, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, Func`4 bodyW
ithLocal, Func`1 localInit, Action`1 localFinally)
   at System.Threading.Tasks.Parallel.ForEachWorker[TSource,TLocal](TSource[] array, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, Action`3 bodyWithStateA
ndIndex, Func`4 bodyWithStateAndLocal, Func`5 bodyWithEverything, Func`1 localInit, Action`1 localFinally)
   at System.Threading.Tasks.Parallel.ForEachWorker[TSource,TLocal](IEnumerable`1 source, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, Action`3 bodyWithS
tateAndIndex, Func`4 bodyWithStateAndLocal, Func`5 bodyWithEverything, Func`1 localInit, Action`1 localFinally)
   at System.Threading.Tasks.Parallel.ForEach[TSource](IEnumerable`1 source, ParallelOptions parallelOptions, Action`1 body)
   at ParallelTest1_ForEach.Program.Main(String[] args)

                */
                File.Delete(filename);
            });



        }
    }
}
