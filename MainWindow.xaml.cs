using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        private enum State
        {
            Dead = 0,
            Alive = 1,
            Virus = 2
        }

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
        private List<List<State> > m_board = new List<List<State>>();
        private int m_boardRows = Constants.BoardHeight;
        private int m_boardCols = Constants.BoardWidth;


        public MainWindow()
        {
            InitializeComponent();

            // Window will resize automatically with board size.
            this.ResizeMode = ResizeMode.NoResize;

            // Need the window to resize to our contents.
            this.SizeToContent = SizeToContent.WidthAndHeight;

            // Initialize the board data structure.
            BoardInit( Constants.BoardHeight, Constants.BoardWidth );


            //MainList.Height = Constants.BoardHeight * Constants.TileSize;
            //MainList.MaxWidth = Constants.BoardWidth * Constants.TileSize;
            
            MainGrid.Width = Constants.BoardWidth * Constants.TileSize;
            
            int numberOfTiles = Constants.BoardHeight * Constants.BoardWidth;
            for (int i = 0; i < numberOfTiles; ++i)
            {
                ListBoxItem tile = new ListBoxItem();
                MainList.Items.Insert(0, tile);
            }
        }

        // Step button on-click handler.
        private void StepBtn_Click(object sender, RoutedEventArgs e)
        {
            Step();
        }

        // Speed slider handler.
        private void SpeedSlider_Changed(object sender, RoutedEventArgs e)
        {
            // TODO - Set timer.
        }

        // Run button on-click handler.
        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO - for(timer){ Step() }
        }

        // Run button on-click handler.
        private void MainList_Click(object sender, MouseEventArgs e)
        {
            
            int index = MainList.SelectedIndex;
            ListBoxItem newItem = new ListBoxItem();
            Point myPoint = getPoint(index);
            if (m_board[myPoint.X][myPoint.Y] == State.Alive)
            {
                newItem.Background = Brushes.White;
                m_board[myPoint.X][myPoint.Y] = State.Dead;
            }
            else
            {
                newItem.Background = Brushes.Black;
                m_board[myPoint.X][myPoint.Y] = State.Alive;
            }
            MainList.Items[index] = newItem;
        }

        // Will populate the board with dead tiles with the given dimensions.
        private void BoardInit(int rows, int cols)
        {
            this.m_board.Capacity = rows;
            for(int row = 0 ; row < rows; ++row )
            {
                this.m_board.Add( new List<State>(cols) );
                for( int col = 0; col < cols; ++col )
                {
                    this.m_board[row].Add(State.Dead);
                }
            }
        }

        // This will "step" the board one time tick into the future.
        private void Step()
        {
            int numNeighbors = 0;
            for (int i = 0; i < m_board.Capacity; ++i)
            {
                for (int j = 0; j < m_board[0].Capacity; ++j)
                {
                    numNeighbors = 0;
                    
                    // Check for living neighbors.
                    if( i != 0 && j != 0 && m_board[i - 1][j - 1] == State.Alive   )
                    {
                        numNeighbors++;
                    }
                    if( i != 0  && m_board[i - 1][j] == State.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != 0 && j != m_board[0].Count - 1 && m_board[i - 1][j + 1] == State.Alive )
                    {
                        numNeighbors++;
                    }
                    if( j != 0 && m_board[i][j - 1] == State.Alive )
                    {
                        numNeighbors++;
                    }
                    if( j != m_board[0].Count - 1 && m_board[i][j + 1] == State.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != m_board.Count - 1 && j != 0 && m_board[i + 1][j - 1] == State.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != m_board.Count - 1 && j != 0 && m_board[i + 1][j] == State.Alive )
                    {
                        numNeighbors++;
                    }
                    if( i != m_board.Count - 1 && j != m_board[0].Count - 1 && m_board[i + 1][j + 1] == State.Alive )
                    {
                        numNeighbors++;
                    }

                    // Update this tile.
                    switch(numNeighbors)
                    {
                        // Current tile will die or stay dead.
                        case 0:
                        case 1:
                            m_board[i][j] = State.Dead;
                            break;
                        // Nothing happens with two alive neighbors.
                        case 2:
                            break;
                        // Current tile is "born"
                        case 3:
                            m_board[i][j] = State.Alive;
                            break;
                        // Death by overcrowding.
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            m_board[i][j] = State.Dead;
                            break;
                        default:
                            break;
                    }
                } // End cols
            } // End rows

            // Update the board. 
            DrawBoard();
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
                    if( m_board[row][col] == State.Dead )
                    {
                        MainList.Items[index] = newItem;
                    } 
                    else if( m_board[row][col] == State.Alive )
                    {
                        newItem.Background = Brushes.Black;
                        MainList.Items[index] = newItem;
                    }
                    else
                    {
                        newItem.Background = Brushes.Green;
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
