using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ParallelTest1_ForEach
{
    class Program
    {
        public int currentCount = 0;
        public int maxCount = 0;

        public void setMaxCount(int count)
        {
            maxCount = count;
        }

        public string percentDone()
        {
            float currentCountFloat = currentCount;
            float percentdone = (currentCountFloat / maxCount);
            string percStr = percentdone.ToString("P");
            return percStr;
        }

        public int imagesLeft()
        {
            int imgsLeft = maxCount - currentCount;
            return imgsLeft;
        }

        public int imgsDone()
        {
            return currentCount;
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
            
            int numberOfMaxDegreeOfParallelismInt = 1;

            // Taken from https://stackoverflow.com/a/11791998/7963551

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("The default is to have 3 parallel executions at once, overwrite with a commandline argument" );
                numberOfMaxDegreeOfParallelismInt = 3; // Set to 3, which is 75% of a quadcore machine which I expect is the minimum amount of cores,
                // that people who are interested in this program will have. I give the posibility to overwrite the default by giving an argument, 
                // so people with more cores can use them all at once.
            }
            else
            {
                
                string strMaxDegrees = args[0];
                numberOfMaxDegreeOfParallelismInt = Convert.ToInt32(strMaxDegrees);
                Console.WriteLine("Executing " + numberOfMaxDegreeOfParallelismInt + " files at once");
            }

            // Set the maximum parallel executions via an argument. The other option would be to hardcode it, which I am not a fan of. 
            // The third option is to do (1) "Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 1.0))", making use of 75% of the total processer usage. 

           
            // The magic is in the foreach statement: 
            // For each of the files in the list, execute the 'current file' in effect the next in the list.
            // While no more than maxParallelExecutions is running at the same time

            //Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = 6 }, // use the line from (1) . // orginal line

            Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = numberOfMaxDegreeOfParallelismInt }, 
                (currentFile) =>
            {
                String fileName = Path.GetFileName(currentFile); // Test if filename is required, can currentFile be used?

                // Console.WriteLine("started " + currentFile);  // Disabled because it does not matter to the viewer which file is being executed,
                // The progress indicator is the important thing.
                p.execbat(currentFile); // moved the executing logic to a function, so it's self contained and thus will not generate the exceptions seen before. 

                Interlocked.Increment(ref p.currentCount);

                Console.WriteLine("percent done = " + p.percentDone() + " | Images to go = " + p.imagesLeft() + " | Images done this session = " + p.imgsDone());
                // TODO: Time remaning. 
                //
                /*
                Stopwwatch start at execbat(), stop after execution => How long does one file tale
                Time for 1 file * leftovers (imagesLeft() ) == Time left. 
                However the time is not always uniform, so we need a average time.
                UNTESTED: 
                Put stopwatch elapsedtime into variable, restart timer, execute, stop timer, put elapsed time into variable. 
                Then after incremented, do time taken / imgsDone() == average time taken * imgsLeft() == Time remaining.
                This way the counter is more accurate as time goes on. 





                */
                File.Delete(currentFile);
            });



        }
    }
}
