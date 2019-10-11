using System;
using System.Threading;
using System.Diagnostics;


namespace ChartDirectorSampleCode
{
    class RandomWave
    {        // The callback function to handle the generated data
        public delegate void DataHandler(double elapsedTime, double series0, double series1);
        private DataHandler handler;

        // Random number genreator thread
        Thread pingThread;
        private bool stopThread;

        // The period of the data series in milliseconds. This random series implementation just use the 
        // windows timer for timing. In many computers, the default windows timer resolution is 1/64 sec,
        // or 15.6ms. This means the interval may not be exactly accurate.
        const int interval = 100;

        public RandomWave(DataHandler handler)
        {
            this.handler = handler;
        }

        //
        // Start the random generator thread
        //        
        public void start()
        {
            if (null != pingThread)
                return;

            pingThread = new Thread(threadProc);
            pingThread.Start();
        }

        //
        // Stop the random generator thread
        //
        public void stop()
        {
            stopThread = true;
            if (null != pingThread)
                pingThread.Join();
            pingThread = null;
            stopThread = false;
        }

        //
        // The random generator thread
        //
        void threadProc(object obj)
        {
            long currentTime = 0;
            long nextTime = 0;

            // Variables to keep track of the timing
            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (!stopThread)
            {
                // Compute the next data value
                currentTime = timer.Elapsed.Ticks / 10000;

                double p = currentTime / 1000.0 * 4;
                double series0 = 20 + Math.Cos(p * 2.2) * 10 + 1 / (Math.Cos(p) * Math.Cos(p) + 0.01);
                double series1 = 210 + 60 * Math.Sin(p / 21.7) * Math.Sin(p / 7.8);

                // Call the handler
                handler(currentTime / 1000.0, series0, series1);

                // Sleep until next walk
                if ((nextTime += interval) <= currentTime)
                    nextTime = currentTime + interval;

                Thread.Sleep((int)(nextTime - currentTime));
            }
        }
    }
}
