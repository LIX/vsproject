using System;
using System.Windows.Forms;
using ChartDirector;


namespace ChartDirectorSampleCode
{
    public partial class FrmRealTimeViewPort : Form
    {
        // The random data source
        private RandomWalk dataSource;

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
        private double[] dataSeriesA = new double[sampleSize];
        private double[] dataSeriesB = new double[sampleSize];

        // The index of the array position to which new data values are added.
        private int currentIndex = 0;

        // The full range is initialized to 180 seconds. It will extend when more data are available.
        private int initialFullRange = 180;

        // The visible range is initialized to 30 seconds.
        private int initialVisibleRange = 30;

        // The maximum zoom in is 5 seconds.
        private int zoomInLimit = 5;

        // If the track cursor is at the end of the data series, we will automatic move the track
        // line when new data arrives.
        private double trackLineEndPos;
        private bool trackLineIsAtEnd;


        public FrmRealTimeViewPort()
        {
            InitializeComponent();
        }

        private void FrmRealtimeViewPort_Load(object sender, EventArgs e)
        {
            // Initialize the WinChartViewer
            initChartViewer(winChartViewer1);

            // Start the random data generator
            dataSource = new RandomWalk(onData);
            dataSource.start();

            // Now can start the timers for data collection and chart update
            chartUpdateTimer.Interval = 100;
            chartUpdateTimer.Start();
        }

        private void FrmRealTimeViewPort_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (null != dataSource)
                dataSource.stop();
        }

        //
        // Initialize the WinChartViewer
        //
        private void initChartViewer(WinChartViewer viewer)
        {
            // Enable mouse wheel zooming
            viewer.MouseWheelZoomRatio = 1.1;

            // Configure the initial viewport 
            viewer.ViewPortWidth = initialVisibleRange / (double)initialFullRange;

            // Initially set the mouse usage to "Pointer" mode (Drag to Scroll mode)
            pointerPB.Checked = true;
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
            WinChartViewer viewer = winChartViewer1;
            
            // Enables auto scroll if the viewport is showing the latest data before the update
            bool autoScroll = (currentIndex > 0) && (0.01 + viewer.getValueAtViewPort("x",
                viewer.ViewPortLeft + viewer.ViewPortWidth) >= timeStamps[currentIndex - 1]);

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

                // Remove oldest data to leave space for new data. To avoid frequent removal, we ensure at
                // least 5% empty space available after removal.
                int originalIndex = currentIndex;
                currentIndex = sampleSize * 95 / 100 - 1;
                if (currentIndex > sampleSize - packets.Count)
                    currentIndex = sampleSize - packets.Count;

                for (int i = 0; i < currentIndex; ++i)
                {
                    int srcIndex = i + originalIndex - currentIndex;
                    timeStamps[i] = timeStamps[srcIndex];
                    dataSeriesA[i] = dataSeriesA[srcIndex];
                    dataSeriesB[i] = dataSeriesB[srcIndex];
                }
            }

            // Append the data from the queue to the data arrays
            for (int n = packets.Offset; n < packets.Offset + packets.Count; ++n)
            {
                DataPacket p = packets.Array[n];
                timeStamps[currentIndex] = p.elapsedTime;
                dataSeriesA[currentIndex] = p.series0;
                dataSeriesB[currentIndex] = p.series1;
                ++currentIndex;
            }

            //
            // As we added more data, we may need to update the full range. 
            //

            double startDate = timeStamps[0];
            double endDate = timeStamps[currentIndex - 1];
            
            // Use the initialFullRange (which is 60 seconds in this demo) if this is sufficient.
            double duration = endDate - startDate;
            if (duration < initialFullRange)
                endDate = startDate + initialFullRange;

            // Update the new full data range to include the latest data
            bool axisScaleHasChanged = viewer.updateFullRangeH("x", startDate, endDate,
                Chart.KeepVisibleRange);

            if (autoScroll)
            {
                // Scroll the viewport if necessary to display the latest data
                double viewPortEndPos = viewer.getViewPortAtValue("x", timeStamps[currentIndex - 1]);
                if (viewPortEndPos > viewer.ViewPortLeft + viewer.ViewPortWidth)
                {
                    viewer.ViewPortLeft = viewPortEndPos - viewer.ViewPortWidth;
                    axisScaleHasChanged = true;
                }
            }

            // Set the zoom in limit as a ratio to the full range
            viewer.ZoomInWidthLimit = zoomInLimit / (viewer.getValueAtViewPort("x", 1) -
                viewer.getValueAtViewPort("x", 0));

            // Trigger the viewPortChanged event. Updates the chart if the axis scale has changed
            // (scrolling or zooming) or if new data are added to the existing axis scale.
            viewer.updateViewPort(axisScaleHasChanged || (duration < initialFullRange), false);
        }

        //
        // The ViewPortChanged event handler. This event occurs if the user scrolls or zooms in
        // or out the chart by dragging or clicking on the chart. It can also be triggered by
        // calling WinChartViewer.updateViewPort.
        //
        private void winChartViewer1_ViewPortChanged(object sender, WinViewPortEventArgs e)
        {
            // Update the chart if necessary
            if (e.NeedUpdateChart)
                drawChart(winChartViewer1);

            // Update the full chart
            drawFullChart(viewPortControl1);
        }

        //
        // Draw the chart.
        //
        private void drawChart(WinChartViewer viewer)
        {
            // Get the start date and end date that are visible on the chart.
            double viewPortStartDate = viewer.getValueAtViewPort("x", viewer.ViewPortLeft);
            double viewPortEndDate = viewer.getValueAtViewPort("x", viewer.ViewPortLeft + viewer.ViewPortWidth);

            // Extract the part of the data arrays that are visible.
            double[] viewPortTimeStamps = null;
            double[] viewPortDataSeriesA = null;
            double[] viewPortDataSeriesB = null;

            if (currentIndex > 0)
            {
                // Get the array indexes that corresponds to the visible start and end dates
                int startIndex = (int)Math.Floor(Chart.bSearch2(timeStamps, 0, currentIndex, viewPortStartDate));
                int endIndex = (int)Math.Ceiling(Chart.bSearch2(timeStamps, 0, currentIndex, viewPortEndDate));

                // Extract the visible data
                if (timeStamps[endIndex] >= viewPortStartDate)
                {
                    int noOfPoints = endIndex - startIndex + 1;
                    viewPortTimeStamps = (double[])Chart.arraySlice(timeStamps, startIndex, noOfPoints);
                    viewPortDataSeriesA = (double[])Chart.arraySlice(dataSeriesA, startIndex, noOfPoints);
                    viewPortDataSeriesB = (double[])Chart.arraySlice(dataSeriesB, startIndex, noOfPoints);
                }

                // Keep track of the latest available data at chart plotting time
                trackLineEndPos = timeStamps[currentIndex - 1];
            }

            //
            // At this stage, we have extracted the visible data. We can use those data to plot the chart.
            //

            //================================================================================
            // Configure overall chart appearance.
            //================================================================================

            // Create an XYChart object of size 640 x 350 pixels
            XYChart c = new XYChart(640, 350);

            // Set the plotarea at (20, 30) with width 41 pixels less than chart width, and height 50 pixels
            // less than chart height. Use a vertical gradient from light blue (f0f6ff) to sky blue (a0c0ff)
            // as background. Set border to transparent and grid lines to white (ffffff).
            c.setPlotArea(20, 30, c.getWidth() - 41, c.getHeight() - 50, c.linearGradientColor(0, 30, 0,
                c.getHeight() - 20, 0xf0f6ff, 0xa0c0ff), -1, Chart.Transparent, 0xffffff, 0xffffff);       

            // As the data can lie outside the plotarea in a zoomed chart, we need enable clipping.
            c.setClipping();

            // Add a title to the chart using 18 pts Times New Roman Bold Italic font
            c.addTitle("   Real-Time Chart with Viewport Control", "Arial", 18);

            // Add a legend box at (55, 25) using horizontal layout. Use 8pts Arial Bold as font. Set the
            // background and border color to Transparent and use line style legend key.
            LegendBox b = c.addLegend(55, 25, false, "Arial Bold", 10);
            b.setBackground(Chart.Transparent);
            b.setLineStyleKey();

            // Set the x and y axis stems to transparent and the label font to 10pt Arial
            c.xAxis().setColors(Chart.Transparent);
            c.yAxis().setColors(Chart.Transparent);
            c.xAxis().setLabelStyle("Arial", 10);
            c.yAxis().setLabelStyle("Arial", 10, 0x336699);

            // Add axis title using 10pts Arial Bold Italic font
            c.yAxis().setTitle("Ionic Temperature (C)", "Arial Bold", 10);
            c.yAxis().setTitlePos(Chart.Left, 1);

            // Configure the y-axis label to be inside the plot area and above the horizontal grid lines
            c.yAxis().setLabelGap(-1);
            c.yAxis().setLabelAlignment(1);
            c.yAxis().setMargin(20);

            // Configure the x-axis labels to be to the left of the vertical grid lines
            c.xAxis().setLabelAlignment(1);
            
            //================================================================================
            // Add data to chart
            //================================================================================

            //
            // In this example, we represent the data by lines. You may modify the code below to use other
            // representations (areas, scatter plot, etc).
            //

            // Add a line layer for the lines, using a line width of 2 pixels
            LineLayer layer = c.addLineLayer2();
            layer.setLineWidth(2);
            layer.setFastLineMode();

            // Now we add the 3 data series to a line layer, using the color red (ff0000), green (00cc00)
            // and blue (0000ff)
            layer.setXData(viewPortTimeStamps);
            layer.addDataSet(viewPortDataSeriesA, 0xff0000, "Alpha");
            layer.addDataSet(viewPortDataSeriesB, 0x00cc00, "Beta");

            //================================================================================
            // Configure axis scale and labelling
            //================================================================================

            if (currentIndex > 0)
                c.xAxis().setDateScale(viewPortStartDate, viewPortEndDate);

            // For the automatic axis labels, set the minimum spacing to 75/30 pixels for the x/y axis.
            c.xAxis().setTickDensity(75);
            c.yAxis().setTickDensity(40);

            // We use "hh:nn:ss" as the axis label format.
            c.xAxis().setLabelFormat("{value|nn:ss}");

            // We make sure the tick increment must be at least 1 second.
            c.xAxis().setMinTickInc(1);

            // Set the auto-scale margin to 0.05, and the zero affinity to 0.6
            c.yAxis().setAutoScale(0.05, 0.05, 0.6);

            //================================================================================
            // Output the chart
            //================================================================================

            // We need to update the track line too. If the mouse is moving on the chart (eg. if 
            // the user drags the mouse on the chart to scroll it), the track line will be updated
            // in the MouseMovePlotArea event. Otherwise, we need to update the track line here.
            if (!winChartViewer1.IsInMouseMoveEvent)
                trackLineLabel(c, trackLineIsAtEnd ? c.getWidth() : viewer.PlotAreaMouseX);
 
            viewer.Chart = c;
        }

        //
        // Draw track line with data labels
        //
        private double trackLineLabel(XYChart c, int mouseX)
        {
            // Clear the current dynamic layer and get the DrawArea object to draw on it.
            DrawArea d = c.initDynamicLayer();

            // The plot area object
            PlotArea plotArea = c.getPlotArea();

            // Get the data x-value that is nearest to the mouse, and find its pixel coordinate.
            double xValue = c.getNearestXValue(mouseX);
            int xCoor = c.getXCoor(xValue);
            if (xCoor < plotArea.getLeftX())
                return xValue;

            // Draw a vertical track line at the x-position
            d.vline(plotArea.getTopY(), plotArea.getBottomY(), xCoor, 0x888888);

            // Draw a label on the x-axis to show the track line position.
            string xlabel = "<*font,bgColor=000000*> " + c.xAxis().getFormattedLabel(xValue, "hh:nn:ss.ff") +
                " <*/font*>";
            TTFText t = d.text(xlabel, "Arial Bold", 10);

            // Restrict the x-pixel position of the label to make sure it stays inside the chart image.
            int xLabelPos = Math.Max(0, Math.Min(xCoor - t.getWidth() / 2, c.getWidth() - t.getWidth()));
            t.draw(xLabelPos, plotArea.getBottomY() + 1, 0xffffff);

            // Iterate through all layers to draw the data labels
            for (int i = 0; i < c.getLayerCount(); ++i)
            {
                Layer layer = c.getLayerByZ(i);

                // The data array index of the x-value
                int xIndex = layer.getXIndexOf(xValue);

                // Iterate through all the data sets in the layer
                for (int j = 0; j < layer.getDataSetCount(); ++j)
                {
                    ChartDirector.DataSet dataSet = layer.getDataSetByZ(j);

                    // Get the color and position of the data label
                    int color = dataSet.getDataColor();
                    int yCoor = c.getYCoor(dataSet.getPosition(xIndex), dataSet.getUseYAxis());

                    // Draw a track dot with a label next to it for visible data points in the plot area
                    if ((yCoor >= plotArea.getTopY()) && (yCoor <= plotArea.getBottomY()) && (color !=
                        Chart.Transparent) && (!string.IsNullOrEmpty(dataSet.getDataName())))
                    {
                        d.circle(xCoor, yCoor, 4, 4, color, color);

                        string label = "<*font,bgColor=" + color.ToString("x") + "*> " + c.formatValue(
                            dataSet.getValue(xIndex), "{value|P4}") + " <*/font*>";
                        t = d.text(label, "Arial Bold", 10);

                        // Draw the label on the right side of the dot if the mouse is on the left side the
                        // chart, and vice versa. This ensures the label will not go outside the chart image.
                        if (xCoor <= (plotArea.getLeftX() + plotArea.getRightX()) / 2)
                            t.draw(xCoor + 5, yCoor, 0xffffff, Chart.Left);
                        else
                            t.draw(xCoor - 5, yCoor, 0xffffff, Chart.Right);
                    }
                }
            }

            return xValue;
        }

        //
        // Draw the full thumbnail chart and display it in the given WinViewPortControl
        //
        private void drawFullChart(WinViewPortControl vpc)
        {
            // Create an XYChart object of size 640 x 50 pixels   
            XYChart c = new XYChart(640, 60);

            // Set the plotarea with the same horizontal position as that in the main chart for alignment.
            c.setPlotArea(20, 0, c.getWidth() - 41, c.getHeight() - 1, 0xc0d8ff, -1, 0xcccccc, 
                Chart.Transparent, 0xffffff);

            // Set the x axis stem to transparent and the label font to 10pt Arial
            c.xAxis().setColors(Chart.Transparent);
            c.xAxis().setLabelStyle("Arial", 10);

            // Put the x-axis labels inside the plot area by setting a negative label gap. Use
            // setLabelAlignment to put the label at the right side of the tick.
            c.xAxis().setLabelGap(-1);
            c.xAxis().setLabelAlignment(1);

            // Set the y axis stem and labels to transparent (that is, hide the labels)
            c.yAxis().setColors(Chart.Transparent, Chart.Transparent);

            // Add a line layer for the lines with fast line mode enabled
            LineLayer layer = c.addLineLayer();
            layer.setFastLineMode();

            // Now we add the 3 data series to a line layer, using the color red (0xff3333), green
            // (0x008800) and blue (0x3333cc)
            layer.setXData((double[])Chart.arraySlice(timeStamps, 0, currentIndex));
            layer.addDataSet((double[])Chart.arraySlice(dataSeriesA, 0, currentIndex), 0xff3333);
            layer.addDataSet((double[])Chart.arraySlice(dataSeriesB, 0, currentIndex), 0x008800);

            // The x axis scales should reflect the full range of the view port
            c.xAxis().setDateScale(vpc.Viewer.getValueAtViewPort("x", 0), vpc.Viewer.getValueAtViewPort("x", 1));
            c.xAxis().setLabelFormat("{value|nn:ss}");

            // For the automatic x-axis labels, set the minimum spacing to 75 pixels.
            c.xAxis().setTickDensity(75);

            // For the auto-scaled y-axis, as we hide the labels, we can disable axis rounding. This can
            // make the axis scale fit the data tighter.
            c.yAxis().setRounding(false, false);

            // Output the chart
            vpc.Chart = c;
        }

        //
        // Draw track cursor when mouse is moving over plotarea
        //
        private void winChartViewer1_MouseMovePlotArea(object sender, MouseEventArgs e)
        {
            WinChartViewer viewer = (WinChartViewer)sender;
            double trackLinePos = trackLineLabel((XYChart)viewer.Chart, viewer.PlotAreaMouseX);
            trackLineIsAtEnd = (currentIndex <= 0) || (trackLinePos == trackLineEndPos);
            viewer.updateDisplay();
        }

        //
        // Pointer (Drag to Scroll) button event handler
        //
        private void pointerPB_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
                winChartViewer1.MouseUsage = WinChartMouseUsage.ScrollOnDrag;
        }

        //
        // Zoom In button event handler
        //
        private void zoomInPB_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
                winChartViewer1.MouseUsage = WinChartMouseUsage.ZoomIn;
        }

        //
        // Zoom Out button event handler
        //
        private void zoomOutPB_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
                winChartViewer1.MouseUsage = WinChartMouseUsage.ZoomOut;
        }

        //
        // Save button event handler
        //
        private void savePB_Click(object sender, EventArgs e)
        {
            // The standard Save File dialog
            SaveFileDialog fileDlg = new SaveFileDialog();
            fileDlg.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|BMP (*.bmp)|*.bmp|" +
                "SVG (*.svg)|*.svg|PDF (*.pdf)|*.pdf";
            fileDlg.FileName = "chartdirector_demo";
            if (fileDlg.ShowDialog() != DialogResult.OK)
                return;

            // Save the chart
            if (null != winChartViewer1.Chart)
                winChartViewer1.Chart.makeChart(fileDlg.FileName);
        }
    }
}