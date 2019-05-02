using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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



namespace ConwayGameOfLife
{
    static class Constants
    {
        public const int BoardHeight = 40;
        public const int BoardWidth = 80;
        public const int TileSize = 10;

        public const byte Dead = 0;
        public const byte Alive = 1;
        public const byte Virus = 2;

        public const string AliveImage = "Resources/Bacteria.jpg";
        public const string VirusImage = "Resources/Virus.jpg";
    }

    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
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

        //
        // Data Members
        //
        private List<List<byte> > m_board = new List<List<byte>>();
        private int m_boardRows = Constants.BoardHeight;
        private int m_boardCols = Constants.BoardWidth;

        private bool m_running = true;
        private bool m_step = false;


        private System.Timers.Timer m_timer = new System.Timers.Timer();
        private int m_speed = 1000; // In milliseconds.
        

        private Thread boardThread;


        public MainWindow()
        {
            InitializeComponent();

            // Window will resize automatically with board size.
            this.ResizeMode = ResizeMode.NoResize;

            // Need the window to resize to our contents.
            this.SizeToContent = SizeToContent.WidthAndHeight;

            // Initialize the board data structure.
            BoardInit( m_board, Constants.BoardHeight, Constants.BoardWidth );
            
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
            
            // Timer for when the auto-run is selected.
            m_timer.Elapsed += RunEvent;
            m_timer.Enabled = false;
            m_timer.AutoReset = true;
            m_timer.Interval = m_speed;
            SpeedSlider.Value = m_speed;

            // Start our worker thread.
            boardThread = new Thread(Run);
            boardThread.SetApartmentState(ApartmentState.STA);
            boardThread.Start();
        }

        private void RunEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Step();
        }

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
        }

        // Run button on-click handler.
        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            m_timer.Enabled = !m_timer.Enabled;
        }

        private void Run()
        {
            while (m_running)
            {
                if( m_step )
                {
                    Step();
                    m_step = false;
                }
            }       
        }

        // Run button on-click handler.
        private void MainList_Click(object sender, MouseEventArgs e)
        {
            
            int index = MainList.SelectedIndex;
            ListBoxItem newItem = new ListBoxItem();
            Point myPoint = getPoint(index);
            if (m_board[myPoint.X][myPoint.Y] == Constants.Alive)
            {
                newItem.Background = Brushes.White;
                m_board[myPoint.X][myPoint.Y] = Constants.Dead;
            }
            else
            {
                newItem.Background = Brushes.Black;
                m_board[myPoint.X][myPoint.Y] = Constants.Alive;
            }
            MainList.Items[index] = newItem;
        }

        // Will populate the board with dead tiles with the given dimensions.
        private void BoardInit( List<List<byte>> board, int rows, int cols)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            board.Capacity = rows;
            for(int row = 0 ; row < rows; ++row )
            {
                board.Add( new List<byte>(cols) );
                for( int col = 0; col < cols; ++col )
                {
                    board[row].Add(Constants.Dead);
                }
            }
        }

        // This will "step" the board one time tick into the future.
        private void Step()
        {
            List<List<byte>> newBoard = new List<List<byte>>();
            BoardInit(newBoard, m_boardRows, m_boardCols);

            int numNeighbors = 0;
            for (int i = 0; i < m_board.Capacity; ++i)
            {
                for (int j = 0; j < m_board[0].Capacity; ++j)
                {
                    numNeighbors = 0;
                    
                    // Check for living neighbors.
                    if( i != 0 && j != 0 && m_board[i - 1][j - 1] == Constants.Alive   )
                    {
                        numNeighbors++;
                    }
                    if( i != 0  && m_board[i - 1][j] == Constants.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != 0 && j != m_board[0].Count - 1 && m_board[i - 1][j + 1] == Constants.Alive )
                    {
                        numNeighbors++;
                    }
                    if( j != 0 && m_board[i][j - 1] == Constants.Alive )
                    {
                        numNeighbors++;
                    }
                    if( j != m_board[0].Count - 1 && m_board[i][j + 1] == Constants.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != m_board.Count - 1 && j != 0 && m_board[i + 1][j - 1] == Constants.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != m_board.Count - 1 && j != 0 && m_board[i + 1][j] == Constants.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != m_board.Count - 1 && j != m_board[0].Count - 1 && m_board[i + 1][j + 1] == Constants.Alive )
                    {
                        numNeighbors++;
                    }

                    // Update this tile.
                    switch(numNeighbors)
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
                } // End cols
            } // End rows

            // Update the board. 
            m_board = newBoard;
            Application.Current.Dispatcher.Invoke(DrawBoard);
        }

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
                        newItem.Background = Brushes.Green;
                        MainList.Items[index] = newItem;
                    } 
                    else if( m_board[row][col] == Constants.Alive )
                    {
                        newItem.Background = Brushes.Black;
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
    } // Class
} // Namespace
