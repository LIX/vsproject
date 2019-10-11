using System;
using System.Windows.Forms;
using ChartDirector;


namespace ChartDirectorSampleCode
{
    public partial class FrmRealTimeSweep : Form
    {
        // The random data source
        private RandomWave dataSource;

        // A thread-safe queue with minimal read/write contention
        private class DataPacket
        {
            public double elapsedTime;
            public double series0;
            public double series1;
        };
        private DoubleBufferedQueue<DataPacket> buffer = new DoubleBufferedQueue<DataPacket>();

        // The data arrays that store the realtime data. The data arrays are updated in realtime. 
        // In this demo, we store at most 10000 values. 
        private const int sampleSize = 10000;
        private double[] timeStamps = new double[sampleSize];
        private double[] channel1 = new double[sampleSize];
        private double[] channel2 = new double[sampleSize];
        
        // The index of the array position to which new data values are added.
        private int currentIndex = 0;

        // The time range of the sweep chart
        private const int timeRange = 60;

        public FrmRealTimeSweep()
        {
            InitializeComponent();
        }

        private void FrmRealtimeSweep_Load(object sender, EventArgs e)
        {
            // Start the random data generator
            dataSource = new RandomWave(onData);
            dataSource.start();

            // Now can start the timers for data collection and chart update
            chartUpdateTimer.Interval = 100;
            chartUpdateTimer.Start();
        }

        private void FrmRealTimeSweep_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (null != dataSource)
                dataSource.stop();
        }

        //
        // Handles realtime data from RandomWave. The RandomWave will call this method from its own thread.
        //
        private void onData(double elapsedTime, double series0, double series1)
        {
            DataPacket p = new DataPacket();
            p.elapsedTime = elapsedTime;
            p.series0 = series0;
            p.series1 = series1;
            buffer.put(p);
        }

        //
        // Update the chart and the viewport periodically
        //
        private void chartUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Get new data from the queue and append them to the data arrays
            var packets = buffer.get();
            if (packets.Count <= 0)
                return;

            // if data arrays have insufficient space, we need to remove some old data.
            if (currentIndex + packets.Count >= sampleSize)
            {
                // For safety, we check if the queue contains too much data than the entire data arrays. If
                // this is the case, we only use the latest data to completely fill the data arrays.
                if (packets.Count > sampleSize)
                    packets = new ArraySegment<DataPacket>(packets.Array, packets.Count - sampleSize, sampleSize);

                // Remove data older than the time range to leave space for new data. The data removed must 
                // be at least equal to the packet count.
                int originalIndex = currentIndex;
                if (currentIndex > 0)
                    currentIndex -= (int)(Chart.bSearch(timeStamps, 0, currentIndex, timeStamps[currentIndex - 1] - timeRange));
                if (currentIndex > sampleSize - packets.Count)
                    currentIndex = sampleSize - packets.Count;

                for (int i = 0; i < currentIndex; ++i)
                {
                    int srcIndex = i + originalIndex - currentIndex;
                    timeStamps[i] = timeStamps[srcIndex];
                    channel1[i] = channel1[srcIndex];
                    channel2[i] = channel2[srcIndex];
                }
            }

            // Append the data from the queue to the data arrays
            for (int n = packets.Offset; n < packets.Offset + packets.Count; ++n)
            {
                DataPacket p = packets.Array[n];
                timeStamps[currentIndex] = p.elapsedTime;
                channel1[currentIndex] = p.series0;
                channel2[currentIndex] = p.series1;
                ++currentIndex;
            }

            winChartViewer1.updateViewPort(true, false);
        }

        //
        // Update the chart if the winChartViewer size is changed
        //
        private void winChartViewer1_SizeChanged(object sender, EventArgs e)
        {
            winChartViewer1.updateViewPort(true, false);
        }
        
        //
        // The ViewPortChanged event handler
        //
        private void winChartViewer1_ViewPortChanged(object sender, WinViewPortEventArgs e)
        {
            // Update the chart if necessary
            if (e.NeedUpdateChart)
                drawChart(winChartViewer1);
        }

        //
        // Draw the chart
        //
        private void drawChart(WinChartViewer viewer)
        {
            // Have not started collecting data ???
            if (currentIndex <= 0)
                return;

            // The start time is equal to the latest time minus the time range of the chart
            double startTime = timeStamps[currentIndex - 1] - timeRange;
            int startIndex = (int)Math.Ceiling(Chart.bSearch(timeStamps, 0, currentIndex, startTime) - 0.1);

            // For a sweep chart, if the line goes beyond the right border, it will wrap back to 
            // the left. We need to determine the wrap position (the right border).
            double wrapTime = Math.Floor(startTime / timeRange + 1) * timeRange;
            double wrapIndex = Chart.bSearch(timeStamps, 0, currentIndex, wrapTime);
            int wrapIndexA = (int)Math.Ceiling(wrapIndex);
            int wrapIndexB = (int)Math.Floor(wrapIndex);

            // The data arrays and the colors and names of the data series
            var allArrays = new[] { timeStamps, channel1, channel2 };
            int[] colors = { 0xff0000, 0x00cc00 };
            string[] names = { "Channel 1", "Channel 2" };

            // Split all data arrays into two parts A and B at the wrap position. The B part is the 
            // part that is wrapped back to the left.
            var allArraysA = new double[allArrays.Length][];
            var allArraysB = new double[allArrays.Length][];
            for (int i = 0; i < allArrays.Length; ++i)
            {
                allArraysA[i] = (double[])Chart.arraySlice(allArrays[i], startIndex, wrapIndexA - startIndex + 1);
                allArraysB[i] = (double[])Chart.arraySlice(allArrays[i], wrapIndexB, currentIndex - wrapIndexB);
            }

            // Normalize the plotted timeStamps (the first element of allArrays) to start from 0
            for (int i = 0; i < allArraysA[0].Length; ++i)
                allArraysA[0][i] -= wrapTime - timeRange;
            for (int i = 0; i < allArraysB[0].Length; ++i)
                allArraysB[0][i] -= wrapTime;

            //
            // Now we have prepared all the data and can plot the chart.
            //

            //================================================================================
            // Configure overall chart appearance.
            //================================================================================

           
            XYChart c = new XYChart(Math.Max(300, viewer.Width), Math.Max(150, viewer.Height));

            // Set the plotarea at (0, 0) with width 1 pixel less than chart width, and height 20 pixels
            // less than chart height. Use a vertical gradient from light blue (f0f6ff) to sky blue (a0c0ff)
            // as background. Set border to transparent and grid lines to white (ffffff).
            c.setPlotArea(0, 0, c.getWidth() - 1, c.getHeight() - 20, c.linearGradientColor(0, 0, 0,
                c.getHeight() - 20, 0xf0f6ff, 0xa0c0ff), -1, Chart.Transparent, 0xffffff, 0xffffff);

            // In our code, we can overdraw the line slightly, so we clip it to the plot area.
            c.setClipping();

            // Add a legend box at the right side using horizontal layout. Use 10pt Arial Bold as font. Set
            // the background and border color to Transparent and use line style legend key.
            LegendBox b = c.addLegend(c.getWidth() - 1, 10, false, "Arial Bold", 10);
            b.setBackground(Chart.Transparent);
            b.setAlignment(Chart.Right);
            b.setLineStyleKey();

            // Set the x and y axis stems to transparent and the label font to 10pt Arial
            c.xAxis().setColors(Chart.Transparent);
            c.yAxis().setColors(Chart.Transparent);
            c.xAxis().setLabelStyle("Arial", 10);
            c.yAxis().setLabelStyle("Arial", 10, 0x336699);

            // Configure the y-axis label to be inside the plot area and above the horizontal grid lines
            c.yAxis().setLabelGap(-1);
            c.yAxis().setMargin(20);
            c.yAxis().setLabelAlignment(1);

            // Configure the x-axis labels to be to the left of the vertical grid lines
            c.xAxis().setLabelAlignment(1);

            //================================================================================
            // Add data to chart
            //================================================================================

            // Draw the lines, which consists of A segments and B segments (the wrapped segments)
            foreach (var dataArrays in new[] { allArraysA, allArraysB })
            {
                LineLayer layer = c.addLineLayer2();
                layer.setLineWidth(2);
                layer.setFastLineMode();

                // The first element of dataArrays is the timeStamp, and the rest are the data.
                layer.setXData(dataArrays[0]);
                for (int i = 1; i < dataArrays.Length; ++i)
                    layer.addDataSet(dataArrays[i], colors[i - 1], names[i - 1]);

                // Disable legend entries for the B lines to avoid duplication with the A lines
                if (dataArrays == allArraysB)
                    layer.setLegend(Chart.NoLegend);
            }

            // The B segments contain the latest data. We add a vertical line at the latest position. 
            int lastIndex = allArraysB[0].Length - 1;
            Mark m = c.xAxis().addMark(allArraysB[0][lastIndex], -1);
            m.setMarkColor(0x0000ff, Chart.Transparent, Chart.Transparent);
            m.setDrawOnTop(false);

            // We also add a symbol and a label for each data series at the latest position
            for (int i = 1; i < allArraysB.Length; ++i)
            {
                // Add the symbol
                Layer layer = c.addScatterLayer(new double[] { allArraysB[0][lastIndex] }, new double[] {
                    allArraysB[i][lastIndex] }, "", Chart.CircleSymbol, 9, colors[i - 1], colors[i - 1]);
                layer.moveFront();

                // Add the label
                string label = "<*font,bgColor=" + colors[i - 1].ToString("x") + "*> {value|P4} <*/font*>";
                layer.setDataLabelFormat(label);
                
                // The label style               
                ChartDirector.TextBox t = layer.setDataLabelStyle("Arial Bold", 10, 0xffffff);
                bool isOnLeft = allArraysB[0][lastIndex] <= timeRange / 2;
                t.setAlignment(isOnLeft ? Chart.Left : Chart.Right);
                t.setMargin(isOnLeft ? 5 : 0, isOnLeft ? 0 : 5, 0, 0);
            }

            //================================================================================
            // Configure axis scale and labelling
            //================================================================================

            c.xAxis().setLinearScale(0, timeRange);

            // For the automatic axis labels, set the minimum spacing to 75/40 pixels for the x/y axis.
            c.xAxis().setTickDensity(75);
            c.yAxis().setTickDensity(40);

            // Set the auto-scale margin to 0.05, and the zero affinity to 0.6
            c.yAxis().setAutoScale(0.05, 0.05, 0.6);

            //================================================================================
            // Output the chart
            //================================================================================

            viewer.Chart = c;
        }
    }
}