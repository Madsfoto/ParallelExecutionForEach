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

                

                proc.Start();
                Console.WriteLine("started " + currentFile);

                // if (proc.waitforexit throws exception, delete file)

                try
                {
                    proc.WaitForExit();  // Another key point: As we are executing a external batch file, this program thinks that it can start as many bat files as it wants,
                                         // leading to excessive cpu and ram usage. 
                }
                catch (DivideByZeroException e) // An exception is thrown when the bat file gives an error.
                // The underlaying problem is that I generate the bat files by going from a 'start number' to 'start number + n' without checking if 'start number + n' is a valid filename
                // I let imagemagick handle that error (by giving a console file not found error), and just move on.
                
                {
                    File.Delete(filename); // This is the "The only tool I have is a hammer" approach. I am sure there are smarter and better ways to catch the exceptions
                    // but for the purposes of this program, this is fine. 
                    // Since the bat files are single use and can be remade, it does not matter that they are deleted. The files in here are not worth spending time on anyway. 
                }
                

                File.Delete(filename);
            });



        }
    }
}
