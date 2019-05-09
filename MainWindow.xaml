using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;


namespace ConwayGameOfLife
{
    static class Constants
    {
        public const int BoardHeight = 40;
        public const int BoardWidth = 80;
        public const int TileSize = 10;

        public const int MinRows = 10;
        public const int MaxRows = 60;
        public const int MinCols = 10;
        public const int MaxCols = 140;

        public const byte Dead = 0;
        public const byte Alive = 1;
        public const byte Virus = 2;


        public static Brush DeadColor = Brushes.White;
        public static Brush AliveColor = Brushes.Black;
        public static Brush VirusColor = Brushes.LightGreen;
    }

    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        // Used to convert from ListBox index to m_board indexes and back.
        struct Point
        {
            int x;
            int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int X { get => x; set => x = value; }
            public int Y { get => y; set => y = value; }
        }

        ////////////////////////
        // Data Members
        //

        // This is our data structure for the backend of the board. It reflects the state of the ListBoxItems on the board.
        private List<List<byte> > m_board = new List<List<byte>>();
        private int m_boardRows = Constants.BoardHeight;
        private int m_boardCols = Constants.BoardWidth;

        private bool m_running = true;
        private bool m_step = false;
        private bool m_resizing = false;

        // For randomizing the board.
        private static Random m_rng = new Random();
        private static int m_randChance = 50;

        // This timer will be set to m_speed interval whenever the run button is toggled on.
        // After each interval the Step() function is called.
        private System.Timers.Timer m_timer = new System.Timers.Timer();
        private int m_speed = 1000; // In milliseconds.

        // Separate thread to perform Step() in 
        private Thread boardThread;
        
        // Window component constructor.
        public MainWindow()
        {
            InitializeComponent();

            // Window will resize automatically with board size.
            this.ResizeMode = ResizeMode.NoResize;

            // Need the window to resize to our contents.
            this.SizeToContent = SizeToContent.WidthAndHeight;

            // Initialize the board data structure.
            BoardInit( m_board, Constants.BoardHeight, Constants.BoardWidth );
            m_board.Capacity = Constants.MaxRows;

            // Resize main list for board width/height.
            MainList.Height = Constants.BoardHeight * Constants.TileSize + 4;
            MainList.MaxWidth = Constants.BoardWidth * Constants.TileSize + 4;
           
            // Fill board.
            int numberOfTiles = Constants.BoardHeight * Constants.BoardWidth;
            for (int i = 0; i < numberOfTiles; ++i)
            {
                ListBoxItem tile = new ListBoxItem();
                MainList.Items.Insert(0, tile);
            }
            
            // Display current board size.
            RowTextBox.Text = m_boardRows.ToString();
            ColTextBox.Text = m_boardCols.ToString();

            // Timer for when the auto-run is selected.
            m_timer.Elapsed += RunEvent;
            m_timer.Enabled = false;
            m_timer.AutoReset = true;
            m_timer.Interval = m_speed;

            SpeedSlider.Value = m_speed;

            RandomBtn.Click += new RoutedEventHandler(RandomizeBoard);

            // Start our worker thread.
            boardThread = new Thread(Run);
            boardThread.SetApartmentState(ApartmentState.STA); // This allows the thread to access data members.
            boardThread.Start();
        }

        // Handler for the m_timer event.
        private void RunEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            m_step = true;
        }

        // Terminated thread when closing application.
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            m_running = false;
            boardThread.Abort();
        }

        // Step button on-click handler.
        private void StepBtn_Click(object sender, RoutedEventArgs e)
        {
            m_step = true;
        }

        // Speed slider handler.
        private void SpeedSlider_Changed(object sender, RoutedEventArgs e)
        {
            m_timer.Interval = SpeedSlider.Value;
            if (SliderVal == null)
            {
                SliderVal = new Label();
            }
            SliderVal.Content = String.Format( "{0:F0} ms", SpeedSlider.Value );
        }

        // Run button on-click handler.
        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            m_timer.Enabled = !m_timer.Enabled;
            if (m_timer.Enabled)
                RunBtn.Content = "Pause";
            else
                RunBtn.Content = "Run";
        }

        private void RandomizeBoard(object sender, RoutedEventArgs e)
        {
            int rand;
            for (int x = 0; x < m_boardRows; x++)
            {
                for (int y = 0; y < m_boardCols; y++)
                {
                    rand = m_rng.Next(1, 100);
                    if (rand <= (m_randChance / 10))
                    {
                        m_board[x][y] = Constants.Virus;
                    }
                    else if (rand <= m_randChance)
                    {
                        m_board[x][y] = Constants.Alive; 
                    }
                    else
                    {
                        m_board[x][y] = Constants.Dead;
                    }  
                }
            }
            DrawBoard();
        }

        // This runs in a separate thread and only calls Step() when the Step button is clicked.
        private void Run()
        {
            while (m_running)
            {
                if( m_step )
                {
                    Step();
                    m_step = false;
                }
                System.Threading.Thread.Sleep(1);
            }       
        }

        // Left-click handler. Places "Living" cells on the board.
        private void MainList_LeftClick(object sender, MouseEventArgs e)
        {
            
            int index = MainList.SelectedIndex;
            if (index < 0)
                return;
            ListBoxItem newItem = new ListBoxItem();
            Point myPoint = getPoint(index);
            if (m_board[myPoint.X][myPoint.Y] == Constants.Alive)
            {
                newItem.Background = Constants.DeadColor;
                m_board[myPoint.X][myPoint.Y] = Constants.Dead;
            }
            else
            {
                newItem.Background = Constants.AliveColor;
                m_board[myPoint.X][myPoint.Y] = Constants.Alive;
            }
            MainList.Items[index] = newItem;
        }

        // Right-click handler. Places "Virus" cells on the board.
        private void MainList_RightClick(object sender, MouseEventArgs e)
        {
            int index = MainList.SelectedIndex;
            if (index < 0)
                return;
            ListBoxItem newItem = new ListBoxItem();
            Point myPoint = getPoint(index);
            if (m_board[myPoint.X][myPoint.Y] == Constants.Virus)
            {
                newItem.Background = Constants.DeadColor;
                m_board[myPoint.X][myPoint.Y] = Constants.Dead;
            }
            else
            {
                newItem.Background = Constants.VirusColor;
                m_board[myPoint.X][myPoint.Y] = Constants.Virus;
            }
            MainList.Items[index] = newItem;
        }

        // Will populate the board with dead tiles with the given dimensions. Returns 0 on success.
        private int BoardInit( List<List<byte>> board, int rows, int cols)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }
            
            if( rows < Constants.MinRows || rows > Constants.MaxRows || cols < Constants.MinCols || cols > Constants.MaxCols )
            {
                return -1;
            }
            board.Clear();
            for (int row = 0 ; row < rows; ++row )
            {
                
                board.Add( new List<byte>(cols) );
                for( int col = 0; col < cols; ++col )
                {
                    board[row].Add(Constants.Dead);
                }
            }
            return 0;
        }

        // This will "step" the board one time tick into the future.
        private void Step()
        {
            // If the board is resizing we want to wait until it is done.
            if (m_resizing)
                return;

            // If we are in continuous run mode we need to stop the timer until the operation finishes and then restart it.
            bool reenableTimer = false;
            if(m_timer.Enabled)
            {
                reenableTimer = true;
                m_timer.Enabled = false;
            }

            List<List<byte>> newBoard = new List<List<byte>>();
            BoardInit(newBoard, m_boardRows, m_boardCols);

            int numNeighbors = 0;
            int numVirus = 0;

            // Will keep track of living neighbors for viruses to take.
            List<Tuple<int, int>> neighbors= new List<Tuple<int, int>>();
            Random rand = new Random(); // Randomizer for virus victim choosing.

            for (int i = 0; i < m_boardRows; ++i)
            {
                for (int j = 0; j < m_boardCols; ++j)
                {
                    numNeighbors = 0;
                    
                    // Check for living neighbors.
                    // Top Left
                    if (i != 0 && j != 0)
                    {
                        if (m_board[i - 1][j - 1] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i - 1, j - 1));
                            numNeighbors++;
                        }
                        else if (m_board[i - 1][j - 1] == Constants.Virus)
                        {
                            numVirus++;
                        }

                    }

                    // Top
                    if (i != 0)
                    {
                        if (m_board[i - 1][j] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i - 1, j));
                            numNeighbors++;
                        }
                        else if (m_board[i - 1][j] == Constants.Virus)
                        {
                            numVirus++;
                        }
                    }

                    // Top Right
                    if (i != 0 && j != m_board[0].Count - 1)
                    {
                        if (m_board[i - 1][j + 1] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i - 1, j + 1));
                            numNeighbors++;
                        }
                        else if (m_board[i - 1][j + 1] == Constants.Virus)
                        {
                            numVirus++;
                        }
                    }

                    // Left
                    if (j != 0)
                    {
                        if (m_board[i][j - 1] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i, j - 1));
                            numNeighbors++;
                        }
                        else if (m_board[i][j - 1] == Constants.Virus)
                        {
                            numVirus++;
                        }
                    }

                    // Right
                    if (j != m_board[0].Count - 1)
                    {
                        if (m_board[i][j + 1] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i, j + 1));
                            numNeighbors++;
                        }
                        else if (m_board[i][j + 1] == Constants.Virus)
                        {
                            numVirus++;
                        }
                    }

                    // Bottom Left
                    if (i != m_board.Count - 1 && j != 0)
                    {
                        if (m_board[i + 1][j - 1] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i + 1, j - 1));
                            numNeighbors++;
                        }
                        else if (m_board[i + 1][j - 1] == Constants.Virus)
                        {
                            numVirus++;
                        }
                    }

                    // Bottom
                    if (i != m_board.Count - 1 && j != 0)
                    {
                        if (m_board[i + 1][j] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i + 1, j));
                            numNeighbors++;
                        }
                        else if (m_board[i + 1][j] == Constants.Virus)
                        {
                            numVirus++;
                        }
                    }

                    // Bottom Right
                    if (i != m_board.Count - 1 && j != m_board[0].Count - 1)
                    {
                        if (m_board[i + 1][j + 1] == Constants.Alive)
                        {
                            neighbors.Add(new Tuple<int, int>(i + 1, j + 1));
                            numNeighbors++;
                        }
                        else if (m_board[i + 1][j + 1] == Constants.Virus)
                        {
                            numVirus++;
                        }
                    }

                    // If the current cell is not a virus we will use regular rules.
                    if (m_board[i][j] != Constants.Virus && newBoard[i][j] != Constants.Virus)
                    {
                        // Update this tile.
                        switch (numNeighbors)
                        {
                            // Current tile will die or stay dead.
                            case 0:
                            case 1:
                                newBoard[i][j] = Constants.Dead;
                                break;
                            // Nothing happens with two alive neighbors.
                            case 2:
                                newBoard[i][j] = m_board[i][j];
                                break;
                            // Current tile is "born"
                            case 3:
                                newBoard[i][j] = Constants.Alive;
                                break;
                            // Death by overcrowding.
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                newBoard[i][j] = Constants.Dead;
                                break;
                            default:
                                break;
                        }
                    }
                    else if(m_board[i][j] == Constants.Virus)
                    { 
                        // In the case of a virus we need to grab a victim if there is one or else die.
                        if( numNeighbors > 0)
                        {
                            // Grab a random victim.
                            Tuple<int, int> victim = neighbors[rand.Next(0, neighbors.Count)];
                            newBoard[victim.Item1][victim.Item2] = Constants.Virus;
                            newBoard[i][j] = Constants.Virus; // In the new board the current virus will also live on.
                        }
                        else
                        {
                            newBoard[i][j] = Constants.Dead;
                        }
                    }
                    neighbors.Clear();
                } // End cols
            } // End rows

            // Update the board. 
            m_board = newBoard;
            Application.Current.Dispatcher.Invoke(DrawBoard);

            // If we stopped the timer we need to restart it.
            if( reenableTimer )
                m_timer.Enabled = true;
        }

        // Redraw board.
        private void DrawBoard()
        {
            // Update listboxes based on m_board.
            int index = 0;
            for (int row = 0; row < m_boardRows; ++row)
            {
                for (int col = 0; col < m_boardCols; ++col)
                {
                    ListBoxItem newItem = new ListBoxItem();
                    if( m_board[row][col] == Constants.Virus )
                    {
                        newItem.Background = Constants.VirusColor;
                        MainList.Items[index] = newItem;
                    } 
                    else if( m_board[row][col] == Constants.Alive )
                    {
                        newItem.Background = Constants.AliveColor;
                        MainList.Items[index] = newItem;
                    }
                    else
                    {
                        MainList.Items[index] = newItem;
                    }
                    ++index;
                }
            }
        }

        // Get list box index from m_board location.
        private int GetIndex( int row, int col )
        {
            return row * m_boardCols + col;
        }

        // Get m_board location from list box index.
        private Point getPoint( int index )
        {
            int x = index / m_boardCols;
            int y = index % m_boardCols;

            Point retVal = new Point(x, y);
            
            return retVal;
        }

        // Will prompt user to save the current board configuration to a file.
        private void Save_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "gol";
            if (saveFileDialog.ShowDialog() == true)
            {
                Console.WriteLine(saveFileDialog.FileName);
                using (BinaryWriter writer = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Create)))
                {
                    writer.Write(m_boardRows);
                    writer.Write(m_boardCols);
                    for(int i = 0; i < m_boardRows; ++i)
                    {
                        for(int j = 0; j < m_boardCols; ++j)
                        {
                            writer.Write(m_board[i][j]);
                        }
                    }
                }
            }
        }

        // Will prompt user to select a previously saved board configuration from a file. Handles load errors and malformed formats.
        private void Load_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                using (BinaryReader reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open)))
                {
                    List<List<byte>> tempBoard;
                    int tempRows;
                    int tempCols;

                    // We are doing a try in case the file is smaller than expected.
                    try
                    {
                        tempRows = reader.ReadInt32();
                        tempCols = reader.ReadInt32();
                        tempBoard = new List<List<byte>>();

                        // We will attempt to initialize the board with the read in values. BoardInit will return non-zero on error.
                        int retval = BoardInit(tempBoard, tempRows, tempCols);
                        if( retval != 0 )
                        {
                            MessageBox.Show("File load error: Corrupt or malformed file.", "File Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        for (int i = 0; i < tempRows; ++i)
                        {
                            for (int j = 0; j < tempCols; ++j)
                            {
                                tempBoard[i][j] = reader.ReadByte();

                                // Sanity check of read Byte. If it's not a valid value set it to a "dead" cell.
                                if(tempBoard[i][j] < Constants.Dead || tempBoard[i][j] > Constants.Virus)
                                {
                                    tempBoard[i][j] = Constants.Dead;
                                }
                            }
                        }
                    }
                    catch(EndOfStreamException eos)
                    {
                        MessageBox.Show("File load error: Corrupt or malformed file.", "File Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    m_board = tempBoard;
                    m_boardRows = tempRows;
                    m_boardCols = tempCols;
                }

                // Fill board.
                MainList.Items.Clear();
                int numberOfTiles = m_boardRows * m_boardCols;
                for (int i = 0; i < numberOfTiles; ++i)
                {
                    ListBoxItem tile = new ListBoxItem();
                    MainList.Items.Insert(0, tile);
                }

                // Update GUI board size.
                RowTextBox.Text = m_boardRows.ToString();
                ColTextBox.Text = m_boardCols.ToString();

                // Resize main list for new board width/height.
                MainList.Height = m_boardRows * Constants.TileSize + 4;
                MainList.MaxWidth = m_boardCols * Constants.TileSize + 4;
                DrawBoard();
            }
        }

        // Handler for change the number of rows.
        private void RowTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // We will apply changes only once enter is pressed.
            if (e.Key != Key.Enter)
                return;

            // If we are running we want to stop before continuing with a resize.
            if (m_timer.Enabled)
            {
                m_timer.Enabled = false;
                RunBtn.Content = "Run";
            }

            // Parse input and sanity check.
            string newRowString = Regex.Match(RowTextBox.Text, @"\d+").Value;
            if (newRowString == "")
                return;
            int newRow = Int32.Parse(newRowString);
            if (newRow == m_boardRows)
                return;

            // Check that the input is within range. If so, resize.
            if (newRow >= Constants.MinRows && newRow <= Constants.MaxRows)
            {
                m_resizing = true;
                m_boardRows = newRow;
                RowTextBox.Text = m_boardRows.ToString();
                BoardInit(m_board, m_boardRows, m_boardCols);

                // Fill board.
                MainList.Items.Clear();
                int numberOfTiles = m_boardRows * m_boardCols;
                for (int i = 0; i < numberOfTiles; ++i)
                {
                    ListBoxItem tile = new ListBoxItem();
                    MainList.Items.Insert(0, tile);
                }

                // Resize main list for new board height.
                MainList.Height = m_boardRows * Constants.TileSize + 4;

                DrawBoard();
            }
            else
            {
                MessageBox.Show(String.Format("Row value {0} outside of range: {1} - {2}.", newRow, Constants.MinRows, Constants.MaxRows), "Board Dimensions Outside of Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            m_resizing = false;
        }

        // Handler for change the number of rows.
        private void ColTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // We will apply changes only once enter is pressed.
            if (e.Key != Key.Enter)
                return;

            // If we are running we want to stop before continuing with a resize.
            if (m_timer.Enabled)
            {
                m_timer.Enabled = false;
                RunBtn.Content = "Run";
            }

            string newColString = Regex.Match(ColTextBox.Text, @"\d+").Value;
            if (newColString == "")
                return;
            int newCol = Int32.Parse(newColString);
            if (newCol == m_boardCols)
                return;
            if (newCol >= Constants.MinCols && newCol <= Constants.MaxCols)
            {
                m_resizing = true;
                m_boardCols = newCol;
                ColTextBox.Text = m_boardCols.ToString();
                BoardInit(m_board, m_boardRows, m_boardCols);

                // Fill board.
                MainList.Items.Clear();
                int numberOfTiles = m_boardRows * m_boardCols;
                for (int i = 0; i < numberOfTiles; ++i)
                {
                    ListBoxItem tile = new ListBoxItem();
                    MainList.Items.Insert(0, tile);
                }

                // Resize main list for new board width.
                MainList.MaxWidth = m_boardCols * Constants.TileSize + 4;

                DrawBoard();
            }
            else
            {
                MessageBox.Show(String.Format("Column value {0} outside of range: {1} - {2}.", newCol, Constants.MinCols, Constants.MaxCols), "Board Dimensions Outside of Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            m_resizing = false;
        }

        // This will randomly populate our board.
        private void SeedTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // We will apply changes only once enter is pressed.
            if (e.Key != Key.Enter)
                return;

            // We need an integer value to work with.
            string seedString = Regex.Match(SeedTextBox.Text, @"\d+").Value;
            if (seedString == "")
                return;
            int seed = Int32.Parse(seedString);
            SeedTextBox.Text = "";

            // Use our seed for the random number generator.
            Random rand = new Random(seed);

            // Fill the board randomly.
            for (int row = 0; row < m_boardRows; ++row)
            {
                for (int col = 0; col < m_boardCols; ++col)
                {
                    m_board[row][col] = (byte)rand.Next(Constants.Dead, Constants.Virus + 1);
                }
            }

            // Redraw the board.
            DrawBoard();
        }

        // Handler for change the number of rows.
        private void ChanceTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Parse input and sanity check.
            string newChanceString = Regex.Match(ChanceTextBox.Text, @"\d+").Value;
            if (newChanceString == "")
                return;
            int newChance = Int32.Parse(newChanceString);
            if (newChance > 100 || newChance < 0)
                return;
            ChanceTextBox.Text = newChance.ToString();
            m_randChance = newChance;
        }

        private void MainList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                int index = MainList.SelectedIndex;
                if (index < 0)
                    return;
                ListBoxItem newItem = new ListBoxItem();
                Point myPoint = getPoint(index);
                if (m_board[myPoint.X][myPoint.Y] == Constants.Alive)
                {
                    newItem.Background = Constants.DeadColor;
                    m_board[myPoint.X][myPoint.Y] = Constants.Dead;
                }
                else
                {
                    newItem.Background = Constants.AliveColor;
                    m_board[myPoint.X][myPoint.Y] = Constants.Alive;
                }
                MainList.Items[index] = newItem;
            }
        }
    } // Class
} // Namespace
