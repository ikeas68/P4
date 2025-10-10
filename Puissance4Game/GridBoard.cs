using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Puissance4Game
{
    public class GridBoard : Grid
    {
        static GridBoard()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(GridBoard),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (s, e) => (s as GridBoard)?.UpdateColRow()));
        }

        public GridBoard()
        {
            Loaded += (s, e) => UpdateColRow();
            Templates.CollectionChanged += (s, e) => UpdateColRow();
        }

        #region NbColumns (DP SHORT)
        public int NbColumns { get { return (int)GetValue(NbColumnsProperty); } set { SetValue(NbColumnsProperty, value); } }
        public static readonly DependencyProperty NbColumnsProperty = DependencyProperty.Register(
            "NbColumns", typeof(int), typeof(GridBoard),
            new FrameworkPropertyMetadata(7,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                (s, e) => (s as GridBoard)?.UpdateColRow()));
        #endregion

        #region NbRows (DP SHORT)
        public int NbRows { get { return (int)GetValue(NbRowsProperty); } set { SetValue(NbRowsProperty, value); } }
        public static readonly DependencyProperty NbRowsProperty = DependencyProperty.Register(
            "NbRows", typeof(int), typeof(GridBoard),
            new FrameworkPropertyMetadata(6,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, 
                (s, e) => (s as GridBoard)?.UpdateColRow()));
        #endregion

        #region SelectedTemplate (DP SHORT)
        public int SelectedTemplate { get { return (int)GetValue(SelectedTemplateProperty); } set { SetValue(SelectedTemplateProperty, value); } }
        public static readonly DependencyProperty SelectedTemplateProperty = DependencyProperty.Register(
            "SelectedTemplate", typeof(int), typeof(GridBoard), 
            new FrameworkPropertyMetadata(0, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, 
                (s, e) => (s as GridBoard)?.UpdateColRow()));
        #endregion

        #region Templates (collection de DataTemplate)
        public ObservableCollection<DataTemplate> Templates { get; } = new ObservableCollection<DataTemplate>();
        #endregion

        #region UpdateRoxCol
        private void UpdateColRow()
        {
            Children.Clear();
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();
            if (NbRows > 0 && NbColumns > 0 && SelectedTemplate >= 0 && SelectedTemplate <= Templates.Count() - 1)
            {
                #region create row col
                for (var row = 0; row < NbRows; row++)
                {
                    RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                }
                for (var column = 0; column < NbColumns; column++)
                {
                    ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                }
                #endregion

                #region set template in row col
                for (var row = 0; row < NbRows; row++)
                {
                    for (var column = 0; column < NbColumns; column++)
                    {
                        var cell = new Grid
                        {
                            //Margin = new Thickness(4),
                            IsHitTestVisible = false,
                            Background = this.Background,
                        };

                        var content = BuildCellContent();
                        if (content != null)
                            cell.Children.Add(content);
                      
                        Grid.SetRow(cell, row);
                        Grid.SetColumn(cell, column);
                        Children.Add(cell);
                    }
                }
                #endregion
            }
        }

        private UIElement BuildCellContent()
        {            
            var obj = Templates[SelectedTemplate].LoadContent();   // object
            if (obj is UIElement ui) return ui;
            else return new ContentPresenter { Content = obj };
        }

        #endregion
    }
}
