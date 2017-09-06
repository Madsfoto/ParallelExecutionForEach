using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;

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

        public int execbat(string fileName)
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
            try
            {
                File.Delete(fileName);
            }
            catch
            {
                // The Parallel.ForEach function seems to have an issue with my way of executing.
                // It gives Execption IO errors, the file is in use by another process. 
                // This way those exceptions are ignored and the process can continue through the set of .bat files. 
                // This assumes that the process completes in order, which it should because if the file is not deleted then it can be re-run.
                // If that is not the case, I'll figure something else out. 
            }
            return 0;   // The return 0 is a relic of my testing if it made a difference wether this function was void or int. 
                        // It did not. Since I am not using the return value for anything I'm keeping it as it is. 
                        // Yes, I realize it might be technical debt but for a program this size it does not matter.
        }

        public string timeRemaining(int seconds)
        {
            int daysLeft = 0;
            int hoursLeft = 0;
            int minutesLeft = 0;
            int secondsLeft = 0;
            string timeRemainStr = "";

            int totalIntInS = seconds;

            Double daysLeftDB = 0;
            daysLeftDB = Math.Floor((double)totalIntInS / 60 / 60 / 24);
            daysLeft = (int)daysLeftDB;

            totalIntInS = totalIntInS - (daysLeft * 60 * 60 * 24);

            Double hoursLeftDB = 0;
            hoursLeftDB = Math.Floor((double)totalIntInS / 60 / 60);
            hoursLeft = (int)hoursLeftDB;

            totalIntInS = totalIntInS - (hoursLeft * 60 * 60);

            Double minutesLeftDB = 0;
            minutesLeftDB = Math.Floor((double)totalIntInS / 60);
            minutesLeft = (int)minutesLeftDB;

            totalIntInS = totalIntInS - (minutesLeft * 60);

            secondsLeft = totalIntInS;

            return timeRemainStr = String.Format("{0:00} days, {1:00} hours, {2:00} minutes and {3:00} seconds", daysLeft, hoursLeft, minutesLeft, secondsLeft);
        }


        static void Main(string[] args)
        {
            Program p = new Program();

            var watch = Stopwatch.StartNew();
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
            
            

            Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = numberOfMaxDegreeOfParallelismInt }, (currentFile) =>
                {
                    String fileName = Path.GetFileName(currentFile); // Test if filename is required, can currentFile be used?

                    // Console.WriteLine("started " + currentFile);  // Disabled because it does not matter to the viewer which file is being executed,
                    // The progress indicator is the important thing.
                    var timeForOneExec = Stopwatch.StartNew();
                    p.execbat(currentFile); // moved the executing logic to a function, so it's self contained and thus will not generate the exceptions seen before. 
                    timeForOneExec.Stop();
                    
                    Interlocked.Increment(ref p.currentCount);
                    double timeSpanTicks = watch.ElapsedTicks;
                    // TODO: if time larger than 1 sec, remove x sec from time in ms !!!
                    // How much larger than 1 => remove that amount from ms.
                    Double avgTimeInMS = 0;
                    
                    
                    double avgTimeInS = ((timeSpanTicks / Stopwatch.Frequency) / p.currentCount);
                    int intSec = (int)Math.Floor(avgTimeInS);

                    // If average time in seconds is more than 1, remove the 1000 ms from the ms calculations. If this block is not there, times would be written as
                    // 01:1xxx, where 01 is seconds and 1xxx is the number of ms. Removing 1000 ms gives the expected output of 01:xxx. 
                    if (avgTimeInS >= 1)
                    {
                        avgTimeInMS = (((timeSpanTicks / Stopwatch.Frequency) * 1000) / p.currentCount)-(1000*intSec);
                    }
                    else
                    {
                        avgTimeInMS = (((timeSpanTicks / Stopwatch.Frequency) * 1000) / p.currentCount);
                    }
                    
                    
                    TimeSpan timeS1Exec = timeForOneExec.Elapsed;


                    
                    
                    // Time for 1 execution
                    string timeStr = String.Format("{0:00}:{1:000}", timeS1Exec.Seconds, timeS1Exec.Milliseconds);
                    string avgTime = String.Format("{0:00}:{1:000}", avgTimeInS, avgTimeInMS);

                    // Caclulations for the time remaning
                    
                    int totalSeconds = (int)Math.Floor((avgTimeInS * p.imagesLeft()));

                    String timeRemainStr = p.timeRemaining(totalSeconds); // Done via own function
                    TimeSpan timeSpan = TimeSpan.FromSeconds(((timeSpanTicks / Stopwatch.Frequency) / p.currentCount) * p.imagesLeft());
                    //String timeRemainStr = timeSpan.ToString(); // The correct way to do it. Formatting required if someone wants to use it.
                    



                    Console.WriteLine(p.percentDone() + " done"  +" | Images to go = " + p.imagesLeft() + " | Images done this session = " + p.imgsDone() + 
                        " | Avg time taken = " + avgTime + " | Time remaning " + timeRemainStr);

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

                });
            // Time taken calculations
            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            
            

            Console.WriteLine("Finished at: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("Time taken: " + elapsedTime);
            
        }
    }
}
