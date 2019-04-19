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
        public const int BoardWidth = 70;
        public const int TileSize = 10;
    }

    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            int height = (int)MainGrid.Height;
            
            MainList.Height = Constants.BoardHeight * Constants.TileSize;
            MainList.MaxWidth = Constants.BoardWidth * Constants.TileSize;
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
            // TODO - step()
        }

        // Speed slider handler.
        private void SpeedSlider_Changed(object sender, RoutedEventArgs e)
        {
            // TODO - Set timer.
        }

        // Run button on-click handler.
        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO - for(timer){ step() }
        }
    }
}
