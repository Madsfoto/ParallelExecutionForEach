using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ParallelTest1_ForEach
{
    class Program
    {
        public int currentCount = 1;
        public int maxCount = 1;

        public void setMaxCount(int count)
        {
            maxCount = count;
        }

        public float percentDone()
        {
            float currentCountFloat = currentCount;
            float percentdone = ((currentCountFloat / maxCount)*100);

            return percentdone;
        }

        public void execbat(string fileName)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            // remove the next 2 lines to show the execution of processes. 
            // They start in focus, interrupting your work. 
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;

            proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory(); // It's easier than having to specify where this program will be run.
            proc.StartInfo.FileName = fileName; // Set the currentfile as the one being executed. Incrementing automatically.
            proc.Start();
            proc.WaitForExit();
        }


        static void Main(string[] args)
        {
            Program p = new Program();

            // Get a list of all the bat files in the current directory, so we can execute them later
            var paths = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bat");
            int count = paths.Length;

            p.setMaxCount(count);


            // Set the maximum parallel executions via an argument. The other option would be to hardcode it, which I am not a fan of. 
            // The third option is to do (1) "Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 1.0))", making use of 75% of the total processer usage. 
            // Currently this does not work. 
            // string str = args[0];
            
            // int maxParallelExecutions = Convert.ToInt32(str); // I am currently unable to take an int directly from the argument list, hence the conversion here
            // Crashes if no argument is given


            // The magic is in the foreach statement: 
            // For each of the files in the list, execute the 'current file' in effect the next in the list.
            // While no more than maxParallelExecutions is running at the same time

            Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = 6 }, // use the line from (1) .
                (currentFile) =>
            {
                String fileName = Path.GetFileName(currentFile); // Test if filename is required, can currentFile be used?

                Console.WriteLine("started " + currentFile);
                p.execbat(currentFile); // moved the executing logic to a function, so it's self contained and thus will not generate the exceptions seen before. 

                Interlocked.Increment(ref p.currentCount);

                // Before I've had some "how far are we" logic, but it didn't work. 


                // if (proc.waitforexit throws exception, ignore and delete file)



                //proc.WaitForExit(2147483647);  // Another key point: As we are executing a external batch file, this program thinks that it can start as many bat files as it wants,
                // leading to excessive cpu and ram usage. 
                Console.WriteLine("percent done = " + p.percentDone());
                File.Delete(currentFile);
            });



        }
    }
}
